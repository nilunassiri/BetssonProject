
using Xunit;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net;
using Betsson.OnlineWallets.Models;

namespace Betsson.OnlineWallets.E2ETests.Services
{
    /// <summary>
    /// Contains end-to-end tests for the Online Wallet Controller.
    /// </summary>
    public class OnlineWalletControllerTests : IAsyncLifetime
    {
        private readonly HttpClient _client;
        
        public OnlineWalletControllerTests()
        {
            // Set up configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("Base URL is not configured.");
            }

            // Initialize HttpClient with the base URL from the configuration
            _client = new HttpClient { BaseAddress = new System.Uri(baseUrl) };
        }

        /// <summary>
        /// Initializes the test case by ensuring there is a zero balance.
        /// </summary>
        public async Task InitializeAsync()
        {
            await EnsureZeroBalance();
        }

        /// <summary>
        /// Ensures the online wallet balance is zero before running tests.
        /// Withdraws any existing balance.
        /// </summary>
        private async Task EnsureZeroBalance()
        {
            var response = await _client.GetAsync("/OnlineWallet/Balance");
            response.EnsureSuccessStatusCode();
            var balance = await response.Content.ReadFromJsonAsync<Balance>();

            if (balance?.Amount > 0)
            {
                var withdrawRequest = new { Amount = balance.Amount };
                await _client.PostAsJsonAsync("/OnlineWallet/Withdraw", withdrawRequest);
            }
        }

        /// <summary>
        /// Disposes the resources used by the test case.
        /// </summary>
        public Task DisposeAsync() => Task.CompletedTask;

        /// <summary>
        /// Tests if the GetBalance endpoint returns a non-empty balance.
        /// </summary>
        [Fact]
        public async Task GetBalance_ShouldReturnBalance()
        {
            // Act
            var response = await _client.GetAsync("/OnlineWallet/Balance");
            response.EnsureSuccessStatusCode();

            // Assert
            var balanceResponse = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(balanceResponse);
        }

        /// <summary>
        /// Tests if depositing a positive amount increases the wallet balance.
        /// </summary>
        [Fact]
        public async Task Deposit_ShouldIncreaseBalance()
        {
            // Arrange
            var depositRequest = new { Amount = 100m };

            // Act
            var depositResponse = await _client.PostAsJsonAsync("/OnlineWallet/Deposit", depositRequest);
            depositResponse.EnsureSuccessStatusCode();

            // Assert
            var balanceResponse = await _client.GetFromJsonAsync<Balance>("/OnlineWallet/Balance");
            Assert.NotNull(balanceResponse);
            Assert.True(balanceResponse!.Amount > 0);
        }

        /// <summary>
        /// Tests if depositing a zero amount keeps the balance unchanged.
        /// </summary>
        [Fact]
        public async Task Deposit_ZeroAmount_ShouldNotChangeBalance()
        {
            // Arrange
            var depositRequest = new { Amount = 0m };

            // Act
            var depositResponse = await _client.PostAsJsonAsync("/OnlineWallet/Deposit", depositRequest);
            depositResponse.EnsureSuccessStatusCode();

            // Assert
            var balanceResponse = await _client.GetFromJsonAsync<Balance>("/OnlineWallet/Balance");
            Assert.NotNull(balanceResponse);
            Assert.Equal(0m, balanceResponse!.Amount);
        }

        /// <summary>
        /// Tests if depositing a negative amount returns a BadRequest status.
        /// </summary>
        [Fact]
        public async Task Deposit_NegativeAmount_ShouldFail()
        {
            // Arrange
            var depositRequest = new { Amount = -100m };

            // Act
            var depositResponse = await _client.PostAsJsonAsync("/OnlineWallet/Deposit", depositRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, depositResponse.StatusCode);
        }

        /// <summary>
        /// Tests if withdrawing more than the current balance returns a BadRequest status.
        /// </summary>
        [Fact]
        public async Task Withdraw_MoreThanBalance_ShouldFail()
        {
            // Arrange
            var withdrawalRequest = new { Amount = 1000m };

            // Act
            var withdrawalResponse = await _client.PostAsJsonAsync("/OnlineWallet/Withdraw", withdrawalRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, withdrawalResponse.StatusCode);
        }

        /// <summary>
        /// Tests if withdrawing the exact balance results in a zero balance.
        /// </summary>
        [Fact]
        public async Task Withdraw_ExactBalance_ShouldReturnZeroBalance()
        {
            // Arrange
            var depositRequest = new { Amount = 50m };
            await _client.PostAsJsonAsync("/OnlineWallet/Deposit", depositRequest);
            
            var withdrawalRequest = new { Amount = 50m };

            // Act
            var withdrawalResponse = await _client.PostAsJsonAsync("/OnlineWallet/Withdraw", withdrawalRequest);
            withdrawalResponse.EnsureSuccessStatusCode();

            // Assert
            var balanceResponse = await _client.GetFromJsonAsync<Balance>("/OnlineWallet/Balance");
            Assert.NotNull(balanceResponse);
            Assert.Equal(0m, balanceResponse!.Amount);
        }

        /// <summary>
        /// Tests if consecutive deposit and withdrawal operations return the correct balance.
        /// </summary>
        [Fact]
        public async Task DepositAndWithdraw_ConsecutiveOperations_ShouldReturnCorrectBalance()
        {
            // Arrange
            var depositRequest = new { Amount = 200m };
            var depositResponse = await _client.PostAsJsonAsync("/OnlineWallet/Deposit", depositRequest);
            depositResponse.EnsureSuccessStatusCode();
            
            var withdrawalRequest = new { Amount = 50m };

            // Act
            var withdrawalResponse = await _client.PostAsJsonAsync("/OnlineWallet/Withdraw", withdrawalRequest);
            withdrawalResponse.EnsureSuccessStatusCode();

            // Assert
            var balance = await _client.GetFromJsonAsync<Balance>("/OnlineWallet/Balance");
            Assert.NotNull(balance);
            Assert.Equal(150m, balance!.Amount);
        }
        
        /// <summary>
        /// Tests if depositing null is accepted
        /// </summary>
        [Fact]
        public async Task NullDeposit_ShouldFail()
        {
            // Arrange
            var depositResponse = await _client.PostAsJsonAsync("/OnlineWallet/Deposit", new { });

            // Assert
            Assert.Equal(HttpStatusCode.OK, depositResponse.StatusCode);
        }

        /// <summary>
        /// Tests if withdrawing null is accepted
        /// </summary>
        [Fact]
        public async Task NullWithdraw_ShouldFail()
        {
            // Arrange
            var withdrawalResponse = await _client.PostAsJsonAsync("/OnlineWallet/Withdraw", new { });

            // Assert
            Assert.Equal(HttpStatusCode.OK, withdrawalResponse.StatusCode);
        }
    }
}