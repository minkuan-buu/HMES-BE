using System.Net;
using System.Text;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CartRepositories;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Repositories.DeviceRepositories;
using HMES.Data.Repositories.OrderDetailRepositories;
using HMES.Data.Repositories.OrderRepositories;
using HMES.Data.Repositories.ProductRepositories;
using HMES.Data.Repositories.TransactionRepositories;
using HMES.Data.Repositories.UserAddressRepositories;
using HMES.Data.Repositories.UserRepositories;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using System.Text.Json;

using Transaction = HMES.Data.Entities.Transaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MimeKit.Text;

namespace HMES.Business.Services.OrderServices
{
    public class OrderServices : IOrderServices
    {
        private readonly ILogger<OrderServices> _logger;
        private readonly IUserRepositories _userRepositories;
        private readonly IMapper _mapper;
        private readonly IOrderRepositories _orderRepositories;
        private readonly IOrderDetailRepositories _orderDetailRepositories;
        private readonly ITransactionRepositories _transactionRepositories;
        private readonly ICartRepositories _cartRepositories;
        private readonly ICartItemsRepositories _cartItemsRepositories;
        private readonly IUserAddressRepositories _userAddressRepositories;
        private readonly IDeviceRepositories _deviceRepositories;
        private readonly IDeviceItemsRepositories _deviceItemsRepositories;
        private readonly IProductRepositories _productRepositories;
        private PayOS _payOS;
        private readonly string _ghnToken = Environment.GetEnvironmentVariable("GHN_TOKEN");
        private readonly int _shopId = int.Parse(Environment.GetEnvironmentVariable("GHN_ID_SHOP"));
        private readonly string PayOSClientId = Environment.GetEnvironmentVariable("PAY_OS_CLIENT_ID");
        private readonly string PayOSAPIKey = Environment.GetEnvironmentVariable("PAY_OS_API_KEY");
        private readonly string PayOSChecksumKey = Environment.GetEnvironmentVariable("PAY_OS_CHECKSUM_KEY");
        private readonly HttpClient _httpClient;

        public OrderServices(ILogger<OrderServices> logger, IUserRepositories userRepositories, IMapper mapper, IOrderRepositories orderRepositories, IOrderDetailRepositories orderDetailRepositories, ITransactionRepositories transactionRepositories, ICartRepositories cartRepositories, IUserAddressRepositories userAddressRepositories, IDeviceRepositories deviceRepositories, IProductRepositories productRepositories, IDeviceItemsRepositories deviceItemsRepositories, ICartItemsRepositories cartItemsRepositories)
        {
            _payOS = new PayOS(PayOSClientId, PayOSAPIKey, PayOSChecksumKey);
            _logger = logger;
            _userRepositories = userRepositories;
            _mapper = mapper;
            _orderRepositories = orderRepositories;
            _orderDetailRepositories = orderDetailRepositories;
            _transactionRepositories = transactionRepositories;
            _cartRepositories = cartRepositories;
            _userAddressRepositories = userAddressRepositories;
            _deviceRepositories = deviceRepositories;
            _productRepositories = productRepositories;
            _deviceItemsRepositories = deviceItemsRepositories;
            _cartItemsRepositories = cartItemsRepositories;
            _httpClient = new HttpClient();
        }

        public async Task<String> CreatePaymentUrl(string Token, Guid Id)
        {
            var userId = new Guid(Authentication.DecodeToken(Token, "userid"));
            var order = await _orderRepositories.GetSingle(
                x => x.Id.Equals(Id) && x.UserId.Equals(userId) && (x.Status.Equals(OrderEnums.Pending.ToString()) || x.Status.Equals(OrderEnums.AllowRepayment.ToString())),
                includeProperties: "Transactions,OrderDetails.Product,OrderDetails.Device,UserAddress"
            );

            if (order.UserAddressId == null)
            {
                throw new CustomException("Người dùng chưa có địa chỉ cho đơn hàng.");
            }

            // Kiểm tra giao dịch PENDING trước
            var pendingTransaction = order.Transactions.FirstOrDefault(x => x.OrderId.Equals(Id) && x.Status.Equals(TransactionEnums.PENDING.ToString()));
            if (pendingTransaction != null)
            {
                return $"https://pay.payos.vn/web/{pendingTransaction.PaymentLinkId}";
            }

            // Lấy danh sách sản phẩm và thiết bị trong đơn hàng
            var productIds = order.OrderDetails.Where(od => od.ProductId != null).Select(od => od.ProductId).ToList();
            var deviceIds = order.OrderDetails.Where(od => od.DeviceId != null).Select(od => od.DeviceId).ToList();

            var productDetails = order.OrderDetails.Where(od => od.ProductId != null).ToList();
            var deviceDetails = order.OrderDetails.Where(od => od.DeviceId != null).ToList();

            var products = await _productRepositories.GetList(p => productIds.Contains(p.Id));
            var devices = await _deviceRepositories.GetList(d => deviceIds.Contains(d.Id));

            int districtId = await GetDistrictId(TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Province), TextConvert.ConvertFromUnicodeEscape(order.UserAddress.District));
            string wardId = await GetWardId(districtId, TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward));
            int serviceId = await GetService(districtId);

            // Kiểm tra tổng số lượng đã đặt nhưng chưa thanh toán
            foreach (var od in order.OrderDetails)
            {
                if (od.ProductId != null)
                {
                    var product = products.FirstOrDefault(p => p.Id == od.ProductId);
                    if (product == null || product.Amount < od.Quantity)
                    {
                        throw new CustomException($"Sản phẩm {od.ProductId} không đủ số lượng để thanh toán.");
                    }
                }
                else if (od.DeviceId != null)
                {
                    var device = devices.FirstOrDefault(d => d.Id == od.DeviceId);
                    if (device == null || device.Quantity < od.Quantity)
                    {
                        throw new CustomException($"Thiết bị {od.DeviceId} không đủ số lượng để thanh toán.");
                    }
                }
            }

            var ghnOrder = new
            {
                token = _ghnToken,
                shop_id = _shopId,
                required_note = "CHOXEMHANGKHONGTHU",
                from_name = "HMES",
                from_address = "117 Xô Viết Nghệ Tĩnh, Phường 17, Quận Bình Thạnh,TP. Hồ Chí Minh",
                from_province_name = "TP. Hồ Chí Minh",
                from_district_name = "Bình Thạnh",
                from_ward_name = "Phường 17",
                to_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Name),
                to_phone = order.UserAddress.Phone,
                to_address = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Address),
                to_ward_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward),
                to_district_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.District),
                to_province_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Province),
                cod_amount = 0,
                weight = 500,
                length = 20,
                width = 50,
                height = 30,
                service_type_id = serviceId,
                payment_type_id = 2,
                items = productDetails
                        .Select(p => new
                        {
                            name = p.Product?.Name ?? "Sản phẩm",
                            code = p.ProductId,
                            quantity = p.Quantity,
                            price = (int)p.UnitPrice,
                            weight = 500,
                            length = 20,
                            width = 50,
                            height = 30,
                        })
                        .Concat(deviceDetails.Select(d => new
                        {
                            name = d.Device?.Name ?? "Thiết bị",
                            code = d.DeviceId,
                            quantity = d.Quantity,
                            price = (int)d.UnitPrice,
                            weight = 500,
                            length = 20,
                            width = 50,
                            height = 30,
                        }))
                        .ToArray()
            };

            int weightOfOrder = ghnOrder.items.Sum(item => item.weight * item.quantity);
            ghnOrder = new
            {
                ghnOrder.token,
                ghnOrder.shop_id,
                ghnOrder.required_note,
                ghnOrder.from_name,
                ghnOrder.from_address,
                ghnOrder.from_province_name,
                ghnOrder.from_district_name,
                ghnOrder.from_ward_name,
                ghnOrder.to_name,
                ghnOrder.to_phone,
                ghnOrder.to_address,
                ghnOrder.to_ward_name,
                ghnOrder.to_district_name,
                ghnOrder.to_province_name,
                ghnOrder.cod_amount,
                weight = weightOfOrder,
                ghnOrder.length,
                ghnOrder.width,
                ghnOrder.height,
                ghnOrder.service_type_id,
                ghnOrder.payment_type_id,
                ghnOrder.items
            };

            int shippingFee = await CalculateShippingFee(ghnOrder);

            // Tạo link thanh toán mới
            var OrderPaymentRefId = int.Parse(GenerateRandomRefId());
            List<ItemData> itemDatas = new();

            foreach (var item in order.OrderDetails)
            {
                string itemName = item.DeviceId != null
                    ? $"{TextConvert.ConvertFromUnicodeEscape(item.Device.Name)} ({TextConvert.ConvertFromUnicodeEscape(item.Device.Name)})"
                    : $"{TextConvert.ConvertFromUnicodeEscape(item.Product.Name)} ({TextConvert.ConvertFromUnicodeEscape(item.Product.Name)})";

                itemDatas.Add(new ItemData(itemName, item.Quantity, (int)item.UnitPrice));
            }

            itemDatas.Add(new ItemData("Phí vận chuyển", 1, shippingFee));

            string returnURL = Environment.GetEnvironmentVariable("PAYMENT_RETURN_URL") ?? throw new Exception("PAYMENT_RETURN_URL is missing");
            DateTime paymentExpireDate = DateTime.Now.AddMinutes(15);

            PaymentData paymentData = new PaymentData(OrderPaymentRefId, (int)order.TotalPrice + shippingFee, "",
                itemDatas, cancelUrl: returnURL, returnUrl: returnURL, expiredAt: new DateTimeOffset(paymentExpireDate).ToUnixTimeSeconds());

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

            Transaction NewTransaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OrderPaymentRefId = OrderPaymentRefId,
                Status = TransactionEnums.PENDING.ToString(),
                PaymentLinkId = createPayment.paymentLinkId,
                CreatedAt = DateTime.Now,
                PaymentMethod = PaymentMethodEnums.BANK.ToString(),
            };

            await _transactionRepositories.Insert(NewTransaction);

            order.Status = OrderEnums.PendingPayment.ToString();

            await _orderRepositories.Update(order);

            // **Trừ số lượng sản phẩm sau khi đã tạo giao dịch thành công**
            foreach (var od in order.OrderDetails)
            {
                if (od.ProductId != null)
                {
                    var product = products.First(p => p.Id == od.ProductId);
                    product.Amount -= od.Quantity;
                    await _productRepositories.Update(product);
                }
                else if (od.Device != null)
                {
                    od.Device.Quantity -= od.Quantity;
                    await _deviceRepositories.Update(od.Device); // Dùng trực tiếp từ order
                }
            }

            return createPayment.checkoutUrl;
        }


        private string GenerateRandomRefId()
        {
            var random = new Random();
            return random.Next(10000000, 99999999).ToString();
        }

        public async Task<ResultModel<DataResultModel<Guid>>> CreateOrder(CreateOrderDetailReqModel orderRequest, string token)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));

                // Lấy địa chỉ mặc định của user
                var userAddress = await _userAddressRepositories.GetSingle(
                    ua => ua.UserId == userId && ua.Status == "Default");

                // Lấy đơn hàng Pending hiện có của user (nếu có)
                var existingOrder = await _orderRepositories.GetSingle(
                    o => o.UserId == userId && o.Status == OrderEnums.Pending.ToString(),
                    includeProperties: "OrderDetails,Transactions");

                // Xóa OrderDetail của đơn hàng cũ nếu có
                if (existingOrder != null)
                {
                    await _orderDetailRepositories.DeleteRange(existingOrder.OrderDetails.ToList());
                    existingOrder.OrderDetails.Clear();
                }

                // Xác định orderId
                Guid orderId = existingOrder != null ? existingOrder.Id : Guid.NewGuid();

                // Tạo danh sách OrderDetail mới
                List<OrderDetail> newOrderDetails = new List<OrderDetail>();

                // Thêm OrderDetail cho sản phẩm
                foreach (var prodReq in orderRequest.Products)
                {
                    var orderDetail = new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        ProductId = prodReq.Id,
                        UnitPrice = prodReq.UnitPrice,
                        Quantity = prodReq.Quantity,
                        Status = OrderEnums.Pending.ToString(),
                        CreatedAt = DateTime.Now
                    };
                    newOrderDetails.Add(orderDetail);
                }

                // Thêm OrderDetail cho thiết bị
                foreach (var devReq in orderRequest.Devices)
                {
                    var orderDetail = new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        DeviceId = devReq.Id,
                        UnitPrice = devReq.UnitPrice,
                        Quantity = devReq.Quantity,
                        Status = OrderEnums.Pending.ToString(),
                        CreatedAt = DateTime.Now
                    };
                    newOrderDetails.Add(orderDetail);
                }

                // Tính tổng giá trị đơn hàng
                decimal totalPrice = newOrderDetails.Sum(od => od.UnitPrice * od.Quantity);

                Order order;
                if (existingOrder != null)
                {
                    // Cập nhật đơn hàng Pending
                    await _orderDetailRepositories.DeleteRange(existingOrder.OrderDetails.ToList());
                    existingOrder.OrderDetails.Clear();

                    foreach (var od in newOrderDetails)
                    {
                        existingOrder.OrderDetails.Add(od);
                    }

                    existingOrder.TotalPrice = totalPrice;
                    existingOrder.UpdatedAt = DateTime.Now;
                    await _orderRepositories.Update(existingOrder);
                    order = existingOrder;
                }
                else
                {
                    // Tạo mới đơn hàng
                    order = new Order
                    {
                        Id = orderId,
                        UserId = userId,
                        UserAddressId = userAddress != null ? userAddress.Id : null, // Gán UserAddressId vào đơn hàng
                        TotalPrice = totalPrice,
                        Status = OrderEnums.Pending.ToString(),
                        CreatedAt = DateTime.Now,
                        OrderDetails = newOrderDetails
                    };
                    await _orderRepositories.Insert(order);
                }

                return new ResultModel<DataResultModel<Guid>>()
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new DataResultModel<Guid>()
                    {
                        Data = order.Id
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        private async Task CreateShippingGHN(Order order, string paymentMethod)
        {
            try
            {
                if (order == null)
                    throw new CustomException("Không tìm thấy đơn hàng.");

                // Tách sản phẩm và thiết bị từ OrderDetails
                var productDetails = order.OrderDetails.Where(od => od.ProductId != null).ToList();
                var deviceDetails = order.OrderDetails.Where(od => od.DeviceId != null).ToList();

                int codAmount = (int)productDetails.Sum(p => p.UnitPrice * p.Quantity)
                               + (int)deviceDetails.Sum(d => d.UnitPrice * d.Quantity);

                int districtId = await GetDistrictId(TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Province), TextConvert.ConvertFromUnicodeEscape(order.UserAddress.District));
                string wardId = await GetWardId(districtId, TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward));
                int serviceId = await GetService(districtId);

                var ghnOrder = new
                {
                    token = _ghnToken,
                    shop_id = _shopId,
                    required_note = "CHOXEMHANGKHONGTHU",
                    from_name = "HMES",
                    from_address = "117 Xô Viết Nghệ Tĩnh, Phường 17, Quận Bình Thạnh,TP. Hồ Chí Minh",
                    from_province_name = "TP. Hồ Chí Minh",
                    from_district_name = "Bình Thạnh",
                    from_ward_name = "Phường 17",
                    to_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Name),
                    to_phone = order.UserAddress.Phone,
                    to_address = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Address),
                    to_ward_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward),
                    to_district_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.District),
                    to_province_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Province),
                    cod_amount = codAmount,
                    weight = 500,
                    length = 20,
                    width = 50,
                    height = 30,
                    service_type_id = serviceId,
                    payment_type_id = paymentMethod == "Banking" ? 1 : 2,
                    items = productDetails
                        .Select(p => new
                        {
                            name = TextConvert.ConvertFromUnicodeEscape(p.Product?.Name) ?? "Sản phẩm",
                            code = p.ProductId,
                            quantity = p.Quantity,
                            price = (int)p.UnitPrice,
                            weight = 500,
                            length = 20,
                            width = 50,
                            height = 30,
                        })
                        .Concat(deviceDetails.Select(d => new
                        {
                            name = TextConvert.ConvertFromUnicodeEscape(d.Device?.Name) ?? "Thiết bị",
                            code = d.DeviceId,
                            quantity = d.Quantity,
                            price = (int)d.UnitPrice,
                            weight = 500,
                            length = 20,
                            width = 50,
                            height = 30,
                        }))
                        .ToArray()
                };


                int weightOfOrder = ghnOrder.items.Sum(item => item.weight * item.quantity);
                ghnOrder = new
                {
                    ghnOrder.token,
                    ghnOrder.shop_id,
                    ghnOrder.required_note,
                    ghnOrder.from_name,
                    ghnOrder.from_address,
                    ghnOrder.from_province_name,
                    ghnOrder.from_district_name,
                    ghnOrder.from_ward_name,
                    ghnOrder.to_name,
                    ghnOrder.to_phone,
                    ghnOrder.to_address,
                    ghnOrder.to_ward_name,
                    ghnOrder.to_district_name,
                    ghnOrder.to_province_name,
                    ghnOrder.cod_amount,
                    weight = weightOfOrder,
                    ghnOrder.length,
                    ghnOrder.width,
                    ghnOrder.height,
                    ghnOrder.service_type_id,
                    ghnOrder.payment_type_id,
                    ghnOrder.items
                };

                //int shippingFee = await CalculateShippingFee(_ghnToken, ghnOrder.items, weightOfOrder, _shopId, districtId, wardId.ToString(), codAmount);

                string ghnResponse = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create", ghnOrder);
                var responseObject = JsonConvert.DeserializeObject<dynamic>(ghnResponse);

                if (responseObject == null || responseObject.code != 200)
                {
                    var message = responseObject?.message?.ToString();
                    if (!string.IsNullOrEmpty(message) && message.Contains("Too many request. This request is processing", StringComparison.OrdinalIgnoreCase))
                    {
                        if (responseObject?.data is JObject dataObj && dataObj["order_code"] != null)
                        {
                            order.ShippingOrderCode = dataObj["order_code"]?.ToString();
                            order.UpdatedAt = DateTime.Now;
                            order.Status = OrderEnums.Delivering.ToString();
                            await _orderRepositories.Update(order);
                            return;
                        }
                    }
                    else
                    {
                        throw new CustomException("Không thể tạo đơn hàng trên GHN: " + (responseObject?.message ?? "Lỗi không xác định."));
                    }
                }
                else if (responseObject?.data is JObject dataObj && dataObj["order_code"] != null)
                {
                    order.ShippingOrderCode = dataObj["order_code"]?.ToString();
                    order.UpdatedAt = DateTime.Now;
                    order.Status = OrderEnums.Delivering.ToString();
                    await _orderRepositories.Update(order);
                }

            }
            catch (Exception ex)
            {
                throw new CustomException($"Lỗi khi tạo đơn hàng GHN: {ex.Message}");
            }
        }


        private async Task<int> GetDistrictId(string province, string district)
        {
            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district", new { province });
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            foreach (var d in result.data)
            {
                if ((string)d.DistrictName == district)
                    return (int)d.DistrictID;
            }

            return 0;
        }


        private async Task<string> GetWardId(int districtId, string ward)
        {
            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward", new { district_id = districtId });
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            foreach (var w in result.data)
            {
                if ((string)w.WardName == ward)
                    return w.WardCode;
            }

            return "";
        }

        private async Task<int> GetService(int districtId)
        {
            var requestBody = new
            {
                shop_id = _shopId,
                from_district = 1462,  // Mã quận/huyện của shop
                to_district = districtId
            };

            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/available-services", requestBody);
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            if (result == null || result.data == null)
                return 0;

            var services = (result.data as JArray)?.ToObject<List<dynamic>>();

            var matchedService = services?.FirstOrDefault();

            return matchedService?["service_type_id"] != null ? (int)matchedService["service_type_id"] : 0;
        }

        private async Task<int> CalculateShippingFee(object sample)
        {
            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/preview", sample);
            var result = JsonConvert.DeserializeObject<dynamic>(response);
            return result.data != null && result.data["total_fee"] != null ? (int)result.data["total_fee"] : 0;
        }

        private async Task<string> SendPostRequest(string url, object data)
        {
            var requestContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Token", _ghnToken);
            _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId.ToString());

            var response = await _httpClient.PostAsync(url, requestContent);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<ResultModel<DataResultModel<OrderPaymentResModel>>> HandleCheckTransaction(string id, string token)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));

                // Lấy giao dịch đang chờ xử lý theo PaymentLinkId
                var transaction = await _transactionRepositories.GetSingle(
                    x => x.PaymentLinkId.Equals(id),
                    includeProperties: "Order.OrderDetails.Product,Order.OrderDetails.Device,Order.UserAddress,Order.DeviceItems"
                );

                if (transaction == null)
                    throw new CustomException("Transaction not found");

                if (transaction.Order == null)
                    throw new CustomException("Order not found");

                var orderResModel = new OrderPaymentResModel()
                {
                    Id = transaction.Order.Id,
                    OrderPrice = transaction.Order.TotalPrice,
                    ShippingPrice = transaction.Order.ShippingFee ?? 0,
                    TotalPrice = transaction.Order.TotalPrice + (transaction.Order.ShippingFee ?? 0),
                    StatusPayment = transaction.Status,
                    UserAddress = transaction.Order.UserAddress != null ? new OrderUserAddress
                    {
                        Id = transaction.Order.UserAddress.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(transaction.Order.UserAddress.Name),
                        Phone = transaction.Order.UserAddress.Phone,
                        Address = TextConvert.ConvertFromUnicodeEscape(transaction.Order.UserAddress.Address),
                        IsDefault = transaction.Order.UserAddress.Status.Equals(UserAddressEnums.Default.ToString())
                    } : null,
                    OrderProductItem = transaction.Order.OrderDetails.Where(x => x.DeviceId == null).Select(detail => new OrderDetailResModel
                    {
                        Id = detail.Id,
                        ProductName = detail.Product != null ? TextConvert.ConvertFromUnicodeEscape(detail.Product.Name) : null,
                        Attachment = detail.Product?.ProductAttachments.FirstOrDefault()?.Attachment ?? detail.Device?.Attachment ?? detail.Product?.MainImage ?? "",
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice
                    }).ToList()
                };

                if (!transaction.Status.Equals(TransactionEnums.PENDING.ToString()))
                {
                    foreach (var deviceItem in transaction.Order.DeviceItems)
                    {
                        orderResModel.OrderProductItem.Add(new OrderDetailResModel()
                        {
                            Id = deviceItem.DeviceId,
                            ProductName = TextConvert.ConvertFromUnicodeEscape(transaction.Order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).Device?.Name),
                            Attachment = transaction.Order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).Device?.Attachment ?? deviceItem.Device?.Attachment ?? "",
                            Quantity = 1,
                            UnitPrice = transaction.Order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).UnitPrice,
                            Serial = deviceItem.Serial,
                        });
                    }
                    return new ResultModel<DataResultModel<OrderPaymentResModel>>
                    {
                        StatusCodes = (int)HttpStatusCode.OK,
                        Response = new DataResultModel<OrderPaymentResModel> { Data = orderResModel }
                    };
                }

                // Lấy thông tin thanh toán từ cổng thanh toán
                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(transaction.OrderPaymentRefId);

                if (paymentLinkInformation == null)
                    throw new CustomException("Transaction not found in payment system");

                if (paymentLinkInformation.status.Equals(TransactionEnums.PAID.ToString()))
                {
                    var getTransaction = paymentLinkInformation.transactions.FirstOrDefault();
                    transaction.TransactionReference = getTransaction.reference;
                    transaction.FinishedTransactionAt = DateTime.Parse(getTransaction.transactionDateTime);
                    orderResModel.StatusPayment = TransactionEnums.PAID.ToString();
                    transaction.Status = TransactionEnums.PAID.ToString();
                    transaction.Order.Status = OrderEnums.Delivering.ToString();
                    transaction.Order.UpdatedAt = DateTime.Now;

                    await CreateShippingGHN(transaction.Order, "Banking");

                    // Xóa giỏ hàng sau khi thanh toán

                    // Lấy cart items của user từ DB (hoặc theo orderId nếu bạn đang lọc theo order)
                    var cart = await _cartRepositories.GetSingle(x => x.UserId.Equals(userId), includeProperties: "CartItems");

                    // Giả sử bạn đã có transaction.Order.OrderDetails như trước
                    var cartItemFromTransaction = transaction.Order.OrderDetails
                        .Select(od => new { od.ProductId, od.Quantity })
                        .ToList();

                    // Áp dụng logic kiểm tra số lượng
                    var itemsToDelete = new List<CartItem>();
                    var itemsToUpdate = new List<CartItem>();

                    foreach (var cartItem in cart.CartItems)
                    {
                        var productInTransaction = cartItemFromTransaction.FirstOrDefault(ct => ct.ProductId == cartItem.ProductId);
                        if (productInTransaction != null)
                        {
                            if (cartItem.Quantity <= productInTransaction.Quantity)
                            {
                                itemsToDelete.Add(cartItem);
                            }
                            else
                            {
                                cartItem.Quantity -= productInTransaction.Quantity;
                                itemsToUpdate.Add(cartItem);
                            }
                        }
                    }
                    await _cartItemsRepositories.UpdateRange(itemsToUpdate);
                    await _cartItemsRepositories.DeleteRange(itemsToDelete);
                    var newDeviceItem = await CreateDeviceItem(transaction.Order);
                    foreach (var deviceItem in newDeviceItem)
                    {
                        orderResModel.OrderProductItem.Add(new OrderDetailResModel()
                        {
                            Id = deviceItem.Id,
                            ProductName = TextConvert.ConvertFromUnicodeEscape(transaction.Order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).Device?.Name),
                            Attachment = transaction.Order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).Device?.Attachment ?? deviceItem.Device?.Attachment ?? "",
                            Quantity = 1,
                            UnitPrice = transaction.Order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).UnitPrice,
                            Serial = deviceItem.Serial
                        });
                    }
                }
                else if (paymentLinkInformation.status.Equals(TransactionEnums.CANCELLED.ToString()))
                {
                    transaction.Status = TransactionEnums.CANCELLED.ToString();
                    transaction.Order.Status = OrderEnums.Cancelled.ToString();
                    transaction.Order.UpdatedAt = DateTime.Now;
                    orderResModel.StatusPayment = TransactionEnums.CANCELLED.ToString();

                    //Hoàn trả số lượng sản phẩm & thiết bị nếu đơn hàng bị hủy
                    foreach (var orderDetail in transaction.Order.OrderDetails)
                    {
                        if (orderDetail.Product != null)
                        {
                            orderDetail.Product.Amount += orderDetail.Quantity;
                            await _productRepositories.Update(orderDetail.Product);
                        }
                        if (orderDetail.Device != null)
                        {
                            orderDetail.Device.Quantity += orderDetail.Quantity;
                            await _deviceRepositories.Update(orderDetail.Device);
                        }
                    }
                }
                await _transactionRepositories.Update(transaction);
                await _orderRepositories.Update(transaction.Order);

                return new ResultModel<DataResultModel<OrderPaymentResModel>>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new DataResultModel<OrderPaymentResModel> { Data = orderResModel }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<ResultModel<ListDataResultModel<OrderResModel>>> GetOrderList(string? keyword, decimal? minPrice, decimal? maxPrice, DateTime? startDate, DateTime? endDate, string? status, int pageIndex = 1, int pageSize = 10)
        {
            var (orders, totalItems) = await _orderRepositories.GetAllOrdersAsync(keyword, minPrice, maxPrice, startDate, endDate, status, pageIndex, pageSize);

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = new ListDataResultModel<OrderResModel>
            {
                Data = _mapper.Map<List<OrderResModel>>(orders),
                CurrentPage = pageIndex,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
            return new ResultModel<ListDataResultModel<OrderResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }

        public async Task<ResultModel<ListDataResultModel<OrderResModel>>> GetSelfOrderList(string token, string? keyword, decimal? minPrice, decimal? maxPrice, DateTime? startDate, DateTime? endDate, string? status, int pageIndex = 1, int pageSize = 10)
        {
            var userId = new Guid(Authentication.DecodeToken(token, "userid"));

            var (orders, totalItems) = await _orderRepositories.GetSelfOrdersAsync(userId, keyword, minPrice, maxPrice, startDate, endDate, status, pageIndex, pageSize);

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = new ListDataResultModel<OrderResModel>
            {
                Data = _mapper.Map<List<OrderResModel>>(orders),
                CurrentPage = pageIndex,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
            return new ResultModel<ListDataResultModel<OrderResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }

        public async Task<ResultModel<DataResultModel<OrderDetailsResModel>>> GetOrderDetails(Guid orderId)
        {

            var order = await _orderRepositories.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return new ResultModel<DataResultModel<OrderDetailsResModel>>
                {
                    StatusCodes = (int)HttpStatusCode.NotFound,
                    Response = null
                };
            }

            // if (order.UserAddressId == null)
            // {
            //     var defaultUserAddress = await _userAddressRepositories.GetSingle(
            //         ua => ua.UserId == order.UserId && ua.Status == "Default")
            //         ?? throw new CustomException("Không tìm thấy địa chỉ mặc định cho người dùng.");

            //     // Gán UserAddressId cho Order
            //     order.UserAddressId = defaultUserAddress.Id;

            //     // Cập nhật Order trong cơ sở dữ liệu
            //     await _orderRepositories.Update(order);

            //     // Tải lại đối tượng Order từ cơ sở dữ liệu để đảm bảo dữ liệu mới nhất
            //     order = await _orderRepositories.GetOrderByIdAsync(order.Id)
            //         ?? throw new CustomException("Không thể tải lại đơn hàng sau khi cập nhật UserAddressId.");
            // }
            var orderDetails = _mapper.Map<OrderDetailsResModel>(order);
            if (order.UserAddressId != null)
            {
                var productDetails = order.OrderDetails.Where(od => od.ProductId != null).ToList();
                var deviceDetails = order.OrderDetails.Where(od => od.DeviceId != null).ToList();

                int districtId = await GetDistrictId(TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Province), TextConvert.ConvertFromUnicodeEscape(order.UserAddress.District));
                string wardId = await GetWardId(districtId, TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward));
                int serviceId = await GetService(districtId);

                var ghnOrder = new
                {
                    token = _ghnToken,
                    shop_id = _shopId,
                    required_note = "CHOXEMHANGKHONGTHU",
                    from_name = "HMES",
                    from_address = "117 Xô Viết Nghệ Tĩnh, Phường 17, Quận Bình Thạnh,TP. Hồ Chí Minh",
                    from_province_name = "TP. Hồ Chí Minh",
                    from_district_name = "Bình Thạnh",
                    from_ward_name = "Phường 17",
                    to_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Name),
                    to_phone = order.UserAddress.Phone,
                    to_address = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Address),
                    to_ward_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward),
                    to_district_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.District),
                    to_province_name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Province),
                    cod_amount = 0,
                    weight = 500,
                    length = 20,
                    width = 50,
                    height = 30,
                    service_type_id = serviceId,
                    payment_type_id = 2,
                    items = productDetails
                            .Select(p => new
                            {
                                name = p.Product?.Name ?? "Sản phẩm",
                                code = p.ProductId,
                                quantity = p.Quantity,
                                price = (int)p.UnitPrice,
                                weight = 500,
                                length = 20,
                                width = 50,
                                height = 30,
                            })
                            .Concat(deviceDetails.Select(d => new
                            {
                                name = d.Device?.Name ?? "Thiết bị",
                                code = d.DeviceId,
                                quantity = d.Quantity,
                                price = (int)d.UnitPrice,
                                weight = 500,
                                length = 20,
                                width = 50,
                                height = 30,
                            }))
                            .ToArray()
                };

                int weightOfOrder = ghnOrder.items.Sum(item => item.weight * item.quantity);
                ghnOrder = new
                {
                    ghnOrder.token,
                    ghnOrder.shop_id,
                    ghnOrder.required_note,
                    ghnOrder.from_name,
                    ghnOrder.from_address,
                    ghnOrder.from_province_name,
                    ghnOrder.from_district_name,
                    ghnOrder.from_ward_name,
                    ghnOrder.to_name,
                    ghnOrder.to_phone,
                    ghnOrder.to_address,
                    ghnOrder.to_ward_name,
                    ghnOrder.to_district_name,
                    ghnOrder.to_province_name,
                    ghnOrder.cod_amount,
                    weight = weightOfOrder,
                    ghnOrder.length,
                    ghnOrder.width,
                    ghnOrder.height,
                    ghnOrder.service_type_id,
                    ghnOrder.payment_type_id,
                    ghnOrder.items
                };

                int shippingFee = await CalculateShippingFee(ghnOrder);
                orderDetails.ShippingFee = shippingFee;
                orderDetails.TotalPrice = order.TotalPrice + shippingFee;
                order.ShippingFee = shippingFee;
                await _orderRepositories.Update(order);
            }

            if (!order.Status.Equals(OrderEnums.PendingPayment.ToString()))
            {
                orderDetails.Transactions = order.Transactions.Select(x => new OrderTransactionResModel
                {
                    PaymentLinkId = null,
                    PaymentStatus = x.Status,
                    PaymentMethod = x.PaymentMethod,
                    CreatedAt = x.CreatedAt,
                    TransactionId = x.Id,
                }).ToList();
            }


            var result = new DataResultModel<OrderDetailsResModel>
            {
                Data = orderDetails
            };
            return new ResultModel<DataResultModel<OrderDetailsResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };


        }

        private async Task<List<DeviceItem>> CreateDeviceItem(Order order)
        {
            try
            {
                List<DeviceItem> deviceItems = new List<DeviceItem>();
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.Device != null)
                    {
                        for (int i = 0; i < orderDetail.Quantity; i++)
                        {
                            var deviceItem = new DeviceItem
                            {
                                Id = Guid.NewGuid(),
                                DeviceId = orderDetail.Device.Id,
                                UserId = order.UserId,
                                Name = orderDetail.Device.Name,
                                IsActive = false,
                                IsOnline = false,
                                Serial = Authentication.GenerateRandomSerial(16),
                                WarrantyExpiryDate = DateTime.Now.AddYears(2),
                                Status = DeviceItemStatusEnum.Available.ToString(),
                                CreatedAt = DateTime.Now,
                                OrderId = order.Id,
                            };
                            deviceItems.Add(deviceItem);

                            await _deviceItemsRepositories.Insert(deviceItem);
                        }
                    }
                }
                return deviceItems;
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error creating DeviceItem: {ex.Message}");
            }
        }

        public async Task<ResultModel<MessageResultModel>> CashOnDeliveryHandle(Guid orderId, string token)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));

                // Lấy đơn hàng theo orderId
                var order = await _orderRepositories.GetSingle(
                    o => o.Id == orderId && o.UserId == userId,
                    includeProperties: "OrderDetails.Product,OrderDetails.Device,UserAddress"
                );

                if (order == null)
                    throw new CustomException("Order not found");

                if (order.Status != OrderEnums.Pending.ToString())
                    throw new CustomException("Order is not in pending status");

                // Cập nhật trạng thái đơn hàng thành "CashOnDelivery"
                await CreateShippingGHN(order, "CashOnDelivery");
                order.Status = OrderEnums.Delivering.ToString();
                order.UpdatedAt = DateTime.Now;
                await _orderRepositories.Update(order);
                var OrderPaymentRefId = int.Parse(GenerateRandomRefId());

                var NewTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    OrderPaymentRefId = OrderPaymentRefId,
                    Status = TransactionEnums.PROCESSING.ToString(),
                    PaymentMethod = PaymentMethodEnums.COD.ToString(),
                    CreatedAt = DateTime.Now,
                };
                await _transactionRepositories.Insert(NewTransaction);

                var cart = await _cartRepositories.GetSingle(x => x.UserId.Equals(userId), includeProperties: "CartItems");

                // Giả sử bạn đã có transaction.Order.OrderDetails như trước
                var cartItemFromTransaction = order.OrderDetails
                    .Select(od => new { od.ProductId, od.Quantity })
                    .ToList();

                // Áp dụng logic kiểm tra số lượng
                var itemsToDelete = new List<CartItem>();
                var itemsToUpdate = new List<CartItem>();
                if (cart == null)
                {
                    return new ResultModel<MessageResultModel>
                    {
                        StatusCodes = (int)HttpStatusCode.OK,
                        Response = new MessageResultModel { Message = "Cash on delivery order created successfully." }
                    };
                }
                foreach (var cartItem in cart.CartItems)
                {
                    var productInTransaction = cartItemFromTransaction.FirstOrDefault(ct => ct.ProductId == cartItem.ProductId);
                    if (productInTransaction != null)
                    {
                        if (cartItem.Quantity <= productInTransaction.Quantity)
                        {
                            itemsToDelete.Add(cartItem);
                        }
                        else
                        {
                            cartItem.Quantity -= productInTransaction.Quantity;
                            itemsToUpdate.Add(cartItem);
                        }
                    }
                }
                await _cartItemsRepositories.UpdateRange(itemsToUpdate);
                await _cartItemsRepositories.DeleteRange(itemsToDelete);
                var newDeviceItem = await CreateDeviceItem(order);


                return new ResultModel<MessageResultModel>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel { Message = "Cash on delivery order created successfully." }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<ResultModel<MessageResultModel>> CancelOrder(Guid orderId, string token)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));

                // Lấy đơn hàng theo orderId
                var order = await _orderRepositories.GetSingle(
                    o => o.Id == orderId && o.UserId == userId,
                    includeProperties: "OrderDetails.Product,OrderDetails.Device,UserAddress,Transactions"
                );

                if (order == null)
                    throw new CustomException("Order not found");

                if (order.Status != OrderEnums.Delivering.ToString())
                    throw new CustomException("Order is not in Delivering status");

                var transaction = order.Transactions.FirstOrDefault(x => x.PaymentMethod == PaymentMethodEnums.COD.ToString() && x.Status.Equals(TransactionEnums.PROCESSING.ToString()));

                if (transaction == null)
                {
                    throw new CustomException("Order is not Cash on Delivery.");
                }

                // Cập nhật trạng thái đơn hàng thành "Cancelled"
                transaction.Status = TransactionEnums.CANCELLED.ToString();
                order.Status = OrderEnums.Cancelled.ToString();
                order.UpdatedAt = DateTime.Now;
                await _orderRepositories.Update(order);
                await _transactionRepositories.Update(transaction);
                await CancelShipping(order);

                return new ResultModel<MessageResultModel>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel { Message = "Order cancelled successfully." }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<ResultModel<MessageResultModel>> UpdateOrderAddress(Guid orderId, Guid userAddressId, string token)
        {
            try
            {
                var userId = new Guid(Authentication.DecodeToken(token, "userid"));
                var order = await _orderRepositories.GetSingle(x => x.Id.Equals(orderId) && x.UserId.Equals(userId));

                if (order == null)
                    throw new CustomException("Order not found");

                var userAddress = await _userAddressRepositories.GetSingle(x => x.Id.Equals(userAddressId) && x.UserId.Equals(order.UserId));
                if (userAddress == null)
                    throw new CustomException("User address not found");

                order.UserAddressId = userAddress.Id;
                order.UpdatedAt = DateTime.Now;
                await _orderRepositories.Update(order);

                return new ResultModel<MessageResultModel>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel { Message = "Order address updated successfully." }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        private async Task CancelShipping(Order order)
        {
            try
            {
                if (order == null)
                    throw new CustomException("Không tìm thấy đơn hàng.");
                string[] orderCodes = { order.ShippingOrderCode };
                var cancelRequest = new
                {
                    token = _ghnToken,
                    order_codes = orderCodes,
                    shop_id = _shopId,
                };

                string cancelResponse = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/switch-status/cancel", cancelRequest);
                var responseObject = JsonConvert.DeserializeObject<dynamic>(cancelResponse);

                if (responseObject == null || responseObject.code != 200)
                {
                    throw new CustomException("Không thể hủy đơn hàng trên GHN: " + (responseObject?.message ?? "Lỗi không xác định."));
                }
            }
            catch (Exception ex)
            {
                throw new CustomException($"Lỗi khi hủy đơn hàng GHN: {ex.Message}");
            }
        }

        public async Task<ResultModel<DataResultModel<OrderPaymentResModel>>> GetCODBilling(Guid orderId, string token)
        {
            var userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var order = await _orderRepositories.GetSingle(x => x.Id.Equals(orderId) && x.UserId.Equals(userId), includeProperties: "OrderDetails.Product,OrderDetails.Device,UserAddress,DeviceItems");
            if (order == null)
            {
                throw new CustomException("Order not found");
            }
            if (order.Status != OrderEnums.Delivering.ToString())
            {
                throw new CustomException("Order is not in pending status");
            }
            var orderResModel = new OrderPaymentResModel()
            {
                Id = order.Id,
                OrderPrice = order.TotalPrice,
                ShippingPrice = order.ShippingFee ?? 0,
                TotalPrice = order.TotalPrice + (order.ShippingFee ?? 0),
                StatusPayment = order.Status,
                UserAddress = order.UserAddress != null ? new OrderUserAddress
                {
                    Id = order.UserAddress.Id,
                    Name = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Name),
                    Phone = order.UserAddress.Phone,
                    Address = TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Address),
                    IsDefault = order.UserAddress.Status.Equals(UserAddressEnums.Default.ToString())
                } : null,
                OrderProductItem = order.OrderDetails.Where(x => x.DeviceId == null).Select(detail => new OrderDetailResModel
                {
                    Id = detail.Id,
                    ProductName = detail.Product != null ? TextConvert.ConvertFromUnicodeEscape(detail.Product.Name) : null,
                    Attachment = detail.Product?.ProductAttachments.FirstOrDefault()?.Attachment ?? detail.Device?.Attachment ?? detail.Product?.MainImage ?? "",
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice
                }).ToList()
            };

            foreach (var deviceItem in order.DeviceItems)
            {
                orderResModel.OrderProductItem.Add(new OrderDetailResModel()
                {
                    Id = deviceItem.DeviceId,
                    ProductName = TextConvert.ConvertFromUnicodeEscape(order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).Device?.Name),
                    Attachment = order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).Device?.Attachment ?? deviceItem.Device?.Attachment ?? "",
                    Quantity = 1,
                    UnitPrice = order.OrderDetails.FirstOrDefault(x => x.DeviceId == deviceItem.DeviceId).UnitPrice,
                    Serial = deviceItem.Serial,
                });
            }
            return new ResultModel<DataResultModel<OrderPaymentResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<OrderPaymentResModel> { Data = orderResModel }
            };
        }
    }
}