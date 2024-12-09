using Xunit;
using System.Net.Http;
using System.Net.Http.Json;

namespace Betsson.OnlineWallets.E2ETests.Services
{
    public class OnlineWalletControllerTests
    {
        private readonly HttpClient _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5002") };

        [Fact]
        public async Task GetBalance_ShouldReturnBalance()
        {
            // Act
            var response = await _client.GetAsync("/OnlineWallet/Balance");

            // Assert
            response.EnsureSuccessStatusCode();
            var balanceResponse = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(balanceResponse);
        }

        [Fact]
        public async Task Deposit_ShouldIncreaseBalance()
        {
            // Arrange
            var depositRequest = new { Amount = 100m };  // Adjust to match your DepositRequest model

            // Act
            var depositResponse = await _client.PostAsJsonAsync("/OnlineWallet/Deposit", depositRequest);

            // Assert
            depositResponse.EnsureSuccessStatusCode();
            var balanceResponse = await depositResponse.Content.ReadAsStringAsync();
            Assert.NotEmpty(balanceResponse); // add more logic to check updated balance
        }
        
        [Fact]
        public async Task Withdraw_ShouldDecreaseBalance()
        {
            // Arrange
            var withdrawalRequest = new { Amount = 50m };  // Adjust to match your WithdrawalRequest model

            // Act
            var withdrawalResponse = await _client.PostAsJsonAsync("/OnlineWallet/Withdraw", withdrawalRequest);

            // Assert
            withdrawalResponse.EnsureSuccessStatusCode();
            var newBalanceResponse = await withdrawalResponse.Content.ReadAsStringAsync();
            Assert.NotEmpty(newBalanceResponse); // Add more specific checks to ensure balance decreased
        }
    }
}