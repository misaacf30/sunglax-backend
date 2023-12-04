namespace webapi.Dtos.Stripe
{
    public record AddStripeCustomer(
        string Email,
        string Name
        //AddStripeCard CreditCard
    );
}
