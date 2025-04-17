using Microsoft.AspNetCore.Http;

namespace EcommerceNashApp.Core.Exeptions
{
    public class ErrorCode
    {
        /// <summary>
        /// Custom error code, message, and status.
        /// </summary>
        // User related errors (100-199)
        public static readonly ErrorCode USER_NOT_FOUND = new(100, "User not found", StatusCodes.Status404NotFound);
        public static readonly ErrorCode INVALID_CREDENTIALS = new(101, "Invalid credentials", StatusCodes.Status401Unauthorized);
        public static readonly ErrorCode DUPLICATE_EMAIL = new(101, "Email already exists", StatusCodes.Status409Conflict);
        public static readonly ErrorCode PASSWORDS_DO_NOT_MATCH = new(102, "Passwords do not match", StatusCodes.Status400BadRequest);
        public static readonly ErrorCode IDENTITY_CREATION_FAILED = new(103, "Identity creation failed", StatusCodes.Status500InternalServerError);


        // Access related errors (400-499)
        public static readonly ErrorCode ACCESS_DENIED = new(403, "Access denied to view or modify this resource", StatusCodes.Status403Forbidden);

        // Product related errors (600-699)
        public static readonly ErrorCode PRODUCT_NOT_FOUND = new(600, "Product not found", StatusCodes.Status404NotFound);
        public static readonly ErrorCode DUPLICATE_PRODUCT = new(601, "Product already existed", StatusCodes.Status409Conflict);

        // Cloudinary related errors (700-799)
        public static readonly ErrorCode CLOUDINARY_UPLOAD_FAILED = new(700, "Cloudinary upload failed", StatusCodes.Status500InternalServerError);
        public static readonly ErrorCode CLOUDINARY_DELETE_FAILED = new(701, "Cloudinary delete failed", StatusCodes.Status500InternalServerError);

        // Category related errors (800-899)
        public static readonly ErrorCode CATEGORY_NOT_FOUND = new(800, "Category not found", StatusCodes.Status404NotFound);
        public static readonly ErrorCode DUPLICATE_CATEGORY = new(801, "Category already existed", StatusCodes.Status409Conflict);
        public static readonly ErrorCode PARENT_CATEGORY_NOT_FOUND = new(802, "Parent category not found", StatusCodes.Status404NotFound);
        public static readonly ErrorCode CATEGORY_CIRCULAR_REFERENCE = new(803, "Category circular reference", StatusCodes.Status400BadRequest);
        public static readonly ErrorCode CATEGORY_HAS_SUBCATEGORIES = new(804, "Category has subcategories, cannot delete", StatusCodes.Status400BadRequest);

        // Rating related errors (900-999)
        public static readonly ErrorCode RATING_NOT_FOUND = new(900, "Rating not found", StatusCodes.Status404NotFound);
        public static readonly ErrorCode RATING_ALREADY_EXISTS = new(901, "Rating already exists", StatusCodes.Status409Conflict);

        // Token related errors (1000-1099)
        public static readonly ErrorCode INVALID_OR_EXPIRED_REFRESH_TOKEN = new(1000, "Invalid or expired refresh token", StatusCodes.Status401Unauthorized);

        // Validation related errors (1100-1199)
        public static readonly ErrorCode VALIDATION_ERROR = new(1100, "Validation error", StatusCodes.Status400BadRequest);

        /// <summary>
        /// Atributes for error code, message, and status. 
        /// </summary>
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
