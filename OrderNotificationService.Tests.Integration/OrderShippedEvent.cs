using System;

namespace OrderNotificationService.Tests.Integration;

public class OrderShippedEvent
{
    public Guid OrderId { get; set; }
    public DateTime ShippedDate { get; set; }
}