using DevTKSS.MyManufacturerERP.Domain.Entities;

namespace DevTKSS.MyManufacturerERP.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    public static LookupDto FromEntity(TodoList list) => new()
    {
        Id = list.Id,
        Title = list.Title
    };

    public static LookupDto FromEntity(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title
    };
}
