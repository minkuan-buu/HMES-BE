using HMES.Business.Services.OrderServices;
using HMES.Data.DTO.RequestModel;
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
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDetailReqModel orderRequest)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.CreateOrder(orderRequest, token);
            return Ok(result);
        }

    }
}