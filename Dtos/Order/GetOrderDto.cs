using webapi.Models;

namespace webapi.Dtos.Order
{
    public record GetOrderDto(
        int Id,
        string? Name,
        decimal Total,
        DateTime? CreatedAt,
        List<GetOrderProductDto> items
        );
}
