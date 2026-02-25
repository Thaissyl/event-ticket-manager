using EventTickets.Core.Entities;
using EventTickets.Core.Enums;
using EventTickets.Core.Interfaces;
using EventTickets.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventTickets.Tests.Unit.Services;

public class CartServiceTests
{
    [Fact]
    public async Task GetCartAsync_WhenEmpty_ReturnsEmptyResponse()
    {
        // Arrange
        var mockCartRepo = new Mock<ICartReservationRepository>();
        var mockTierRepo = new Mock<ITicketTierRepository>();

        mockCartRepo.Setup(r => r.GetBySessionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var service = new CartService(mockCartRepo.Object, mockTierRepo.Object);

        // Act
        var result = await service.GetCartAsync("test-session");

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
        result.TotalAmount.Should().Be(0);
    }
}
