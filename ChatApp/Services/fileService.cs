using ChatApp.DTOs;
// using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

public class FileService : IFileService
{
    private readonly string _basePath;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] PdfExtensions = { ".pdf" };
    private static readonly string[] WordExtensions = { ".doc", ".docx" };
    private static readonly string[] VideoExtensions = { ".mp4", ".mov", ".avi", ".mkv" };

    public FileService(IConfiguration config)
    {
        _basePath = config["FileStorage:BasePath"]!;
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new BadHttpRequestException("File is empty.");

        var extension = Path.GetExtension(file.FileName).ToLower();

        // Validate extension + determine folder
        string folderName = GetFolderByExtension(extension);

        // Validate size
        if (file.Length > MaxFileSize)
            throw new BadHttpRequestException("File must be smaller than 10MB.");

        // Build full folder path
        var folderPath = Path.Combine(_basePath, folderName);

        // Ensure folder exists
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Generate unique file name
        var fileName = $"{Guid.NewGuid()}{extension}";

        var fullPath = Path.Combine(folderPath, fileName);

        // Save file
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative URL (used by frontend)
        return $"/{folderName}/{fileName}";
    }

    private string GetFolderByExtension(string extension)
    {
        if (ImageExtensions.Contains(extension))
            return "images";

        if (PdfExtensions.Contains(extension))
            return "pdfs";

        if (WordExtensions.Contains(extension))
            return "word";

        if (VideoExtensions.Contains(extension))
            return "videos";

        var allAllowed = ImageExtensions
            .Concat(PdfExtensions)
            .Concat(WordExtensions)
            .Concat(VideoExtensions);

        throw new BadHttpRequestException(
            $"File type {extension} not allowed. Allowed: {string.Join(", ", allAllowed)}");
    }

    /*private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "images/chat");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/images/chat/{fileName}";
    }


    private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    public async Task<string> SaveImageAsync2(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new BadHttpRequestException($"File type {ext} not allowed. Use {string.Join(", ", AllowedExtensions)}.");

        if (file.Length > 4 * 1024 * 1024)
            throw new BadHttpRequestException("Image must be smaller than 4MB.");

        var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "chat");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = Guid.NewGuid() + ext;
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream);

        return $"/images/chat/{fileName}";
    }*/
}