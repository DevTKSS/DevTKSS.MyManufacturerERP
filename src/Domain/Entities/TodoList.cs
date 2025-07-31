using System.Drawing;

namespace DevTKSS.MyManufacturerERP.Domain.Entities;

public class TodoList
{
    public string? Title { get; set; }
    public Colour? Color { get; set; }
    public IList<TodoItem> Items { get; set; } = new List<TodoItem>();
}
