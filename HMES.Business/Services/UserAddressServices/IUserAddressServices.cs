using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.UserAddressServices;

public interface IUserAddressServices
{
    Task<ResultModel<MessageResultModel>> CreateUserAddress(UserAddressCreateReqModel userAddressReq, string token);
}
