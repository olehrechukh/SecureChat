using System;
using System.Threading.Tasks;
using Chat.Shared;
using FluentAssertions;
using Xunit;

namespace EmailChecker.Shared.Tests
{
    public class SecurityServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("string")]
        [InlineData("Hello how do you do?")]
        public void EncryptThanDecrypt_ShouldHaveInitialValue(string message)
        {
            var (publicKey, privateKey) = SecurityService.GenerateKeys();

            var encrypted = SecurityService.EncryptText(publicKey, message);
            var decrypted = SecurityService.DecryptData(privateKey, encrypted);

            message.Should().Be(decrypted);
        }
    }
}