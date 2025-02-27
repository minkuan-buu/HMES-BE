using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Enums;
using HMES.Data.Repositories.CartRepositories;
using HMES.Data.Repositories.OrderDetailRepositories;
using HMES.Data.Repositories.OrderRepositories;
using HMES.Data.Repositories.TransactionRepositories;
using HMES.Data.Repositories.UserRepositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using Transaction = HMES.Data.Entities.Transaction;

namespace HMES.Business.Services.OrderServices
{
    public class OrderServices : IOrderServices
    {
        private readonly IHubContext<TransactionHub> _transactionHub;
        private readonly ILogger<OrderServices> _logger;
        private readonly IUserRepositories _userRepositories;
        private readonly IMapper _mapper;
        private readonly IOrderRepositories _orderRepositories;
        private readonly IOrderDetailRepositories _orderDetailRepositories;
        private readonly ITransactionRepositories _transactionRepositories;
        private readonly ICartRepositories _cartRepositories;
        private PayOS _payOS;

        public OrderServices(IHubContext<TransactionHub> transactionHub, ILogger<OrderServices> logger, IUserRepositories userRepositories, IMapper mapper, OrderRepositories orderRepositories, IOrderDetailRepositories orderDetailRepositories, ITransactionRepositories transactionRepositories, ICartRepositories cartRepositories)
        {
            _payOS = new PayOS("421fdf87-bbe1-4694-a76c-17627d705a85", "7a2f58da-4003-4349-9e4b-f6bbfc556c9b", "da759facf68f863e0ed11385d3bf9cf24f35e2b171d1fa8bae8d91ce1db9ff0c");
            _logger = logger;
            _userRepositories = userRepositories;
            _mapper = mapper;
            _orderRepositories = orderRepositories;
            _orderDetailRepositories = orderDetailRepositories;
            _transactionRepositories = transactionRepositories;
            _cartRepositories = cartRepositories;
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

    }

}