namespace ServicesApp.Models;

public static class AppPermissions
{
    public static List<string> All = new()
    {
        "Users.View", "Users.Manage",
        "Services.View", "Services.Manage",
        "Orders.View", "Orders.Manage",
        "Categories.Manage",
        "Settings.Edit",
        "Payments.View",
        "Tickets.Manage",
        "Roles.Manage"
    };
}
