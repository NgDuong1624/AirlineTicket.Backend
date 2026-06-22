using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AirlineTicket.Api.Realtime;

public class CustomerChatSession
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid AirlineId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? AssignedStaffConnectionId { get; set; }
}

public class StaffSession
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid AirlineId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public HashSet<string> ActiveCustomerConnectionIds { get; } = new();
}

public class SupportChatHub : Hub
{
    // In-memory sessions storage (ideal for modular monolith / prototype deployment)
    private static readonly ConcurrentDictionary<string, CustomerChatSession> ActiveCustomers = new();
    private static readonly ConcurrentDictionary<string, StaffSession> OnlineStaff = new();

    /// <summary>
    /// Customers join the chat by selecting an Airline ID.
    /// </summary>
    public async Task CustomerJoinChat(Guid airlineId, string customerName)
    {
        var connectionId = Context.ConnectionId;
        var session = new CustomerChatSession
        {
            ConnectionId = connectionId,
            AirlineId = airlineId,
            CustomerName = string.IsNullOrWhiteSpace(customerName) ? "Khách hàng" : customerName
        };

        ActiveCustomers[connectionId] = session;

        await Clients.Caller.SendAsync("SystemMessage", "Chào mừng bạn! Hệ thống đang tìm kiếm nhân viên hỗ trợ...");

        // Try to automatically assign a staff member if available
        var assigned = await TryAssignStaffToCustomer(session);
        if (!assigned)
        {
            await Clients.Caller.SendAsync("SystemMessage", "Hiện chưa có nhân viên nào trực tuyến. Bạn có thể trò chuyện với AI Assistant trong thời gian chờ đợi.");
        }
    }

    /// <summary>
    /// Staff members register as online for a specific airline.
    /// </summary>
    public async Task StaffRegister(Guid airlineId, string staffName)
    {
        var connectionId = Context.ConnectionId;
        var session = new StaffSession
        {
            ConnectionId = connectionId,
            AirlineId = airlineId,
            StaffName = string.IsNullOrWhiteSpace(staffName) ? "Nhân viên hỗ trợ" : staffName
        };

        OnlineStaff[connectionId] = session;

        await Clients.Caller.SendAsync("SystemMessage", "Bạn đã đăng ký trực tuyến thành công và sẵn sàng nhận tin nhắn hỗ trợ.");

        // Check if there are any unassigned customers waiting for this airline
        var waitingCustomer = ActiveCustomers.Values
            .FirstOrDefault(c => c.AirlineId == airlineId && string.IsNullOrEmpty(c.AssignedStaffConnectionId));

        if (waitingCustomer != null)
        {
            waitingCustomer.AssignedStaffConnectionId = connectionId;
            session.ActiveCustomerConnectionIds.Add(waitingCustomer.ConnectionId);

            await Clients.Client(waitingCustomer.ConnectionId).SendAsync("AgentAssigned", session.StaffName);
            await Clients.Caller.SendAsync("NewCustomerChat", waitingCustomer.ConnectionId, waitingCustomer.CustomerName);
            await Clients.Client(waitingCustomer.ConnectionId).SendAsync("SystemMessage", $"Nhân viên {session.StaffName} đã tham gia cuộc hội thoại.");
        }
    }

    /// <summary>
    /// Customer sends a message to the assigned staff member.
    /// </summary>
    public async Task SendMessageToAirline(string message)
    {
        var connectionId = Context.ConnectionId;
        if (!ActiveCustomers.TryGetValue(connectionId, out var customer))
        {
            await Clients.Caller.SendAsync("SystemMessage", "Lỗi: Phiên trò chuyện chưa bắt đầu.");
            return;
        }

        // Auto-assign if not yet assigned (or if assigned agent disconnected)
        if (string.IsNullOrEmpty(customer.AssignedStaffConnectionId) || !OnlineStaff.ContainsKey(customer.AssignedStaffConnectionId))
        {
            var assigned = await TryAssignStaffToCustomer(customer);
            if (!assigned)
            {
                await Clients.Caller.SendAsync("SystemMessage", "Hiện không có nhân viên trực tuyến. Tin nhắn của bạn không thể gửi đi.");
                return;
            }
        }

        var staffConnectionId = customer.AssignedStaffConnectionId;
        if (staffConnectionId != null)
        {
            await Clients.Client(staffConnectionId).SendAsync("ReceiveMessage", connectionId, "Customer", customer.CustomerName, message);
            await Clients.Caller.SendAsync("ReceiveMessage", connectionId, "Customer", customer.CustomerName, message);
        }
    }

    /// <summary>
    /// Staff sends a message to a specific customer.
    /// </summary>
    public async Task SendMessageToCustomer(string customerConnectionId, string message)
    {
        var connectionId = Context.ConnectionId;
        if (!OnlineStaff.TryGetValue(connectionId, out var staff))
        {
            await Clients.Caller.SendAsync("SystemMessage", "Lỗi: Bạn chưa đăng ký làm nhân viên hỗ trợ.");
            return;
        }

        if (ActiveCustomers.TryGetValue(customerConnectionId, out var customer))
        {
            await Clients.Client(customerConnectionId).SendAsync("ReceiveMessage", connectionId, "Staff", staff.StaffName, message);
            await Clients.Caller.SendAsync("ReceiveMessage", customerConnectionId, "Staff", staff.StaffName, message);
        }
        else
        {
            await Clients.Caller.SendAsync("SystemMessage", "Khách hàng này đã ngắt kết nối.");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        // If a customer disconnected
        if (ActiveCustomers.TryRemove(connectionId, out var customer))
        {
            if (!string.IsNullOrEmpty(customer.AssignedStaffConnectionId))
            {
                if (OnlineStaff.TryGetValue(customer.AssignedStaffConnectionId, out var staff))
                {
                    staff.ActiveCustomerConnectionIds.Remove(connectionId);
                    await Clients.Client(customer.AssignedStaffConnectionId).SendAsync("CustomerDisconnected", connectionId, customer.CustomerName);
                }
            }
        }

        // If a staff disconnected
        if (OnlineStaff.TryRemove(connectionId, out var staffSession))
        {
            foreach (var custId in staffSession.ActiveCustomerConnectionIds)
            {
                if (ActiveCustomers.TryGetValue(custId, out var cust))
                {
                    cust.AssignedStaffConnectionId = null;
                    await Clients.Client(custId).SendAsync("SystemMessage", "Nhân viên hỗ trợ đã ngắt kết nối. Hệ thống đang tìm kiếm nhân viên khác...");

                    // Attempt to re-assign right away
                    await TryAssignStaffToCustomer(cust);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> TryAssignStaffToCustomer(CustomerChatSession customer)
    {
        // Find a random online staff of the selected airline
        var availableStaff = OnlineStaff.Values
            .Where(s => s.AirlineId == customer.AirlineId)
            .ToList();

        if (!availableStaff.Any())
        {
            return false;
        }

        // Select staff member with the fewest active chats to distribute load, or just random
        var random = new Random();
        var selectedStaff = availableStaff[random.Next(availableStaff.Count)];

        customer.AssignedStaffConnectionId = selectedStaff.ConnectionId;
        selectedStaff.ActiveCustomerConnectionIds.Add(customer.ConnectionId);

        await Clients.Client(customer.ConnectionId).SendAsync("AgentAssigned", selectedStaff.StaffName);
        await Clients.Client(selectedStaff.ConnectionId).SendAsync("NewCustomerChat", customer.ConnectionId, customer.CustomerName);
        await Clients.Client(customer.ConnectionId).SendAsync("SystemMessage", $"Nhân viên {selectedStaff.StaffName} đã tham gia cuộc hội thoại.");

        return true;
    }
}
