using System;

namespace OrderNotificationService.Tests.Integration;

public class Log
{
    public Guid OrderId { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
}