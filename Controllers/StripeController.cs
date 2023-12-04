using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Stripe;
using Stripe.Checkout;
using Stripe.TestHelpers;
using webapi.Dtos.Stripe;
using webapi.Models.Stripe;
using webapi.Services;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IStripeAppService _stripeService;

        public StripeController(IStripeAppService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("customer/add")]
        public async Task<ActionResult<StripeCustomer>> AddStripeCustomer(
            [FromBody] AddStripeCustomer customer,
            CancellationToken ct)
        {
            StripeCustomer createdCustomer = await _stripeService.AddStripeCustomerAsync(
                customer,
                ct);

            if(createdCustomer.CustomerId == null) {                // *
                return BadRequest("Customer already existssss");
            }

            return StatusCode(StatusCodes.Status200OK, createdCustomer);
        }


        [HttpPost("payment/add")]
        public async Task<ActionResult<StripePayment>> AddStripePayment(
            [FromBody] AddStripePayment payment,
            CancellationToken ct)
        {
            StripePayment createdPayment = await _stripeService.AddStripePaymentAsync(
                payment,
                ct);

            return StatusCode(StatusCodes.Status200OK, createdPayment);
        }

        [HttpPost("session/create")]
        public async Task<ActionResult<StripeSession>> CreateStripeSession(
            [FromBody] AddStripeSession session,
            CancellationToken ct)
        {
            StripeSession createdSession = await _stripeService.AddStripeSessionAsync(
                session,
                ct);

            return Ok(createdSession);
        }

        [HttpGet("session/getStatus/{id}")]
        public async Task<ActionResult> GetStripeSessionStatus(
                string id,
                CancellationToken ct)
        {
            var result = await _stripeService.GetStripeSessionStatusAsync(id, ct);

            return Ok(result) ;
        }

        [HttpPost("product/create")]
        public async Task<ActionResult> AddStripeProduct(
            [FromBody] AddStripeProduct product,
            CancellationToken ct)
        {
            StripeProduct createdProduct = await _stripeService.AddStripeProductAsync(
                product,
                ct);

            return StatusCode(StatusCodes.Status200OK, createdProduct);
        }

        [HttpPost("price/create")]
        public async Task<ActionResult> AddStripePrice(
            [FromBody] AddStripePrice price,
            CancellationToken ct)
        {
            StripePrice createdPrice = await _stripeService.AddStripePriceAsync(price, ct);
            return StatusCode(StatusCodes.Status200OK, createdPrice);
        }
    }
}
