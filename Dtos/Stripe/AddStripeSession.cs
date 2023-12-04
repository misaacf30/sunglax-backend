using Stripe;
using Stripe.Checkout;

namespace webapi.Dtos.Stripe
{
    public record AddStripeSession(
        string? CustomerEmail,
        string? Customer,     // may be null
        string SuccessUrl,
        string CancelUrl,
        List<Tuple<string, int>> Products
    );
}
