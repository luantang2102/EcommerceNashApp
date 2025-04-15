using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> AddImageAsync(IFormFile file);
        Task<DeletionResult> DeleteImageAsync(string publicId);
    }
}
