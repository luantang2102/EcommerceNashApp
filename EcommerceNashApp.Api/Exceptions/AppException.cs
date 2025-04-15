using EcommerceNashApp.Core.Exeptions;

namespace EcommerceNashApp.Api.Exceptions
{
    public class AppException : Exception
    {
        private readonly ErrorCode _errorCode;
        private readonly Dictionary<string, object> _attributes;

        public AppException(ErrorCode errorCode) : base(errorCode.GetMessage())
        {
            _errorCode = errorCode;
            _attributes = new Dictionary<string, object>();
        }

        public AppException(ErrorCode errorCode, Dictionary<string, object> attributes) : base(errorCode.GetMessage())
        {
            _errorCode = errorCode;
            _attributes = attributes ?? new Dictionary<string, object>();
        }

        public ErrorCode GetErrorCode() => _errorCode;
        public Dictionary<string, object> GetAttributes() => _attributes;
    }
}
