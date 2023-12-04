namespace webapi.Models.Stripe
{
    public record StripeProduct(
        string Id,
        string Name,
        string Description
    );
}
