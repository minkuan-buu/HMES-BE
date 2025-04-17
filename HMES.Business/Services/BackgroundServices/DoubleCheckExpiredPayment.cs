using System.Text;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CartRepositories;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Repositories.DeviceRepositories;
using HMES.Data.Repositories.OrderRepositories;
using HMES.Data.Repositories.TransactionRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DoubleCheckExpiredPayment : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private PayOS _payOS;
    private readonly HttpClient _httpClient;
    private readonly string _ghnToken = Environment.GetEnvironmentVariable("GHN_TOKEN");
    private readonly int _shopId = int.Parse(Environment.GetEnvironmentVariable("GHN_ID_SHOP"));

    public DoubleCheckExpiredPayment(IServiceScopeFactory serviceScopeFactory)
    {
        _payOS = new PayOS(
            "421fdf87-bbe1-4694-a76c-17627d705a85",
            "7a2f58da-4003-4349-9e4b-f6bbfc556c9b",
            "da759facf68f863e0ed11385d3bf9cf24f35e2b171d1fa8bae8d91ce1db9ff0c"
        );
        _serviceScopeFactory = serviceScopeFactory;
        _httpClient = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var transactionRepositories = scope.ServiceProvider.GetRequiredService<ITransactionRepositories>();
            var cartRepositories = scope.ServiceProvider.GetRequiredService<ICartRepositories>();
            var orderRepositories = scope.ServiceProvider.GetRequiredService<IOrderRepositories>();
            var cartItemsRepositories = scope.ServiceProvider.GetRequiredService<ICartItemsRepositories>();
            var deviceItemsRepositories = scope.ServiceProvider.GetRequiredService<IDeviceItemsRepositories>();

            var pendingTransactions = await transactionRepositories.GetPendingTransaction();

            if (pendingTransactions == null || pendingTransactions.Count == 0)
            {
                Console.WriteLine("No pending transactions found.");
            }
            else
            {
                foreach (var transaction in pendingTransactions)
                {
                    if (transaction.OrderPaymentRefId <= 0)
                    {
                        Console.WriteLine($"Invalid OrderPaymentRefId for Transaction ID: {transaction.Id}");
                        continue;
                    }

                    try
                    {
                        var paymentLinkInformation = await _payOS.getPaymentLinkInformation(transaction.OrderPaymentRefId);

                        if (paymentLinkInformation == null)
                        {
                            Console.WriteLine($"No Payment Info found for Transaction ID: {transaction.Id}");
                            continue;
                        }

                        if (paymentLinkInformation.status.Equals(TransactionEnums.EXPIRED.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            transaction.Status = TransactionEnums.EXPIRED.ToString();
                            transaction.Order.Status = OrderEnums.Cancelled.ToString();
                            transaction.Order.UpdatedAt = DateTime.Now;

                            foreach (var orderDetail in transaction.Order.OrderDetails)
                            {
                                if (orderDetail.Product != null)
                                {
                                    orderDetail.Product.Amount += orderDetail.Quantity;
                                }
                                if (orderDetail.Device != null)
                                {
                                    orderDetail.Device.Quantity += orderDetail.Quantity;
                                }
                            }

                            await transactionRepositories.Update(transaction);
                            Console.WriteLine($"Transaction ID {transaction.Id} was cancelled and updated in database.");
                        }
                        else if (paymentLinkInformation.status.Equals(TransactionEnums.PAID.ToString()))
                        {
                            var getTransaction = paymentLinkInformation.transactions.FirstOrDefault();
                            transaction.TransactionReference = getTransaction.reference;
                            transaction.FinishedTransactionAt = DateTime.Parse(getTransaction.transactionDateTime);
                            transaction.Status = TransactionEnums.PAID.ToString();
                            transaction.Order.Status = OrderEnums.Delivering.ToString();
                            transaction.Order.UpdatedAt = DateTime.Now;

                            await CreateShippingGHN(transaction.Order, "Banking", orderRepositories);

                            // Xóa giỏ hàng sau khi thanh toán

                            // Lấy cart items của user từ DB (hoặc theo orderId nếu bạn đang lọc theo order)
                            var cart = await cartRepositories.GetSingle(x => x.UserId.Equals(transaction.Order.UserId), includeProperties: "CartItems");

                            // Giả sử bạn đã có transaction.Order.OrderDetails như trước
                            var cartItemFromTransaction = transaction.Order.OrderDetails
                                .Select(od => new { od.ProductId, od.Quantity })
                                .ToList();

                            // Áp dụng logic kiểm tra số lượng
                            foreach (var cartItem in cart.CartItems)
                            {
                                // Tìm kiếm sản phẩm trong giỏ hàng tương ứng với sản phẩm trong giao dịch
                                var productInTransaction = cartItemFromTransaction.FirstOrDefault(ct => ct.ProductId == cartItem.ProductId);
                                if (productInTransaction != null)
                                {
                                    // Nếu số lượng trong giỏ hàng lớn hơn hoặc bằng số lượng trong giao dịch, xóa giỏ hàng
                                    if (cartItem.Quantity >= productInTransaction.Quantity)
                                    {
                                        await cartItemsRepositories.Delete(cartItem); //Lỗi ở đây!!!
                                    }
                                    else
                                    {
                                        // Nếu không, cập nhật lại số lượng trong giỏ hàng
                                        cartItem.Quantity -= productInTransaction.Quantity;
                                        await cartItemsRepositories.Update(cartItem);
                                    }
                                }
                            }

                            await CreateDeviceItem(transaction.Order, deviceItemsRepositories);
                        }
                        else if (paymentLinkInformation.status.Equals(TransactionEnums.CANCELLED.ToString()))
                        {
                            transaction.Status = TransactionEnums.CANCELLED.ToString();
                            transaction.Order.Status = OrderEnums.Cancelled.ToString();
                            transaction.Order.UpdatedAt = DateTime.Now;

                            //Hoàn trả số lượng sản phẩm & thiết bị nếu đơn hàng bị hủy
                            foreach (var orderDetail in transaction.Order.OrderDetails)
                            {
                                if (orderDetail.Product != null)
                                {
                                    orderDetail.Product.Amount += orderDetail.Quantity;
                                }
                                if (orderDetail.Device != null)
                                {
                                    orderDetail.Device.Quantity += orderDetail.Quantity;
                                }
                            }
                        }
                        await transactionRepositories.Update(transaction);
                    }
                    catch (Exception ex)
                    {
                        transaction.Status = TransactionEnums.CANCELLED.ToString();
                        transaction.Order.Status = OrderEnums.Cancelled.ToString();
                        transaction.Order.UpdatedAt = DateTime.Now;
                        await transactionRepositories.Update(transaction);
                        Console.WriteLine($"Error fetching payment info for Transaction ID: {transaction.OrderPaymentRefId} - Exception: {ex.Message}");
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task CreateShippingGHN(Order order, string paymentMethod, IOrderRepositories orderRepositories)
    {
        try
        {
            if (order == null)
                Console.WriteLine("Không tìm thấy đơn hàng.");

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
                        await orderRepositories.Update(order);
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Không thể tạo đơn hàng trên GHN: " + (responseObject?.message ?? "Lỗi không xác định."));
                }
            }
            else if (responseObject?.data is JObject dataObj && dataObj["order_code"] != null)
            {
                order.ShippingOrderCode = dataObj["order_code"]?.ToString();
                order.UpdatedAt = DateTime.Now;
                order.Status = OrderEnums.Delivering.ToString();
                await orderRepositories.Update(order);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi tạo đơn hàng GHN: {ex.Message}");
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

    private async Task<string> SendPostRequest(string url, object data)
    {
        var requestContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Token", _ghnToken);
        _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId.ToString());

        var response = await _httpClient.PostAsync(url, requestContent);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task CreateDeviceItem(Order order, IDeviceItemsRepositories deviceItemsRepositories)
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

                        await deviceItemsRepositories.Insert(deviceItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating DeviceItem: {ex.Message}");
        }
    }
}
