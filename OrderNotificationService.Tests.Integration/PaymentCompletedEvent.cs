using System;

namespace OrderNotificationService.Tests.Integration;

public class PaymentCompletedEvent
{
    public Guid OrderId { get; set; }
    public DateTime PaymentDate { get; set; }
}