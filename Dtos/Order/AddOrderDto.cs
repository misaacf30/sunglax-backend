namespace webapi.Dtos.Order
{
    public record AddOrderDto(
        int? userId,
        decimal Total,
        string? StripeSessionId,
        List<Tuple<string, int>> Items      // PriceId | Quantity
    );
}
