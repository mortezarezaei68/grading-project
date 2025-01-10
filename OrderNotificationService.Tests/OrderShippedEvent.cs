using System;

namespace OrderNotificationService.Tests;

public class OrderShippedEvent
{
    public Guid OrderId { get; set; }
    public DateTime ShippedDate { get; set; }
}