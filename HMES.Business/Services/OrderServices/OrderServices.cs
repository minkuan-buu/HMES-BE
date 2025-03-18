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
        private readonly IDeviceItemsRepositories _deviceItemsRepositories;
        private readonly IProductRepositories _productRepositories;
        private PayOS _payOS;

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

                if (userAddress == null)
                {
                    throw new CustomException("Người dùng chưa có địa chỉ mặc định.");
                }

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
                    existingOrder.UserAddressId = userAddress.Id; // Gán UserAddressId vào đơn hàng
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
                        UserAddressId = userAddress.Id, // Gán UserAddressId vào đơn hàng
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
                    await CreateDeviceItem(); 

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

        private async Task CreateDeviceItem()
        {
        var devices = await _deviceRepositories.GetList(d => d.Status.Equals(DeviceStatusEnum.Active.ToString()));
        var userId = await _userRepositories.GetSingle(u => u.Status.Equals(GeneralStatusEnums.Active.ToString()));

        foreach (var device in devices)
        {
            var deviceItems = new List<DeviceItem>();
            for (int i = 0; i < device.Quantity; i++)
            {
            var deviceItem = new DeviceItem
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                UserId = userId.Id,
                Name = TextConvert.ConvertToUnicodeEscape(device.Name),
                IsActive = false,
                IsOnline = false,
                Serial = Authentication.GenerateRandomSerial(16),
                WarrantyExpiryDate = DateTime.Now.AddYears(2),
                Status = DeviceItemStatusEnum.Available.ToString(),
                CreatedAt = DateTime.Now
            };
            deviceItems.Add(deviceItem);
            }

            await _deviceItemsRepositories.InsertRange(deviceItems);
        }
        }
        }
    }