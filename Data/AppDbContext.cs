using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Models.Entities;

namespace ServicesApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<KycRequest> KycRequests => Set<KycRequest>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser
        builder.Entity<ApplicationUser>()
            .Property(u => u.WalletBalance)
            .HasColumnType("decimal(18,2)");

        // Service price
        builder.Entity<Service>()
            .Property(s => s.Price)
            .HasColumnType("decimal(18,2)");

        // Order price
        builder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)");

        // Payment amount
        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

        // Order - Client relationship
        builder.Entity<Order>()
            .HasOne(o => o.Client)
            .WithMany(u => u.ClientOrders)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order - Executor relationship
        builder.Entity<Order>()
            .HasOne(o => o.Executor)
            .WithMany(u => u.ExecutorOrders)
            .HasForeignKey(o => o.ExecutorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message sender/receiver
        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // KycRequest - one-to-one
        builder.Entity<KycRequest>()
            .HasOne(k => k.User)
            .WithOne(u => u.KycRequest)
            .HasForeignKey<KycRequest>(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // TicketMessage sender
        builder.Entity<TicketMessage>()
            .HasOne(tm => tm.Sender)
            .WithMany()
            .HasForeignKey(tm => tm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Service - Executor
        builder.Entity<Service>()
            .HasOne(s => s.Executor)
            .WithMany(u => u.Services)
            .HasForeignKey(s => s.ExecutorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.Entity<Message>()
            .HasIndex(m => m.ConversationKey);

        builder.Entity<Notification>()
            .HasIndex(n => n.UserId);

        builder.Entity<Service>()
            .HasIndex(s => s.CategoryId);

        builder.Entity<Service>()
            .HasIndex(s => s.IsActive);
    }
}
