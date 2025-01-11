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
            var mongoDbUrl = "mongodb://root:1234@localhost:27017";
            var client = new MongoClient(new MongoUrl(mongoDbUrl));
            _writeDb = client.GetDatabase("OrderWriteDb");
            _readDb = client.GetDatabase("OrderReadDb");

            await _testHarness.Start();

            _busControl = _testHarness.Bus;
        }

        public async Task DisposeAsync()
        {
            await _testHarness.Stop();
        }

        [Fact]
        public async Task Should_Send_Notification_On_OrderShipped_Event()
        {
            var orderId = Guid.NewGuid();
            var eventMessage = new OrderShippedEvent { OrderId = orderId, ShippedDate = DateTime.UtcNow };

            await _busControl.Publish(eventMessage);

            var notificationCollection = _writeDb.GetCollection<Notification>("Notifications");
            var notification = await notificationCollection.Find(n => n.OrderId == orderId).FirstOrDefaultAsync();

            Assert.NotNull(notification);
            Assert.Equal(orderId, notification.OrderId);
        }

        [Fact]
        public async Task Should_Send_Notification_On_PaymentCompleted_Event()
        {
            var orderId = Guid.NewGuid();
            var eventMessage = new PaymentCompletedEvent { OrderId = orderId, PaymentDate = DateTime.UtcNow };

            await _busControl.Publish(eventMessage);

            var notificationCollection = _writeDb.GetCollection<Notification>("Notifications");
            var notification = await notificationCollection.Find(n => n.OrderId == orderId).FirstOrDefaultAsync();

            Assert.NotNull(notification);
            Assert.Equal(orderId, notification.OrderId);
        }

        [Fact]
        public async Task Should_Log_Error_On_FailedNotification_Send()
        {
            var orderId = Guid.NewGuid();
            var eventMessage = new OrderShippedEvent { OrderId = orderId, ShippedDate = DateTime.UtcNow };

            await _busControl.Publish(eventMessage);


            var logCollection = _readDb.GetCollection<Log>("ErrorLogs");
            var logEntry = await logCollection.Find(log => log.OrderId == orderId && log.Level == "Error").FirstOrDefaultAsync();

            Assert.NotNull(logEntry);
            Assert.Equal("Error", logEntry.Level);
        }
    }

}

