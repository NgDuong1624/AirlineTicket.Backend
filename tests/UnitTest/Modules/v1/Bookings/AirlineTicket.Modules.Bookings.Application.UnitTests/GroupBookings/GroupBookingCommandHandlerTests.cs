using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests.GroupBookings;

public class GroupBookingCommandHandlerTests
{
    private readonly Mock<IGroupBookingRepository> _repoMock;
    private readonly Mock<IFlightSeatReservation> _seatReservationMock;

    public GroupBookingCommandHandlerTests()
    {
        _repoMock = new Mock<IGroupBookingRepository>();
        _seatReservationMock = new Mock<IFlightSeatReservation>();
    }

    [Fact]
    public async Task CreateGroupBooking_ShouldCreateActiveLobbyWithInviteCode()
    {
        var handler = new CreateGroupBookingCommandHandler(_repoMock.Object, _seatReservationMock.Object);
        var leaderId = Guid.NewGuid();
        var flightId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByInviteCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupBooking?)null);

        var cmd = new CreateGroupBookingCommand(
            LeaderUserId: leaderId,
            LeaderName: "Alex Group Leader",
            LeaderEmail: "alex@example.com",
            LeaderPhone: "0901234567",
            FlightId: flightId,
            ReturnFlightId: null,
            GroupName: "Danang Vacation",
            SplitStrategy: SplitStrategy.ByPassenger,
            TotalAmount: 5000000,
            Currency: "VND",
            LeaderSeatNumber: "12A"
        );

        _seatReservationMock.Setup(s => s.ReserveSeatsAsync(flightId, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ReservedSeat> { ["12A"] = new ReservedSeat(Guid.NewGuid(), 2500000) });

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.InviteCode.Should().NotBeNullOrWhiteSpace();
        result.Value.InviteCode.Length.Should().Be(6);
        _repoMock.Verify(r => r.AddAsync(It.Is<GroupBooking>(g => g.GroupName == "Danang Vacation" && g.Members.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinGroupBooking_ShouldAddMemberToActiveLobby()
    {
        var handler = new JoinGroupBookingCommandHandler(_repoMock.Object);
        var group = new GroupBooking
        {
            Id = Guid.NewGuid(),
            InviteCode = "GRP123",
            GroupName = "Beach Trip",
            Status = GroupBookingStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddHours(12)
        };

        _repoMock.Setup(r => r.GetByInviteCodeAsync("GRP123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var cmd = new JoinGroupBookingCommand("GRP123", Guid.NewGuid(), "Taylor Swift", "taylor@example.com", "0912345678");
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PassengerName.Should().Be("Taylor Swift");
        group.Members.Should().ContainSingle(m => m.PassengerEmail == "taylor@example.com");
    }

    [Fact]
    public async Task ConfirmMemberPayment_ShouldMarkPaidAndCompleteGroupWhenAllPaid()
    {
        var handler = new ConfirmMemberPaymentCommandHandler(_repoMock.Object);
        var memberId = Guid.NewGuid();
        var member = new GroupMember
        {
            Id = memberId,
            PassengerName = "Solo Passenger",
            PassengerEmail = "solo@example.com",
            AssignedAmount = 2500000,
            PaymentStatus = MemberPaymentStatus.Pending
        };

        var group = new GroupBooking
        {
            Id = Guid.NewGuid(),
            InviteCode = "PAID99",
            GroupName = "Solo Group",
            TotalAmount = 2500000,
            Status = GroupBookingStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddHours(6),
            Members = new List<GroupMember> { member }
        };

        _repoMock.Setup(r => r.GetByInviteCodeAsync("PAID99", It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var cmd = new ConfirmMemberPaymentCommand("PAID99", memberId, "TXN_12345", PaymentProvider.VNPay, 2500000);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Paid");
        result.Value.IsGroupFullyPaid.Should().BeTrue();
        group.Status.Should().Be(GroupBookingStatus.FullyPaid);
    }
}
