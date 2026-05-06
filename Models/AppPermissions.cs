namespace ServicesApp.Models;

public static class AppPermissions
{
    public static List<string> All = new()
    {
        // Users
        "Users.View", "Users.Create", "Users.Edit", "Users.Delete", "Users.ManageRoles", "Users.ToggleStatus",
        
        // KYC
        "Kyc.View", "Kyc.Approve", "Kyc.Reject",
        
        // Categories
        "Categories.View", "Categories.Create", "Categories.Edit", "Categories.Delete",
        
        // Services
        "Services.View", "Services.Create", "Services.Edit", "Services.Delete", "Services.ToggleStatus",
        
        // Orders
        "Orders.View", "Orders.Create", "Orders.Edit", "Orders.Delete", "Orders.UpdateStatus", "Orders.Dispute",
        
        // Payments
        "Payments.View", "Payments.Refund",
        
        // Support
        "Tickets.View", "Tickets.Reply", "Tickets.Close",
        
        // Chat Monitoring
        "Chat.View",
        
        // System
        "Dashboard.View",
        "Settings.View", "Settings.Edit", "Settings.Email",
        "Roles.View", "Roles.Manage"
    };
}
