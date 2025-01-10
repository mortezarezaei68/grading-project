using System;

namespace OrderNotificationService.Tests.Integration;

public class Notification
{
    public Guid OrderId { get; set; }
    public string Message { get; set; }
}