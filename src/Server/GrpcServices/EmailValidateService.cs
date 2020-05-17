using System.Threading.Tasks;
using Chat.Server.Services;
using Chat.Shared;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Chat.Server.GrpcServices
{
    public class ValidationResult
    {
        public readonly bool IsSuccess;
        public readonly string ErrorMessage;

        private ValidationResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success => new ValidationResult(true, string.Empty);

        public static ValidationResult Error(string message) => new ValidationResult(false, message);
    }

    public class EmailValidateService : EmailValidator.EmailValidatorBase
    {
        private readonly ILogger<EmailValidateService> _logger;
        private readonly EmailCheckerService _service;

        public EmailValidateService(ILogger<EmailValidateService> logger, EmailCheckerService service)
        {
            _logger = logger;
            _service = service;
        }

        public override Task<EmailValidateReply> ValidateEmail(EmailValidateRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Input {request}", request);
            var result = _service.Validate(request.Email);

            var reply = new EmailValidateReply {ErrorMessage = result.ErrorMessage, IsSuccess = result.IsSuccess};

            return Task.FromResult(reply);
        }
    }
}