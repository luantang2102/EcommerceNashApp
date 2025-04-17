using EcommerceNashApp.Core.Exeptions;

namespace EcommerceNashApp.Infrastructure.Exceptions
{
    public class AppException : Exception
    {
        private readonly ErrorCode _errorCode;
        private readonly Dictionary<string, object> _attributes;

        public AppException(ErrorCode errorCode)
        {
            _errorCode = errorCode;
            _attributes = new Dictionary<string, object>();
        }
        public AppException(ErrorCode errorCode, Dictionary<string, object> attributes) : base(errorCode.GetMessage())
        {
            _errorCode = errorCode;
            _attributes = attributes ?? new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> GetAttributes()
        {
            return _attributes;
        }

        public ErrorCode GetErrorCode()
        {
            return _errorCode;
        }
    }
}
