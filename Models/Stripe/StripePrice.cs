namespace webapi.Models.Stripe
{
    public record StripePrice(
        string Id,
        long? UnitAmount,
        string Currency,
        string Product
    );
}
