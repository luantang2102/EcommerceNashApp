using EcommerceNashApp.Infrastructure.Exceptions;

namespace EcommerceNashApp.Core.Exeptions
{
    public class ValidationException : AppException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base(ErrorCode.VALIDATION_ERROR)
        {
            Errors = errors;
        }

        public override Dictionary<string, object> GetAttributes()
        {
            return new Dictionary<string, object> { { "errors", Errors } };
        }
    }
}