namespace EcommerceNashApp.Api.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string message) : base(message) { }
    }
}
