using System;
using System.Collections.Generic;
using System.Linq;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.Entities;

public class GroupBooking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LeaderUserId { get; set; }
    public Guid FlightId { get; set; }
    public Guid? ReturnFlightId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public GroupBookingStatus Status { get; set; } = GroupBookingStatus.Active;
    public SplitStrategy SplitStrategy { get; set; } = SplitStrategy.ByPassenger;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    public void AddMember(GroupMember member)
    {
        if (Status != GroupBookingStatus.Active)
        {
            throw new InvalidOperationException("Cannot add member to inactive group booking.");
        }

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = GroupBookingStatus.Expired;
            throw new InvalidOperationException("Group booking session has expired.");
        }

        if (Members.Any(m => m.PassengerEmail.Equals(member.PassengerEmail, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Passenger email already registered in this group.");
        }

        member.GroupBookingId = Id;
        Members.Add(member);
        RecalculateAmounts();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SelectMemberSeat(Guid memberId, string seatNumber, decimal seatPrice, bool isReturn = false)
    {
        if (Status != GroupBookingStatus.Active)
        {
            throw new InvalidOperationException("Cannot select seat for inactive group booking.");
        }

        var member = Members.FirstOrDefault(m => m.Id == memberId);
        if (member == null)
        {
            throw new InvalidOperationException($"Group member {memberId} not found.");
        }

        if (member.PaymentStatus == MemberPaymentStatus.Paid)
        {
            throw new InvalidOperationException("Cannot change seat after payment is completed.");
        }

        // Validate no other member in group took the seat
        if (isReturn)
        {
            if (Members.Any(m => m.Id != memberId && m.ReturnSeatNumber == seatNumber))
            {
                throw new InvalidOperationException($"Return seat {seatNumber} is already selected by another group member.");
            }
            member.ReturnSeatNumber = seatNumber;
        }
        else
        {
            if (Members.Any(m => m.Id != memberId && m.SeatNumber == seatNumber))
            {
                throw new InvalidOperationException($"Seat {seatNumber} is already selected by another group member.");
            }
            member.SeatNumber = seatNumber;
        }

        RecalculateAmounts();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyMemberPayment(Guid memberId, decimal amount, string transactionId, PaymentProvider provider)
    {
        if (Status != GroupBookingStatus.Active)
        {
            throw new InvalidOperationException("Cannot process payment for inactive group booking.");
        }

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = GroupBookingStatus.Expired;
            throw new InvalidOperationException("Group booking session has expired.");
        }

        var member = Members.FirstOrDefault(m => m.Id == memberId);
        if (member == null)
        {
            throw new InvalidOperationException($"Group member {memberId} not found.");
        }

        if (member.PaymentStatus == MemberPaymentStatus.Paid)
        {
            throw new InvalidOperationException("Group member has already paid.");
        }

        member.PaidAmount = amount;
        member.PaymentStatus = MemberPaymentStatus.Paid;
        member.PaymentTransactionId = transactionId;
        member.PaymentProvider = provider;
        member.PaidAt = DateTime.UtcNow;

        PaidAmount = Members.Sum(m => m.PaidAmount);

        if (CheckFullyPaid())
        {
            Status = GroupBookingStatus.FullyPaid;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public bool CheckFullyPaid()
    {
        return Members.Count > 0 && Members.All(m => m.PaymentStatus == MemberPaymentStatus.Paid);
    }

    public void Cancel()
    {
        if (Status == GroupBookingStatus.FullyPaid)
        {
            throw new InvalidOperationException("Cannot cancel fully paid group booking.");
        }

        Status = GroupBookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        if (Status == GroupBookingStatus.Active)
        {
            Status = GroupBookingStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RecalculateAmounts()
    {
        if (SplitStrategy == SplitStrategy.Equal && Members.Count > 0)
        {
            var share = Math.Round(TotalAmount / Members.Count, 2);
            foreach (var member in Members)
            {
                if (member.PaymentStatus != MemberPaymentStatus.Paid)
                {
                    member.AssignedAmount = share;
                }
            }
        }
    }
}
