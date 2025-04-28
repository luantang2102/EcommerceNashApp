namespace EcommerceNashApp.Web.Models.Views
{
    public class AccountPageView
    {
        public string FormType { get; set; } = "login";
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool RememberMe { get; set; }
        public string? ImageUrl { get; set; }
    }
}
