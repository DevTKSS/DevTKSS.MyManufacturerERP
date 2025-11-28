namespace DevTKSS.MyManufacturerERP.Domain.Entities;

public record TodoList : BaseEntity
{
    public string? Title { get; set; }
    public Colour? Color { get; set; }
    public IList<TodoItem> Items { get; set; } = [];
}
