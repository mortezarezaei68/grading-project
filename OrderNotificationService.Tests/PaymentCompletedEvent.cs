using System;

namespace OrderNotificationService.Tests;

public class PaymentCompletedEvent
{
    public Guid OrderId { get; set; }
    public DateTime PaymentDate { get; set; }
}