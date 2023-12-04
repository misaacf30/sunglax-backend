using Stripe;
using Stripe.Checkout;

namespace webapi.Models.Stripe
{
    public record StripeSession(
        string Id,
        string? CustomerEmail,
        string? Customer,     // may be null
        string SuccessUrl,
        string CancelUrl
    );
}
