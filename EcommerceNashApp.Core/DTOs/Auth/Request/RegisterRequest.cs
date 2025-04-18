﻿namespace EcommerceNashApp.Core.DTOs.Auth.Request
{
    public class RegisterRequest
    {
        public string? UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public string? ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; } = string.Empty;

    }
}
