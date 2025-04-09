using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.OrderServices
{
    public interface IOrderServices
    {
        Task<String> CreatePaymentUrl(string token, Guid id);
        Task<ResultModel<DataResultModel<Guid>>> CreateOrder(CreateOrderDetailReqModel orderRequest, string token);
        Task<ResultModel<MessageResultModel>> CashOnDeliveryHandle(Guid orderId, string token);
        Task<ResultModel<DataResultModel<OrderPaymentResModel>>> HandleCheckTransaction(string id, string token);
        Task<ResultModel<ListDataResultModel<OrderResModel>>> GetOrderList(string? keyword,
            decimal? minPrice, decimal? maxPrice, DateTime? startDate, DateTime? endDate, string? status, int pageIndex = 1,
            int pageSize = 10);
        Task<ResultModel<DataResultModel<OrderDetailsResModel>>> GetOrderDetails(Guid orderId);
        Task<ResultModel<MessageResultModel>> CancelOrder(Guid orderId, string token);
    }
}