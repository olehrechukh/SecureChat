using System.Threading.Tasks;
using EmailChecker.Shared;
using EmailChecker.Server.Repositories;
using EmailChecker.Server.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace EmailChecker.Server.GrpcServices
{
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

            var reply = new EmailValidateReply {Message = result ? "Valide" : "Invalide"};

            return Task.FromResult(reply);
        }
    }
}