using HMES.Business.Services.OrderServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers
{
    [Route("api/transaction")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IOrderServices _orderServices;

        public TransactionController(IOrderServices orderServices)
        {
            _orderServices = orderServices;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> CreatePayment([FromBody] Guid Id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.CreatePaymentUrl(token, Id);
            return Ok(result);
        }

        [HttpPost("cod")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> HandleCOD([FromBody] Guid Id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.CashOnDeliveryHandle(Id, token);
            return Ok(result);
        }

        [HttpGet("cod/{id}")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> GetCODBilling(Guid id)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.GetCODBilling(id, token);
            return Ok(result);
        }

        [HttpPost("check")]
        [Authorize(AuthenticationSchemes = "HMESAuthentication")]
        public async Task<IActionResult> HandleCheckTransaction([FromBody] string PaymentLinkId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _orderServices.HandleCheckTransaction(PaymentLinkId, token);
            return Ok(result);
        }
    }
}