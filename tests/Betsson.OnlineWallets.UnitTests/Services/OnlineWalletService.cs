using Moq;
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.Models;

namespace Betsson.OnlineWallets.UnitTests.Services
{
    public class OnlineWalletServiceTests
    {
        private readonly Mock<IOnlineWalletRepository> _repositoryMock;
        private readonly OnlineWalletService _service;

        public OnlineWalletServiceTests()
        {
            // Initialize the mock repository
            _repositoryMock = new Mock<IOnlineWalletRepository>();

            // Create an instance of OnlineWalletService with the mock repository
            _service = new OnlineWalletService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetBalanceAsync_NoTransactions_ReturnsZeroBalance()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync((OnlineWalletEntry?)null);

            // Act
            Balance balance = await _service.GetBalanceAsync();

            // Assert
            Assert.Equal(0, balance.Amount);
        }

        [Fact]
        public async Task DepositFundsAsync_ValidDeposit_IncreasesBalance()
        {
            // Arrange
            decimal initialAmount = 100m;
            decimal depositAmount = 50m;

            // Mock
            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 0, Amount = initialAmount });
            
            Deposit deposit = new() { Amount = depositAmount };

            // Act
            Balance newBalance = await _service.DepositFundsAsync(deposit);

            // Assert
            Assert.Equal(initialAmount + depositAmount, newBalance.Amount);

            // Verify
            _repositoryMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()), Times.Once);
        }

        [Fact]
        public async Task WithdrawFundsAsync_ValidWithdrawal_DecreasesBalance()
        {
            // Arrange
            decimal initialAmount = 100m;
            decimal withdrawalAmount = 50m;

            // Mock
            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 0, Amount = initialAmount });
            
            Withdrawal withdrawal = new() { Amount = withdrawalAmount };

            // Act
            Balance newBalance = await _service.WithdrawFundsAsync(withdrawal);

            // Assert
            Assert.Equal(initialAmount - withdrawalAmount, newBalance.Amount);

            // Verify
            _repositoryMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()), Times.Once);
        }

        [Fact]
        public async Task WithdrawFundsAsync_InsufficientBalance_ThrowsException()
        {
            // Arrange
            decimal initialAmount = 50m;
            decimal withdrawalAmount = 100m;

            // Mock
            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 0, Amount = initialAmount });
            
            Withdrawal withdrawal = new() { Amount = withdrawalAmount };

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientBalanceException>(
                () => _service.WithdrawFundsAsync(withdrawal));
        }
        
        [Fact]
        public async Task DepositFundsAsync_WithDecimalAmount_ValidProcess()
        {
            // Arrange
            decimal initialAmount = 100.235m;
            decimal depositAmount = 50.567m;

            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 0, Amount = initialAmount });

            Deposit deposit = new() { Amount = depositAmount };
            // Act
            Balance newBalance = await _service.DepositFundsAsync(deposit);

            // Assert
            Assert.Equal(initialAmount + depositAmount, newBalance.Amount);
        }
        
        [Fact]
        public async Task Withdraw_WithDecimalAmount_ValidProcess()
        {
            // Arrange
            decimal initialAmount = 200.789m;
            decimal withdrawalAmount = 50.345m;

            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 0, Amount = initialAmount });
            
            Withdrawal withdrawal = new() { Amount = withdrawalAmount };

            // Act
            Balance newBalance = await _service.WithdrawFundsAsync(withdrawal);

            // Assert
            Assert.Equal(initialAmount - withdrawalAmount, newBalance.Amount);
        }
        
    }
}