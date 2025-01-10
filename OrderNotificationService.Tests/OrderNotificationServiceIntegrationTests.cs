using MassTransit;
using MassTransit.Testing;
using MongoDB.Driver;
using Xunit;
using System;
using System.Threading.Tasks;

namespace OrderNotificationService.Tests
{
    public class OrderNotificationServiceIntegrationTests : IAsyncLifetime
    {
        private IBus _busControl;
        private readonly InMemoryTestHarness _testHarness;
        private IMongoDatabase _writeDb;
        private IMongoDatabase _readDb;

        public OrderNotificationServiceIntegrationTests()
        {
            _testHarness = new InMemoryTestHarness();
        }

        public async Task InitializeAsync()
        {
            // Set up MongoDB databases
            var mongoDbUrl = "mongodb://root:1234@localhost:27017";
            var client = new MongoClient(new MongoUrl(mongoDbUrl));
            _writeDb = client.GetDatabase("OrderWriteDb");
            _readDb = client.GetDatabase("OrderReadDb");

            // Start MassTransit InMemory Test Harness
            await _testHarness.Start();

            // Initialize the bus control once
            _busControl = _testHarness.Bus;
        }

        public async Task DisposeAsync()
        {
            // Stop MassTransit InMemory Test Harness
            await _testHarness.Stop();
        }

        [Fact]
        public async Task Should_Send_Notification_On_OrderShipped_Event()
        {
            // Arrange: Simulate an 'OrderShipped' event
            var orderId = Guid.NewGuid();
            var eventMessage = new OrderShippedEvent { OrderId = orderId, ShippedDate = DateTime.UtcNow };

            // Act: Publish the event to MassTransit bus using InMemory Test Harness
            await _busControl.Publish(eventMessage);

            // Assert: Verify that the notification was sent and recorded in the write DB
            var notificationCollection = _writeDb.GetCollection<Notification>("Notifications");
            var notification = await notificationCollection.Find(n => n.OrderId == orderId).FirstOrDefaultAsync();

            // Assert Notification is sent
            Assert.NotNull(notification);
            Assert.Equal(orderId, notification.OrderId);
        }

        [Fact]
        public async Task Should_Send_Notification_On_PaymentCompleted_Event()
        {
            // Arrange: Simulate a 'PaymentCompleted' event
            var orderId = Guid.NewGuid();
            var eventMessage = new PaymentCompletedEvent { OrderId = orderId, PaymentDate = DateTime.UtcNow };

            // Act: Publish the event to MassTransit bus using InMemory Test Harness
            await _busControl.Publish(eventMessage);

            // Assert: Verify that the notification was sent and recorded in the write DB
            var notificationCollection = _writeDb.GetCollection<Notification>("Notifications");
            var notification = await notificationCollection.Find(n => n.OrderId == orderId).FirstOrDefaultAsync();

            // Assert Notification is sent
            Assert.NotNull(notification);
            Assert.Equal(orderId, notification.OrderId);
        }

        [Fact]
        public async Task Should_Log_Error_On_FailedNotification_Send()
        {
            // Arrange: Simulate a scenario where the notification sending fails
            var orderId = Guid.NewGuid();
            var eventMessage = new OrderShippedEvent { OrderId = orderId, ShippedDate = DateTime.UtcNow };

            // Act: Publish the event to MassTransit bus using InMemory Test Harness
            await _busControl.Publish(eventMessage);

            // Simulate a failure (optional)
            // You can mock a failure, e.g., by throwing an exception inside a handler

            // Assert: Verify that an error log is recorded in the read DB
            var logCollection = _readDb.GetCollection<Log>("ErrorLogs");
            var logEntry = await logCollection.Find(log => log.OrderId == orderId && log.Level == "Error").FirstOrDefaultAsync();

            // Assert Error Log is created
            Assert.NotNull(logEntry);
            Assert.Equal("Error", logEntry.Level);
        }
    }

    // Event and Log classes for testing purposes
}

