using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.TimeZoneHelper;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.CartRepositories;
using HMES.Data.Repositories.ProductRepositories;

namespace HMES.Business.Services.CartServices;

public class CartServices : ICartServices
{
    private readonly ICartRepositories _cartRepository;
    private readonly ICartItemsRepositories _cartItemsRepositories;
    private readonly IProductRepositories _productRepositories;
    private readonly IMapper _mapper;

    public CartServices(ICartRepositories cartRepository, ICartItemsRepositories cartItemsRepositories, IProductRepositories productRepositories, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _cartItemsRepositories = cartItemsRepositories;
        _productRepositories = productRepositories;
        _mapper = mapper;
    }

    public async Task<ResultModel<ListDataResultModel<CartItemResponseDto>>> GetAllCartItems(int pageIndex, int pageSize, string token)
    {
        //Get cart Items paging
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var cart = await _cartRepository.GetCartByUserId(userId);

            if (cart == null)
            {
                cart = await InitCart(userId);
            }

            var (cartItems,totalItems) = await _cartItemsRepositories.GetItemsPaging(cart.Id, pageIndex, pageSize);
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var result = new ListDataResultModel<CartItemResponseDto>
            
            {
                CurrentPage = pageIndex,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize,
                Data = _mapper.Map<List<CartItemResponseDto>>(cartItems)
            };

            return new ResultModel<ListDataResultModel<CartItemResponseDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
    }

    public async Task<ResultModel<CartResponseDto>> GetCartByToken(string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));

            var cart = await _cartRepository.GetCartByUserId(userId);

            if (cart == null)
            {
                var newCart = await InitCart(userId);
                var result = _mapper.Map<CartResponseDto>(newCart);
                result.TotalItems = 0;
                
                return new ResultModel<CartResponseDto>
                {
                    StatusCodes = (int)HttpStatusCode.Created,
                    Response = result
                };
            }
            else
            {
                var result = _mapper.Map<CartResponseDto>(cart);
                result.TotalItems = await _cartItemsRepositories.GetTotalItemsInCart(cart.Id);

                return new ResultModel<CartResponseDto>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = result
                };
            }
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
    }

    private async Task<Cart> InitCart(Guid userId)
    {
        Cart newCart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime(),
        };
        await _cartRepository.Insert(newCart);
        return newCart;
    }

    public async Task<ResultModel<CartResponseDto>> AddToCart(CartItemCreateDto cartItemCreateDto, string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var cart = await _cartRepository.GetCartByUserId(userId);
            var product = await _productRepositories.GetByIdAsync(cartItemCreateDto.ProductId);
        
            if (product == null)
            {
                throw new CustomException("Product not found");
            }

            if (cart == null)
            {
                cart = await InitCart(userId);
            }

            var cartItemExist = await _cartItemsRepositories.GetItemExist(cart.Id, product.Id);
            await CreateOrUpdateCartItem(cart, cartItemExist, cartItemCreateDto, product.Price);

            var result = _mapper.Map<CartResponseDto>(cart);
            result.TotalItems = await _cartItemsRepositories.GetTotalItemsInCart(cart.Id);

            return new ResultModel<CartResponseDto>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
    }

    private async Task CreateOrUpdateCartItem(Cart cart, CartItem? cartItemExist, CartItemCreateDto cartItemCreateDto, decimal unitPrice)
    {
        if (cartItemExist != null)
        {
            cartItemExist.Quantity += cartItemCreateDto.Quantity;
            cartItemExist.UpdatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
            cartItemExist.UnitPrice = unitPrice;

            await _cartItemsRepositories.Update(cartItemExist);
        }
        else
        {
            var cartItem = _mapper.Map<CartItem>(cartItemCreateDto);
            cartItem.Id = Guid.NewGuid();
            cartItem.ProductId = cartItemCreateDto.ProductId;
            cartItem.CartId = cart.Id;
            cartItem.CreatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
            cartItem.UnitPrice = unitPrice;

            await _cartItemsRepositories.Insert(cartItem);
        }
    }


    public async Task<ResultModel<CartResponseDto>> UpdateCartItem(CartItemUpdateDto cartItemUpdateDto, string token)
    {
        try
        {
            
            if(cartItemUpdateDto.Quantity < 0)
            {
                throw new CustomException("Quantity must be greater than 0");
            }
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var cart = await _cartRepository.GetCartByUserId(userId);
            
            if(cart == null)
            {
                throw new CustomException("Cart not found");
            }
            var cartItem = await _cartItemsRepositories.GetItemExist(cart.Id, cartItemUpdateDto.ProductId);
            
            if(cartItem==null)
            {
                throw new CustomException("Cart item not found");
            }
            if(cartItemUpdateDto.Quantity == 0)
            {
                await _cartItemsRepositories.Delete(cartItem);
            }
            else
            {
                cartItem.Quantity = cartItemUpdateDto.Quantity;
                cartItem.UpdatedAt = TimeZoneHelper.GetCurrentHoChiMinhTime();
                await _cartItemsRepositories.Update(cartItem);
            }
            var result = _mapper.Map<CartResponseDto>(cart);
            result.TotalItems = await _cartItemsRepositories.GetTotalItemsInCart(cart.Id);
            return new ResultModel<CartResponseDto>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = result
            };
        }catch(Exception e)
        {
            throw new CustomException(e.Message);
        }
    }

    public async Task<ResultModel<MessageResultModel>> ClearCartItems(string token)
    {
        try
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            var cart = await _cartRepository.GetCartByUserId(userId);
            if(cart == null)
            {
                throw new CustomException("Cart not found");
            }
            await _cartItemsRepositories.DeleteAll(cart.Id);
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new MessageResultModel
                {
                    Message = "Cart is cleared"
                }
            };
        }catch(Exception e)
        {
            throw new CustomException(e.Message);
        }
    }
}