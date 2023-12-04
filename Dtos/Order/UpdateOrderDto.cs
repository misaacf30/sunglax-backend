namespace webapi.Dtos.Order
{
    public record UpdateOrderDto
    (
        string StripeSessionId,
        string Name
    );
}
