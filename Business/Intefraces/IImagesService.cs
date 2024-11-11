using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Business.Intefraces
{
    public interface IImagesService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file);
        Task<DeletionResult> DeleteImageAsync(string publicId);
    }
}
