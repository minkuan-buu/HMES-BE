using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.UserAddressServices;

public interface IUserAddressServices
{
    Task<ResultModel<MessageResultModel>> CreateUserAddress(UserAddressCreateReqModel userAddressReq, string token);

    Task<ResultModel<MessageResultModel>> UpdateUserAddress(Guid id, UserAddressUpdateReqModel userAddressReq, string token);

    Task<ResultModel<MessageResultModel>> SetDefaultUserAddress(Guid id, string token);

    Task<ResultModel<MessageResultModel>> DeleteUserAddress(Guid id, string token);

    Task<ResultModel<ListDataResultModel<ListUserAddressResModel>>> GetUserAddress(string token);
}
