using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using webapi.Dtos.Stripe;
using webapi.Models.Stripe;

namespace webapi.Services
{
    public interface IStripeAppService
    {
        Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct);
        Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct);
        Task<StripeSession> AddStripeSessionAsync(AddStripeSession session, CancellationToken ct);
        Task<StripeProduct> AddStripeProductAsync(AddStripeProduct product, CancellationToken ct);
        Task<StripePrice> AddStripePriceAsync(AddStripePrice price, CancellationToken ct); // *
        Task<GetStripeSession> GetStripeSessionStatusAsync(string id, CancellationToken ct);
    }
}
