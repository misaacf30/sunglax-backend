namespace webapi.Dtos.Stripe
{
    public record AddStripePrice(
        long UnitAmount,
        string Currency,
        string Product
    );
}
