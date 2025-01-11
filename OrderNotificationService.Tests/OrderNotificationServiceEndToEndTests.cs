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
            _driver.Navigate().GoToUrl("put_your_url_here");

            var orderShippedButton = _driver.FindElement(By.Id("orderShippedButton"));
            orderShippedButton.Click();

            Thread.Sleep(2000); // Wait for 2 seconds

            var notificationMessage = _driver.FindElement(By.ClassName("notificationMessage"));
            Assert.NotNull(notificationMessage);
            Assert.Contains("Your order has been shipped", notificationMessage.Text);
        }

        [Fact]
        public void Should_Send_Notification_On_PaymentCompleted_Event()
        {

            _driver.Navigate().GoToUrl("put_your_url_here");

            var paymentCompletedButton = _driver.FindElement(By.Id("paymentCompletedButton"));
            paymentCompletedButton.Click();

            Thread.Sleep(2000);

            var notificationMessage = _driver.FindElement(By.ClassName("notificationMessage"));
            Assert.NotNull(notificationMessage);
            Assert.Contains("Payment completed successfully", notificationMessage.Text);
        }

        [Fact]
        public void Should_Log_Error_On_FailedNotification_Send()
        {
            _driver.Navigate().GoToUrl("put_your_url_here");

            var orderShippedButton = _driver.FindElement(By.Id("orderShippedButton"));
            orderShippedButton.Click();


            Thread.Sleep(2000); 

            var errorLog = _driver.FindElement(By.ClassName("errorLog"));
            Assert.NotNull(errorLog);
            Assert.Contains("Error", errorLog.Text);
        }
    }
}
