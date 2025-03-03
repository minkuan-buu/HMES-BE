using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.OrderServices
{
    public interface IOrderServices
    {
        Task<String> CreatePaymentUrl(string Token, Guid Id);
        Task<ResultModel<DataResultModel<Guid>>> CreateOrder(CreateOrderDetailReqModel orderRequest, string token);
    }
}