public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file);
    // string GetImageUrl(string filename);
    // string SaveImageAsync2(IFormFile file);
}