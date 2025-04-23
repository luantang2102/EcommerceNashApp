using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Core.Settings;
using EcommerceNashApp.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;


namespace EcommerceNashApp.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinaryConfig> config)
        {
            var account = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult> AddImageAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "ecommerce-nash-app-images"
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "fileName", file.FileName },
                    { "error", uploadResult.Error }
                };
                throw new AppException(ErrorCode.CLOUDINARY_UPLOAD_FAILED, attributes);
            }

            return uploadResult;
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);

            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Error != null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "publicId", publicId },
                    { "error", result.Error }
                };
                throw new AppException(ErrorCode.CLOUDINARY_DELETE_FAILED, attributes);
            }

            return result;
        }
    }
}
