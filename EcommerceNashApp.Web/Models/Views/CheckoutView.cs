using System.ComponentModel.DataAnnotations;

namespace EcommerceNashApp.Web.Models.Views
{
    public class CheckoutView
    {
        public int CurrentStep { get; set; } = 1;
        public ShippingAddressView ShippingAddress { get; set; } = new();
        public CartPageView Cart { get; set; } = new();
        public PaymentView? Payment { get; set; }
        public string? ClientSecret { get; set; } = string.Empty;
        public string? PaymentMethodId { get; set; } = string.Empty;
        public int? OrderNumber { get; set; } = null;
        public bool PaymentSucceeded { get; set; } = false;
        public string? SessionId { get; set; } = string.Empty;
        public string? PaymentMessage { get; set; } = string.Empty;
    }

    public class ShippingAddressView
    {
        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Địa chỉ dòng 1 là bắt buộc")]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [Required(ErrorMessage = "Thành phố là bắt buộc")]
        public string City { get; set; }

        public string State { get; set; }

        [Required(ErrorMessage = "Mã bưu điện là bắt buộc")]
        public string Zip { get; set; }

        [Required(ErrorMessage = "Quốc gia là bắt buộc")]
        public string Country { get; set; }

        public bool SaveAddress { get; set; }
    }

    public class PaymentView
    {
        public string NameOnCard { get; set; } = String.Empty;
    }
}