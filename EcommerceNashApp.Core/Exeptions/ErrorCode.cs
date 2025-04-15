using Microsoft.AspNetCore.Http;

namespace EcommerceNashApp.Core.Exeptions
{
    public class ErrorCode
    {
        // User related errors (100-199)
        public static readonly ErrorCode USER_NOT_FOUND = new(100, "User not found", StatusCodes.Status404NotFound);

        // Token related errors (300-399)

        // Access related errors (400-499)
        public static readonly ErrorCode ACCESS_DENIED = new(403, "Access denied to view or modify this resource", StatusCodes.Status403Forbidden);

        // Product related errors (500-599)
        public static readonly ErrorCode PRODUCT_NOT_FOUND = new(500, "Product not found", StatusCodes.Status404NotFound);
        public static readonly ErrorCode DUPLICATE_PRODUCT = new(501, "Product already existed", StatusCodes.Status404NotFound);

        // Cloudinary related errors (600-699)
        public static readonly ErrorCode CLOUDINARY_UPLOAD_FAILED = new(600, "Cloudinary upload failed", StatusCodes.Status500InternalServerError);
        public static readonly ErrorCode CLOUDINARY_DELETE_FAILED = new(601, "Cloudinary delete failed", StatusCodes.Status500InternalServerError);

        private readonly int _code;
        private readonly string _message;
        private readonly int _status;

        public ErrorCode(int code, string message, int status)
        {
            _code = code;
            _message = message;
            _status = status;
        }

        public int GetCode() => _code;
        public string GetMessage() => _message;
        public int GetStatus() => _status;
    }
}
