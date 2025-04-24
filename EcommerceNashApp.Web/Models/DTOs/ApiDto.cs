namespace EcommerceNashApp.Web.Models.DTOs
{
    public class ApiDto<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Body { get; set; } = default!;
    }
}
