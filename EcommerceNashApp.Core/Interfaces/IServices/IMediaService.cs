using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IMediaService
    {
        Task<ImageUploadResult> AddImageAsync(IFormFile file);
        Task<DeletionResult> DeleteImageAsync(string publicId);
    }
}
