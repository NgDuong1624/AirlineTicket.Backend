using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Domain.ValueObjects;

namespace AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;

public sealed record InitiateMemberPaymentCommand(
    string InviteCode,
    Guid MemberId,
    PaymentProvider Provider,
    string ReturnUrl,
    string CancelUrl
) : ICommand<Result<ProcessMemberPaymentResult>>;

public class InitiateMemberPaymentCommandHandler : ICommandHandler<InitiateMemberPaymentCommand, Result<ProcessMemberPaymentResult>>
{
    private readonly IGroupBookingRepository _repository;
    private readonly IPaymentGatewayFactory _gatewayFactory;

    public InitiateMemberPaymentCommandHandler(
        IGroupBookingRepository repository,
        IPaymentGatewayFactory gatewayFactory)
    {
        _repository = repository;
        _gatewayFactory = gatewayFactory;
    }

    public async Task<Result<ProcessMemberPaymentResult>> Handle(InitiateMemberPaymentCommand request, CancellationToken cancellationToken)
    {
        var groupBooking = await _repository.GetByInviteCodeAsync(request.InviteCode.Trim().ToUpperInvariant(), cancellationToken);
        if (groupBooking == null)
        {
            return Result.Failure<ProcessMemberPaymentResult>(new Error("GroupBooking.NotFound", "Group booking lobby not found."));
        }

        if (groupBooking.Status != GroupBookingStatus.Active || DateTime.UtcNow > groupBooking.ExpiresAt)
        {
            return Result.Failure<ProcessMemberPaymentResult>(new Error("GroupBooking.Inactive", "Group booking session is inactive or expired."));
        }

        var member = groupBooking.Members.FirstOrDefault(m => m.Id == request.MemberId);
        if (member == null)
        {
            return Result.Failure<ProcessMemberPaymentResult>(new Error("GroupMember.NotFound", "Group member not found."));
        }

        if (member.PaymentStatus == MemberPaymentStatus.Paid)
        {
            return Result.Failure<ProcessMemberPaymentResult>(new Error("GroupMember.AlreadyPaid", "Member share already paid."));
        }

        var amountToPay = member.AssignedAmount > 0 ? member.AssignedAmount : Math.Round(groupBooking.TotalAmount / Math.Max(1, groupBooking.Members.Count), 2);
        var currencyEnum = Enum.TryParse<Currency>(groupBooking.Currency, true, out var parsedCurrency)
            ? parsedCurrency
            : Currency.VND;

        var money = new Money(amountToPay, currencyEnum);
        var gateway = _gatewayFactory.GetGateway(request.Provider);

        var checkoutReq = new PaymentCheckoutRequest(
            BookingId: $"GRP-{groupBooking.InviteCode}-{member.Id:N}",
            Money: money,
            ReturnUrl: request.ReturnUrl,
            CancelUrl: request.CancelUrl,
            Description: $"Split Payment - {groupBooking.GroupName} - {member.PassengerName}",
            CustomerEmail: member.PassengerEmail,
            CustomerName: member.PassengerName
        );

        var paymentResult = await gateway.CreatePaymentUrlAsync(checkoutReq, cancellationToken);

        return Result.Success(new ProcessMemberPaymentResult(
            MemberId: member.Id,
            PaymentUrl: paymentResult.PaymentUrl,
            ClientSecret: paymentResult.ClientSecret,
            Status: "Pending",
            IsGroupFullyPaid: groupBooking.CheckFullyPaid()
        ));
    }
}

public sealed record ConfirmMemberPaymentCommand(
    string InviteCode,
    Guid MemberId,
    string TransactionId,
    PaymentProvider Provider,
    decimal Amount
) : ICommand<Result<ProcessMemberPaymentResult>>;

public class ConfirmMemberPaymentCommandHandler : ICommandHandler<ConfirmMemberPaymentCommand, Result<ProcessMemberPaymentResult>>
{
    private readonly IGroupBookingRepository _repository;

    public ConfirmMemberPaymentCommandHandler(IGroupBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProcessMemberPaymentResult>> Handle(ConfirmMemberPaymentCommand request, CancellationToken cancellationToken)
    {
        var groupBooking = await _repository.GetByInviteCodeAsync(request.InviteCode.Trim().ToUpperInvariant(), cancellationToken);
        if (groupBooking == null)
        {
            return Result.Failure<ProcessMemberPaymentResult>(new Error("GroupBooking.NotFound", "Group booking lobby not found."));
        }

        try
        {
            groupBooking.ApplyMemberPayment(request.MemberId, request.Amount, request.TransactionId, request.Provider);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<ProcessMemberPaymentResult>(new Error("GroupBooking.PaymentFailed", ex.Message));
        }

        await _repository.UpdateAsync(groupBooking, cancellationToken);

        return Result.Success(new ProcessMemberPaymentResult(
            MemberId: request.MemberId,
            PaymentUrl: null,
            ClientSecret: null,
            Status: "Paid",
            IsGroupFullyPaid: groupBooking.Status == GroupBookingStatus.FullyPaid
        ));
    }
}
