using System;
using System.Net;
using System.Text.RegularExpressions;
using ActiveUp.Net.Mail;
using Microsoft.Extensions.Logging;

namespace EmailChecker.Server.Services
{
    public class EmailCheckerService
    {
        private readonly ILogger<EmailCheckerService> _logger;

        public EmailCheckerService(ILogger<EmailCheckerService> logger)
        {
            _logger = logger;
        }

        public bool Validate(string address)
        {
            if (!ValidateSyntax(address))
            {
                return false;
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
                    return false;
                smtpClient.Connect(recordCollection.GetPrefered().Exchange);
                try
                {
                    smtpClient.Ehlo(Dns.GetHostName());
                }
                catch
                {
                    smtpClient.Helo(Dns.GetHostName());
                }

                bool flag;
                if (smtpClient.Verify(address))
                {
                    flag = true;
                }
                else
                {
                    try
                    {
                        smtpClient.MailFrom("postmaster@" + address1);
                        smtpClient.RcptTo(address);
                        flag = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(ex.ToString());
                        flag = false;
                    }
                }

                smtpClient.Disconnect();
                return flag;
            }
            catch
            {
                return false;
            }
        }

        private static bool ValidateSyntax(string address)
        {
            return Regex.IsMatch(address, "\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
        }
    }
}