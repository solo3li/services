using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Services;

public class KycService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notifService;

    public KycService(AppDbContext db, NotificationService notifService)
    {
        _db = db;
        _notifService = notifService;
    }

    public async Task<KycRequest?> GetByUserIdAsync(string userId)
    {
        return await _db.KycRequests.FirstOrDefaultAsync(k => k.UserId == userId);
    }

    public async Task<KycRequest> SubmitAsync(string userId, string bio, string skills,
        string? idCardUrl, string? selfieUrl, string? paymentInfo)
    {
        var existing = await GetByUserIdAsync(userId);
        if (existing != null)
        {
            existing.Status = ExecutorStatus.Pending;
            existing.Bio = bio;
            existing.Skills = skills;
            existing.IdCardImageUrl = idCardUrl;
            existing.SelfieUrl = selfieUrl;
            existing.PaymentInfo = paymentInfo;
            existing.SubmittedAt = DateTime.UtcNow;
            existing.RejectionNote = null;
            await _db.SaveChangesAsync();
            return existing;
        }

        var kyc = new KycRequest
        {
            UserId = userId,
            Bio = bio,
            Skills = skills,
            IdCardImageUrl = idCardUrl,
            SelfieUrl = selfieUrl,
            PaymentInfo = paymentInfo
        };
        _db.KycRequests.Add(kyc);
        await _db.SaveChangesAsync();
        return kyc;
    }

    public async Task<bool> ApproveAsync(int kycId, UserManager<ApplicationUser> userManager)
    {
        var kyc = await _db.KycRequests.Include(k => k.User).FirstOrDefaultAsync(k => k.Id == kycId);
        if (kyc == null) return false;

        kyc.Status = ExecutorStatus.Approved;
        kyc.ReviewedAt = DateTime.UtcNow;
        kyc.User.IsExecutor = true;
        kyc.User.ExecutorStatus = ExecutorStatus.Approved;
        kyc.User.Bio = kyc.Bio;

        if (!await userManager.IsInRoleAsync(kyc.User, "Executor"))
            await userManager.AddToRoleAsync(kyc.User, "Executor");

        await _db.SaveChangesAsync();

        await _notifService.CreateAsync(kyc.UserId,
            "🎉 KYC Approved!",
            "Your executor account has been approved. You can now offer services!",
            NotificationType.Kyc,
            "/kyc/status");

        return true;
    }

    public async Task<bool> RejectAsync(int kycId, string reason)
    {
        var kyc = await _db.KycRequests.Include(k => k.User).FirstOrDefaultAsync(k => k.Id == kycId);
        if (kyc == null) return false;

        kyc.Status = ExecutorStatus.Rejected;
        kyc.ReviewedAt = DateTime.UtcNow;
        kyc.RejectionNote = reason;
        kyc.User.ExecutorStatus = ExecutorStatus.Rejected;

        await _db.SaveChangesAsync();

        await _notifService.CreateAsync(kyc.UserId,
            "KYC Application Rejected",
            $"Your KYC was rejected: {reason}",
            NotificationType.Kyc,
            "/kyc/submit");

        return true;
    }

    public async Task<List<KycRequest>> GetPendingAsync()
    {
        return await _db.KycRequests
            .Include(k => k.User)
            .Where(k => k.Status == ExecutorStatus.Pending)
            .OrderByDescending(k => k.SubmittedAt)
            .ToListAsync();
    }

    public async Task<List<KycRequest>> GetAllAsync()
    {
        return await _db.KycRequests
            .Include(k => k.User)
            .OrderByDescending(k => k.SubmittedAt)
            .ToListAsync();
    }
}
