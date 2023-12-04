namespace webapi.Dtos.Order
{
    public record GetOrderProductDto(
        int Id,
        string Name,
        int Quantity
        );
}
