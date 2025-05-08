using EcommerceNashApp.Shared.DTOs.Auth.Request;
using System.ComponentModel.DataAnnotations;

namespace EcommerceNashApp.Web.Models.OverrideValidation
{
    public class RegisterRequestCli : RegisterRequest
    {
        [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Tên người dùng không được chứa khoảng trắng.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[\W]).+$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ cái in hoa, một chữ cái thường, một số và một ký tự đặc biệt.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp với mật khẩu.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; } = string.Empty;
    }
}
