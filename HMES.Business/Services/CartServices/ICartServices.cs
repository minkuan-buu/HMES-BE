using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;

namespace HMES.Business.Services.CartServices;

public interface ICartServices
{
    Task<ResultModel<ListDataResultModel<CartItemResponseDto>>> GetAllCartItems(int pageIndex, int pageSize, string token);
    Task<ResultModel<CartResponseDto>> GetCartByToken(string token);
    Task<ResultModel<CartResponseDto>> AddToCart(CartItemCreateDto cartItemCreateDto, string token);
    Task<ResultModel<CartResponseDto>> UpdateCartItem(CartItemUpdateDto cartItemUpdateDto, string token);
    Task<ResultModel<MessageResultModel>> ClearCartItems(string token);
}


