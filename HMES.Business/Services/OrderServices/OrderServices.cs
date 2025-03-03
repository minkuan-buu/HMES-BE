using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.CartRepositories;
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
using Transaction = HMES.Data.Entities.Transaction;

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
        private readonly IProductRepositories _productRepositories;
        private PayOS _payOS;

        public OrderServices(ILogger<OrderServices> logger, IUserRepositories userRepositories, IMapper mapper, IOrderRepositories orderRepositories, IOrderDetailRepositories orderDetailRepositories, ITransactionRepositories transactionRepositories, ICartRepositories cartRepositories, IUserAddressRepositories userAddressRepositories, IDeviceRepositories deviceRepositories, IProductRepositories productRepositories)
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
        }

        public async Task<String> CreatePaymentUrl(string Token, Guid Id)
        {
            var userId = new Guid(Authentication.DecodeToken(Token, "userid"));
            var order = await _orderRepositories.GetSingle(x => x.Id.Equals(Id) && x.UserId.Equals(userId) && x.Status.Equals(OrderEnums.Pending.ToString()), includeProperties: "Transactions,OrderDetails.Product,Device");
            // Kiểm tra xem có giao dịch PENDING hay không
            var pendingTransaction = order.Transactions.Any(x => x.Status.Equals(TransactionEnums.PENDING.ToString()));

            if (pendingTransaction) // Chỉ cần kiểm tra nếu có giao dịch đang chờ xử lý
            {
                // Lấy PaymentLinkId của giao dịch đang chờ xử lý
                var transaction = order.Transactions.FirstOrDefault(x => x.Status.Equals(TransactionEnums.PENDING.ToString()));
                if (transaction != null)
                {
                    return $"https://pay.payos.vn/web/{transaction.PaymentLinkId}";
                }
                else
                {
                    // Trường hợp không có giao dịch PENDING thì trả về null hoặc thông báo lỗi
                    return "No pending transaction found.";
                }
            }
            else
            {
                // Nếu không có giao dịch PENDING thì tạo link thanh toán mới
                var OrderPaymentRefId = int.Parse(GenerateRandomRefId());
                List<ItemData> itemDatas = new();
                foreach (var item in order.OrderDetails)
                {
                    if (item.DeviceId != null)
                    {
                        itemDatas.Add(new ItemData(
                            $"{TextConvert.ConvertFromUnicodeEscape(item.Device.Name)} ({TextConvert.ConvertFromUnicodeEscape(item.Device.Name)})",
                            item.Quantity,
                            (int)item.UnitPrice
                        ));
                    }
                    else
                    {
                        itemDatas.Add(new ItemData(
                            $"{TextConvert.ConvertFromUnicodeEscape(item.Product.Name)} ({TextConvert.ConvertFromUnicodeEscape(item.Product.Name)})",
                            item.Quantity,
                            (int)item.UnitPrice
                        ));
                    }
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
                };
                await _transactionRepositories.Insert(NewTransaction);
                return createPayment.checkoutUrl;
            }
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

                // Lấy đơn hàng Pending hiện có của user (nếu có) kèm theo OrderDetails
                var existingOrder = await _orderRepositories.GetSingle(
                    o => o.UserId == userId && o.Status == OrderEnums.Pending.ToString(),
                    includeProperties: "OrderDetails");
                //clear toàn bộ orderdetail của order cũ
                if (existingOrder != null)
                {
                    await _orderDetailRepositories.DeleteRange(existingOrder.OrderDetails.ToList());
                    existingOrder.OrderDetails.Clear();
                }

                // Xác định orderId: nếu có đơn hàng pending rồi thì dùng orderId đó, nếu chưa có thì tạo mới
                Guid orderId = existingOrder != null ? existingOrder.Id : Guid.NewGuid();

                var productIds = orderRequest.Products.Select(p => p.Id).ToList();
                var products = await _productRepositories.GetList(p => productIds.Contains(p.Id));

                foreach (var prodReq in orderRequest.Products)
                {
                    var product = products.FirstOrDefault(p => p.Id == prodReq.Id);
                    if (product == null || product.Amount < prodReq.Quantity)
                    {
                        throw new CustomException($"Sản phẩm {prodReq.Id} không đủ số lượng để đặt hàng.");
                    }
                }

                // Kiểm tra Device
                List<Device> availableDevices = new List<Device>();
                if (orderRequest.DeviceQuantity > 0)
                {
                    var allDevices = await _deviceRepositories.GetList(d => d.UserId == null);

                    var allocatedOrderDetails = await _orderDetailRepositories.GetList(
                        od => od.DeviceId != null && od.Status == OrderEnums.Pending.ToString());
                    var allocatedDeviceIds = allocatedOrderDetails.Select(od => od.DeviceId.Value).ToList();

                    // Tìm các Device thực sự còn trống
                    availableDevices = allDevices.Where(d => !allocatedDeviceIds.Contains(d.Id)).ToList();

                    if (availableDevices.Count < orderRequest.DeviceQuantity)
                    {
                        throw new CustomException("Không đủ thiết bị trống để thực hiện đơn hàng.");
                    }
                }

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

                if (orderRequest.DeviceQuantity > 0)
                {
                    var selectedDevices = availableDevices.Take(orderRequest.DeviceQuantity).ToList();
                    foreach (var device in selectedDevices)
                    {
                        var orderDetail = new OrderDetail
                        {
                            Id = Guid.NewGuid(),
                            OrderId = orderId,
                            DeviceId = device.Id,
                            UnitPrice = device.Price,
                            Quantity = 1, 
                            Status = OrderEnums.Pending.ToString(),
                            CreatedAt = DateTime.Now
                        };
                        newOrderDetails.Add(orderDetail);
                    }
                }

                decimal totalPrice = newOrderDetails.Sum(od => od.UnitPrice * od.Quantity);

                Order order;
                if (existingOrder != null)
                {
                    // Nếu đã có đơn hàng Pending, xóa hết OrderDetail cũ và gán OrderDetail mới
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
                    // Nếu chưa có đơn hàng, tạo mới
                    order = new Order
                    {
                        Id = orderId,
                        UserId = userId,
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
    }
}