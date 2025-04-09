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
        private readonly HttpClient _httpClient;

        public OrderServices(ILogger<OrderServices> logger, IUserRepositories userRepositories, IMapper mapper, IOrderRepositories orderRepositories, IOrderDetailRepositories orderDetailRepositories, ITransactionRepositories transactionRepositories, ICartRepositories cartRepositories, IUserAddressRepositories userAddressRepositories, IDeviceRepositories deviceRepositories, IProductRepositories productRepositories, IDeviceItemsRepositories deviceItemsRepositories, ICartItemsRepositories cartItemsRepositories)
        {
            _payOS = new PayOS("421fdf87-bbe1-4694-a76c-17627d705a85", "7a2f58da-4003-4349-9e4b-f6bbfc556c9b", "da759facf68f863e0ed11385d3bf9cf24f35e2b171d1fa8bae8d91ce1db9ff0c");
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
                x => x.Id.Equals(Id) && x.UserId.Equals(userId) && x.Status.Equals(OrderEnums.Pending.ToString()),
                includeProperties: "Transactions,OrderDetails.Product,OrderDetails.Device,UserAddress"
            );

            if (order.UserAddressId == null)
            {
                throw new CustomException("Người dùng chưa có địa chỉ cho đơn hàng.");
            }

            // Kiểm tra giao dịch PENDING trước
            var pendingTransaction = order.Transactions.FirstOrDefault(x => x.Status.Equals(TransactionEnums.PENDING.ToString()));
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
            int wardId = await GetWardId(districtId, TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward));
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

            PaymentData paymentData = new PaymentData(OrderPaymentRefId, (int)order.TotalPrice + shippingFee, "",
                itemDatas, cancelUrl: returnURL, returnUrl: returnURL);

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

            // **Trừ số lượng sản phẩm sau khi đã tạo giao dịch thành công**
            foreach (var od in order.OrderDetails)
            {
                if (od.ProductId != null)
                {
                    var product = products.First(p => p.Id == od.ProductId);
                    product.Amount -= od.Quantity;
                    await _productRepositories.Update(product);
                }
                else if (od.DeviceId != null)
                {
                    var device = devices.First(d => d.Id == od.DeviceId);
                    device.Quantity -= od.Quantity;
                    await _deviceRepositories.Update(device);
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
                    includeProperties: "OrderDetails");

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
                int wardId = await GetWardId(districtId, TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward));
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

                //int shippingFee = await CalculateShippingFee(_ghnToken, ghnOrder.items, weightOfOrder, _shopId, districtId, wardId.ToString(), codAmount);

                string ghnResponse = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create", ghnOrder);
                var responseObject = JsonConvert.DeserializeObject<dynamic>(ghnResponse);

                if (responseObject == null || responseObject.code != 200){
                    throw new CustomException("Không thể tạo đơn hàng trên GHN: " + (responseObject?.message ?? "Lỗi không xác định."));
                } else if (responseObject.data == null || responseObject.data["order_code"] != null){
                    order.ShippingOrderCode = (string)responseObject.data["order_code"];
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


        private async Task<int> GetWardId(int districtId, string ward)
        {
            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward", new { district_id = districtId });
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            foreach (var w in result.data)
            {
                if ((string)w.WardName == ward)
                    return (int)w.WardCode;
            }

            return 0;
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
                    x => x.PaymentLinkId.Equals(id) && x.Status.Equals(TransactionEnums.PENDING.ToString()),
                    includeProperties: "Order.OrderDetails.Product,Order.OrderDetails.Device,Order.UserAddress"
                );

                if (transaction == null)
                    throw new CustomException("Transaction not found");

                if (transaction.Order == null)
                    throw new CustomException("Order not found");

                var orderResModel = new OrderPaymentResModel()
                {
                    Id = transaction.Order.Id,
                    TotalPrice = transaction.Order.TotalPrice,
                    StatusPayment = transaction.Status,
                    UserAddress = transaction.Order.UserAddress != null ? new OrderUserAddress
                    {
                        Id = transaction.Order.UserAddress.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(transaction.Order.UserAddress.Name),
                        Phone = transaction.Order.UserAddress.Phone,
                        Address = TextConvert.ConvertFromUnicodeEscape(transaction.Order.UserAddress.Address),
                        IsDefault = transaction.Order.UserAddress.Status.Equals(UserAddressEnums.Default.ToString())
                    } : null,
                    OrderProductItem = transaction.Order.OrderDetails.Select(detail => new OrderDetailResModel
                    {
                        Id = detail.Id,
                        ProductName = detail.Product != null ? TextConvert.ConvertFromUnicodeEscape(detail.Product.Name) : null,
                        ProductItemName = detail.Device != null ? TextConvert.ConvertFromUnicodeEscape(detail.Device.Name) : null,
                        Attachment = detail.Product?.ProductAttachments.FirstOrDefault()?.Attachment ?? detail.Device?.Attachment ?? "",
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice
                    }).ToList()
                };

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
                    var cartItems = await _cartItemsRepositories.GetList(x => x.Cart.UserId.Equals(userId), includeProperties: "Cart,Product,Device");

                    // Giả sử bạn đã có transaction.Order.OrderDetails như trước
                    var cartItemFromTransaction = transaction.Order.OrderDetails
                        .Select(od => new { od.ProductId, od.Quantity })
                        .ToList();

                    // Áp dụng logic kiểm tra số lượng
                    foreach (var cartItem in cartItems)
                    {
                        var matchedTransactionItem = cartItemFromTransaction
                            .FirstOrDefault(ct => ct.ProductId == cartItem.ProductId);

                        if (matchedTransactionItem != null)
                        {
                            if (matchedTransactionItem.Quantity >= cartItem.Quantity)
                            {
                                await _cartItemsRepositories.Delete(cartItem);
                            }
                            else
                            {
                                cartItem.Quantity -= matchedTransactionItem.Quantity;
                                await _cartItemsRepositories.Update(cartItem);
                            }
                        }
                    }

                    await CreateDeviceItem(transaction.Order);
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


            var productDetails = order.OrderDetails.Where(od => od.ProductId != null).ToList();
            var deviceDetails = order.OrderDetails.Where(od => od.DeviceId != null).ToList();

            int districtId = await GetDistrictId(TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Province), TextConvert.ConvertFromUnicodeEscape(order.UserAddress.District));
            int wardId = await GetWardId(districtId, TextConvert.ConvertFromUnicodeEscape(order.UserAddress.Ward));
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

            var orderDetails = _mapper.Map<OrderDetailsResModel>(order);

            orderDetails.ShippingFee = shippingFee;
            orderDetails.TotalPrice = order.TotalPrice + shippingFee;

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

        private async Task CreateDeviceItem(Order order)
        {
            try
            {
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
                            };

                            await _deviceItemsRepositories.Insert(deviceItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error creating DeviceItem: {ex.Message}");
            }
        }

        public async Task<ResultModel<MessageResultModel>> CashOnDeliveryHandle(Guid orderId, string token){
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

        public async Task<ResultModel<MessageResultModel>> CancelOrder(Guid orderId, string token){
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

                // Cập nhật trạng thái đơn hàng thành "Cancelled"
                order.Status = OrderEnums.Cancelled.ToString();
                order.UpdatedAt = DateTime.Now;
                await _orderRepositories.Update(order);
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

        private async Task CancelShipping(Order order)
        {
            try
            {
                if (order == null)
                    throw new CustomException("Không tìm thấy đơn hàng.");

                var cancelRequest = new
                {
                    token = _ghnToken,
                    order_code = order.ShippingOrderCode,
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
    }
}