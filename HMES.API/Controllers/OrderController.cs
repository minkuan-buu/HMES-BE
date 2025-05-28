using HMES.Business.Services.OrderServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderServices _orderServices;

        public OrderController(IOrderServices orderServices)
        {
            _orderServices = orderServices;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDetailReqModel orderRequest)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.CreateOrder(orderRequest, token);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? keyword,
            [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate, [FromQuery] string? status, [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {

            var result = await _orderServices.GetOrderList(keyword, minPrice, maxPrice, startDate, endDate,
                status, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetSelfOrders([FromQuery] string? keyword,
            [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate, [FromQuery] string? status, [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.GetSelfOrderList(token, keyword, minPrice, maxPrice, startDate, endDate,
                status, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var result = await _orderServices.GetOrderDetails(id);
            return Ok(result);
        }

        [HttpPost("cancel")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> CancelOrder([FromBody] Guid orderId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.CancelOrder(orderId, token);
            return Ok(result);
        }

        [HttpPut("change-address")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> UpdateOrderAddress([FromBody] OrderUpdateAddress request)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.UpdateOrderAddress(request.OrderId, request.UserAddressId, token);
            return Ok(result);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> HandleCallback([FromBody] GHNReqModel callbackData)
        {
            await _orderServices.HandleGhnCallback(callbackData);
            return Ok();
        }

        [HttpPost("confirm-cod")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication", Roles = "Consultant")]
        public async Task<IActionResult> ConfirmOrderCOD([FromBody] OrderConfirmReqModel orderConfirm)
        {
            var result = await _orderServices.ConfirmOrderCOD(orderConfirm);
            return Ok(result);
        }
    }
}