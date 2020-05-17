using System;
using System.Net;
using System.Text.RegularExpressions;
using ActiveUp.Net.Mail;
using Chat.Server.GrpcServices;
using Microsoft.Extensions.Logging;

namespace Chat.Server.Services
{
    public class EmailCheckerService
    {
        private readonly ILogger<EmailCheckerService> _logger;

        public EmailCheckerService(ILogger<EmailCheckerService> logger)
        {
            _logger = logger;
        }

        public ValidationResult Validate(string address)
        {
            if (!ValidateSyntax(address))
            {
                return ValidationResult.Error("Incorrect email syntax");
            }

            try
            {
                var address1 = address.Split('@')[1];
                using var smtpClient = new SmtpClient {SendTimeout = 0, ReceiveTimeout = 0};
                var recordCollection = new MxRecordCollection();
                try
                {
                    recordCollection = Validator.GetMxRecords(address1);
                }
                catch
                {
                    _logger.LogInformation("Can't connect to DNS server.");
                }

                if (recordCollection.Count <= 0)
                {
                    return ValidationResult.Error("Incorrect dns");
                }

                smtpClient.Connect(recordCollection.GetPrefered().Exchange);
                try
                {
                    smtpClient.Ehlo(Dns.GetHostName());
                }
                catch
                {
                    smtpClient.Helo(Dns.GetHostName());
                }

                ValidationResult flag;
                if (smtpClient.Verify(address))
                {
                    flag = ValidationResult.Success;
                }
                else
                {
                    try
                    {
                        smtpClient.MailFrom("postmaster@" + address1);
                        smtpClient.RcptTo(address);
                        flag = ValidationResult.Success;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(ex.ToString());
                        flag = ValidationResult.Error("Email doest not exist");
                    }
                }

                smtpClient.Disconnect();
                return flag;
            }
            catch
            {
                return ValidationResult.Error("Unknown error");
            }
        }

        private static bool ValidateSyntax(string address)
        {
            return Regex.IsMatch(address, "\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
        }
    }
}