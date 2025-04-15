namespace EcommerceNashApp.Core.Exeptions
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Body { get; set; } = default!;

        public ApiResponse(int code, string message, T body = default!)
        {
            Code = code;
            Message = message;
            Body = body;
        }

        public ApiResponse() { }
    }
}
