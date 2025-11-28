using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevTKSS.MyManufacturerERP.Domain.Common;

public abstract record BaseEntity
{
    public int Id { get; set; }

    private readonly List<BaseEvent> _Notifications = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> Notifications => _Notifications.AsReadOnly();

    public void AddEntityEvent(BaseEvent baseNotification)
    {
        _Notifications.Add(baseNotification);
    }

    public void RemoveNotification(BaseEvent baseNotification)
    {
        _Notifications.Remove(baseNotification);
    }

    public void ClearNotification()
    {
        _Notifications.Clear();
    }
}
