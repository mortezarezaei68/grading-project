using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using MongoDB.Driver;
using Xunit;

namespace OrderNotificationService.Tests.Integration;

public class OrderNotificationServiceEndToEndTests : IAsyncLifetime
{
    private IBus _busControl;
    private readonly InMemoryTestHarness _testHarness;
    private IMongoDatabase _writeDb;
    private IMongoDatabase _readDb;
    private MongoClient _mongoClient;

    public OrderNotificationServiceEndToEndTests()
    {
        _testHarness = new InMemoryTestHarness();
    }

    public async Task InitializeAsync()
    {
        // Set up MongoDB databases (you can also use an in-memory MongoDB for testing purposes)
        var mongoDbUrl = "mongodb://root:1234@localhost:27017"; // Make sure MongoDB is running
        _mongoClient = new MongoClient(new MongoUrl(mongoDbUrl));
        _writeDb = _mongoClient.GetDatabase("OrderWriteDb");
        _readDb = _mongoClient.GetDatabase("OrderReadDb");

        // Start MassTransit InMemory Test Harness
        await _testHarness.Start();
        _busControl = _testHarness.Bus;
    }

    public async Task DisposeAsync()
    {
        // Stop MassTransit InMemory Test Harness
        await _testHarness.Stop();
    }

    [Fact]
    public async Task FullEndToEndTest_OrderShippedEvent_SendsNotificationAndLogsErrors()
    {
        // Arrange: Simulate an 'OrderShipped' event
        var orderId = Guid.NewGuid();
        var eventMessage = new OrderShippedEvent
        {
            OrderId = orderId,
            ShippedDate = DateTime.UtcNow
        };

        // Act: Publish the event to MassTransit bus using InMemory Test Harness
        await _busControl.Publish(eventMessage);

        // Wait for processing to complete
        await Task.Delay(500); // Allow time for the event to be processed and recorded

        // Assert: Verify that the notification was sent and recorded in the write DB
        var notificationCollection = _writeDb.GetCollection<Notification>("Notifications");
        var notification = await notificationCollection.Find(n => n.OrderId == orderId).FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(orderId, notification.OrderId);
        Assert.Contains("Your order has been shipped", notification.Message);

        // Assert: Check that no error occurred during processing
        var logCollection = _readDb.GetCollection<Log>("ErrorLogs");
        var logEntry = await logCollection.Find(log => log.OrderId == orderId && log.Level == "Error").FirstOrDefaultAsync();
        Assert.Null(logEntry); // No error should have been logged
    }

    [Fact]
    public async Task FullEndToEndTest_PaymentCompletedEvent_SendsNotificationAndLogsErrors()
    {
        // Arrange: Simulate a 'PaymentCompleted' event
        var orderId = Guid.NewGuid();
        var eventMessage = new PaymentCompletedEvent
        {
            OrderId = orderId,
            PaymentDate = DateTime.UtcNow
        };

        // Act: Publish the event to MassTransit bus using InMemory Test Harness
        await _busControl.Publish(eventMessage);

        // Wait for processing to complete
        await Task.Delay(500); // Allow time for the event to be processed and recorded

        // Assert: Verify that the notification was sent and recorded in the write DB
        var notificationCollection = _writeDb.GetCollection<Notification>("Notifications");
        var notification = await notificationCollection.Find(n => n.OrderId == orderId).FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(orderId, notification.OrderId);
        Assert.Contains("Your payment has been completed", notification.Message);

        // Assert: Check that no error occurred during processing
        var logCollection = _readDb.GetCollection<Log>("ErrorLogs");
        var logEntry = await logCollection.Find(log => log.OrderId == orderId && log.Level == "Error").FirstOrDefaultAsync();
        Assert.Null(logEntry); // No error should have been logged
    }

    [Fact]
    public async Task FullEndToEndTest_SimulateErrorInNotification_FailsAndLogsError()
    {
        // Arrange: Simulate an 'OrderShipped' event
        var orderId = Guid.NewGuid();
        var eventMessage = new OrderShippedEvent
        {
            OrderId = orderId,
            ShippedDate = DateTime.UtcNow
        };

        // Simulate an error in the notification processing (e.g., no connection to email service, etc.)
        // You can mock this failure in your event handler (e.g., by throwing an exception inside the event handler)

        // Act: Publish the event to MassTransit bus using InMemory Test Harness
        await _busControl.Publish(eventMessage);

        // Wait for processing to complete
        await Task.Delay(500); // Allow time for the event to be processed and recorded

        // Assert: Verify that the error is logged in the read DB
        var logCollection = _readDb.GetCollection<Log>("ErrorLogs");
        var logEntry = await logCollection.Find(log => log.OrderId == orderId && log.Level == "Error").FirstOrDefaultAsync();
        Assert.NotNull(logEntry);
        Assert.Equal("Error", logEntry.Level);
        Assert.Contains("Failed to send notification", logEntry.Message); // Customize the error message based on the failure
    }
}