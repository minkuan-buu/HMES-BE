

using HMES.Business.Services.CartServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HMES.API.Controllers
{

    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartServices _cartServices;

        public CartController(ICartServices cartServices)
        {
            _cartServices = cartServices;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetCartByToken()
        {
            var token = Request.Headers.Authorization.ToString().Split(" ")[1];
            var result = await _cartServices.GetCartByToken(token);
            return Ok(result);
        }
        
        [HttpPost("add-to-cart")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemCreateDto item)
        {
            var token = Request.Headers.Authorization.ToString().Split(" ")[1];
            var result = await _cartServices.AddToCart(item, token);
            return Ok(result);
        }
        
        [HttpPut("update-quantity")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItemUpdateDto item)
        {
            var token = Request.Headers.Authorization.ToString().Split(" ")[1];
            var result = await _cartServices.UpdateCartItem(item, token);
            return Ok(result);
        }
        
        [HttpDelete("clear")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> DeleteItem()
        {
            var token = Request.Headers.Authorization.ToString().Split(" ")[1];
            var result = await _cartServices.ClearCartItems(token);
            return Ok(result);
        }
        
        [HttpGet("get-total-items")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetTotalItemsInCart(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var token = Request.Headers.Authorization.ToString().Split(" ")[1];
            var result = await _cartServices.GetAllCartItems(pageIndex, pageSize,token);
            return Ok(result);
        }
        
        

    }
}