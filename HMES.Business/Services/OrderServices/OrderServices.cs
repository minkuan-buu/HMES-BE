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
        private readonly IUserAddressRepositories _userAddressRepositories;
        private readonly IDeviceRepositories _deviceRepositories;
        private readonly IDeviceItemsRepositories _deviceItemsRepositories;
        private readonly IProductRepositories _productRepositories;
        private PayOS _payOS;
        private readonly string _ghnToken = Environment.GetEnvironmentVariable("GHN_TOKEN");
        private readonly int _shopId = int.Parse(Environment.GetEnvironmentVariable("GHN_ID_SHOP"));
        private readonly HttpClient _httpClient;

        public OrderServices(ILogger<OrderServices> logger, IUserRepositories userRepositories, IMapper mapper, IOrderRepositories orderRepositories, IOrderDetailRepositories orderDetailRepositories, ITransactionRepositories transactionRepositories, ICartRepositories cartRepositories, IUserAddressRepositories userAddressRepositories, IDeviceRepositories deviceRepositories, IProductRepositories productRepositories, IDeviceItemsRepositories deviceItemsRepositories)
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
            _httpClient = new HttpClient();
        }

        public async Task<String> CreatePaymentUrl(string Token, Guid Id)
        {
            var userId = new Guid(Authentication.DecodeToken(Token, "userid"));
            var order = await _orderRepositories.GetSingle(
                x => x.Id.Equals(Id) && x.UserId.Equals(userId) && x.Status.Equals(OrderEnums.Pending.ToString()),
                includeProperties: "Transactions,OrderDetails.Product,OrderDetails.Device"
            );

            // Kiểm tra giao dịch PENDING trước
            var pendingTransaction = order.Transactions.FirstOrDefault(x => x.Status.Equals(TransactionEnums.PENDING.ToString()));
            if (pendingTransaction != null)
            {
                return $"https://pay.payos.vn/web/{pendingTransaction.PaymentLinkId}";
            }

            // Lấy danh sách sản phẩm và thiết bị trong đơn hàng
            var productIds = order.OrderDetails.Where(od => od.ProductId != null).Select(od => od.ProductId).ToList();
            var deviceIds = order.OrderDetails.Where(od => od.DeviceId != null).Select(od => od.DeviceId).ToList();

            var products = await _productRepositories.GetList(p => productIds.Contains(p.Id));
            var devices = await _deviceRepositories.GetList(d => deviceIds.Contains(d.Id));

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

            PaymentData paymentData = new PaymentData(OrderPaymentRefId, (int)order.TotalPrice, "",
                itemDatas, cancelUrl: "https://hmes.buubuu.id.vn/payment", returnUrl: "https://hmes.buubuu.id.vn/payment");

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
                var userAddress = await _userAddressRepositories.GetSingle(
                    ua => ua.UserId == userId && ua.Status == "Default");
                
                userAddress.Address = TextConvert.ConvertFromUnicodeEscape(userAddress.Address);

                if (userAddress == null)
                    throw new CustomException("Người dùng chưa có địa chỉ mặc định.");

                // Tách địa chỉ
                var (ward, district, province) = ParseAddressComponents(userAddress.Address);

                int districtId = await GetDistrictId(province, district);
                if (districtId == 0)
                    throw new CustomException("Không tìm thấy mã quận/huyện phù hợp.");

                string wardCode = await GetWardCode(districtId, ward);
                if (string.IsNullOrEmpty(wardCode))
                    throw new CustomException("Không tìm thấy mã phường/xã phù hợp.");


                if (districtId == 0 || string.IsNullOrEmpty(wardCode))
                    throw new CustomException("Không thể xác định khu vực giao hàng từ địa chỉ người dùng.");

                int serviceId = await GetService(districtId);
                if (serviceId == 0)
                    throw new CustomException("Không có dịch vụ giao hàng khả dụng đến khu vực này.");

                int codAmount = (int)orderRequest.Products.Sum(p => p.UnitPrice * p.Quantity);
                int deviceAmount = (int)orderRequest.Devices.Sum(d => d.UnitPrice * d.Quantity);
                int totalAmount = codAmount + deviceAmount;

                int shippingFee = await CalculateShippingFee(districtId, wardCode, totalAmount);

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
                    to_name = TextConvert.ConvertFromUnicodeEscape(userAddress.Name),
                    to_phone = userAddress.Phone,
                    to_address = TextConvert.ConvertFromUnicodeEscape(userAddress.Address),
                    to_ward_name = ward,
                    to_district_name = district,
                    to_province_name = province,
                    cod_amount = totalAmount,
                    weight = 500,
                    length = 20,
                    width = 50,
                    height = 30,
                    service_type_id = 2,
                    payment_type_id = 2,
                    items = orderRequest.Products
                        .Select(p => new { name = p.Name, code = p.Id, quantity = p.Quantity, price = (int)p.UnitPrice })
                        .Concat(orderRequest.Devices.Select(d => new { name = d.Name, code = d.Id, quantity = d.Quantity, price = (int)d.UnitPrice }))
                        .ToArray()
                };

                string ghnResponse = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create", ghnOrder);
                var responseObject = JsonConvert.DeserializeObject<dynamic>(ghnResponse);

                if (responseObject == null || responseObject.code != 200)
                    throw new CustomException("Không thể tạo đơn hàng trên GHN: " + (responseObject?.message ?? "Lỗi không xác định."));


                Guid orderId = Guid.NewGuid();
                Order order = new Order
                {
                    Id = orderId,
                    UserId = userId,
                    UserAddressId = userAddress.Id,
                    TotalPrice = totalAmount,
                    Status = OrderEnums.Pending.ToString(),
                    CreatedAt = DateTime.Now
                };
                await _orderRepositories.Insert(order);

                return new ResultModel<DataResultModel<Guid>>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new DataResultModel<Guid> { Data = orderId }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException($"Lỗi khi tạo đơn hàng: {ex.Message}");
            }
        }


        private async Task<int> GetDistrictId(string province, string district)
        {
            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district", new { province });
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            if (result == null || result.data == null)
                return 0;

            var districts = (result.data as JArray)?.ToObject<List<dynamic>>();

            var matchedDistrict = districts?.FirstOrDefault(d =>
                string.Equals((string)d["DistrictName"], district, StringComparison.OrdinalIgnoreCase));

            return matchedDistrict?["DistrictID"] != null ? (int)matchedDistrict["DistrictID"] : 0;
        }


        private async Task<string> GetWardCode(int districtId, string ward)
        {
            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward", new { district_id = districtId });
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            if (result == null || result.data == null)
                return string.Empty;

            var wards = (result.data as JArray)?.ToObject<List<dynamic>>();

            var matchedWard = wards?.FirstOrDefault(w =>
                string.Equals((string)w["WardName"], ward, StringComparison.OrdinalIgnoreCase));

            return matchedWard?["WardCode"]?.ToString() ?? string.Empty;
        }
        private (string Ward, string District, string Province) ParseAddressComponents(string fullAddress)
        {
            var parts = fullAddress.Split(',').Select(p => p.Trim()).ToList();

            // Lấy phần cuối là phường, quận, tỉnh (địa chỉ thường viết từ nhỏ đến lớn)
            string province = TextConvert.ConvertFromUnicodeEscape(parts.Count >= 3 ? parts[^1] : "");
            string district = TextConvert.ConvertFromUnicodeEscape(parts.Count >= 2 ? parts[^2] : "");
            string ward = TextConvert.ConvertFromUnicodeEscape(parts.Count >= 1 ? parts[^3] : "");

            return (ward, district, province);
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

            return matchedService?["service_id"] != null ? (int)matchedService["service_id"] : 0;
        }

        private async Task<int> CalculateShippingFee(int districtId, string wardCode, decimal codAmount)
        {
            var feeData = new { service_type_id = 2, to_district_id = districtId, to_ward_code = wardCode, cod_value = codAmount };
            var response = await SendPostRequest("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee", feeData);
            var result = JsonConvert.DeserializeObject<dynamic>(response);
            return result.data != null && result.data["total"] != null ? (int)result.data["total"] : 0;
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

                    // Xóa giỏ hàng sau khi thanh toán
                    var userCart = await _cartRepositories.GetList(x => x.UserId.Equals(userId));
                    if (userCart.Any())
                    {
                        await _cartRepositories.DeleteRange(userCart.ToList());
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
    }
}