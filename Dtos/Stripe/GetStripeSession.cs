using Stripe.Checkout;

namespace webapi.Dtos.Stripe
{
    public record GetStripeSession (
        /*int Id,
        string CustomerId,
        List<Tuple<string, int>> Products,*/
        string Status,
        string Name
        //SessionShippingDetails ShippingDetails
    );
}
