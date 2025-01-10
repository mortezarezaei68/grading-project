using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderNotificationService.Tests.EndToEnd
{
    public class OrderNotificationServiceEndToEndTests : IAsyncLifetime
    {
        private IWebDriver _driver;

        public Task InitializeAsync()
        {
            // Initialize the ChromeDriver (browser instance)
            _driver = new ChromeDriver();
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            // Close the browser after tests
            _driver.Quit();
            return Task.CompletedTask;
        }

        [Fact]
        public void Should_Send_Notification_On_OrderShipped_Event()
        {
            // Arrange: Simulate an 'OrderShipped' event

            // Open the web application page
            _driver.Navigate().GoToUrl("put_your_url_here");

            // Simulate triggering the 'OrderShipped' event by clicking a button or performing any required action
            var orderShippedButton = _driver.FindElement(By.Id("orderShippedButton"));
            orderShippedButton.Click();

            // Wait for a while for the notification to appear (use a more robust wait in real applications)
            Thread.Sleep(2000); // Wait for 2 seconds

            // Assert: Verify that the notification was displayed on the page
            var notificationMessage = _driver.FindElement(By.ClassName("notificationMessage"));
            Assert.NotNull(notificationMessage);
            Assert.Contains("Your order has been shipped", notificationMessage.Text);
        }

        [Fact]
        public void Should_Send_Notification_On_PaymentCompleted_Event()
        {
            // Arrange: Simulate a 'PaymentCompleted' event

            // Open the web application page
            _driver.Navigate().GoToUrl("put_your_url_here");

            // Simulate triggering the 'PaymentCompleted' event
            var paymentCompletedButton = _driver.FindElement(By.Id("paymentCompletedButton"));
            paymentCompletedButton.Click();

            // Wait for a while for the notification to appear
            Thread.Sleep(2000); // Wait for 2 seconds

            // Assert: Verify that the notification was displayed on the page
            var notificationMessage = _driver.FindElement(By.ClassName("notificationMessage"));
            Assert.NotNull(notificationMessage);
            Assert.Contains("Payment completed successfully", notificationMessage.Text);
        }

        [Fact]
        public void Should_Log_Error_On_FailedNotification_Send()
        {
            // Arrange: Simulate a scenario where the notification sending fails
            var orderId = Guid.NewGuid();

            // Open the web application page
            _driver.Navigate().GoToUrl("put_your_url_here");

            // Simulate triggering the 'OrderShipped' event with failure scenario
            var orderShippedButton = _driver.FindElement(By.Id("orderShippedButton"));
            orderShippedButton.Click();

            // Simulate failure (e.g., via an error pop-up or failure in a background operation)

            // Wait for a while for the error to be logged
            Thread.Sleep(2000); // Wait for 2 seconds

            // Assert: Verify that an error log is displayed (or check error UI component)
            var errorLog = _driver.FindElement(By.ClassName("errorLog"));
            Assert.NotNull(errorLog);
            Assert.Contains("Error", errorLog.Text);
        }
    }
}
