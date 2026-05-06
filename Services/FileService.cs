namespace ServicesApp.Services;

public class FileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string?> SaveFileAsync(IFormFile? file, string folder)
    {
        if (file == null || file.Length == 0) return null;

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    public bool DeleteFile(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        var path = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path)) { File.Delete(path); return true; }
        return false;
    }

    public bool IsValidImage(IFormFile? file)
    {
        if (file == null) return false;
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowed.Contains(ext) && file.Length <= 5 * 1024 * 1024; // 5MB
    }

    public bool IsValidFile(IFormFile? file)
    {
        if (file == null) return false;
        return file.Length <= 50 * 1024 * 1024; // 50MB
    }
}
