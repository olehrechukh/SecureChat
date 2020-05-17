using System.Threading.Tasks;
using Chat.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Chat.Server.GrpcServices
{
    public class RsaGeneratorService : RsaGenerator.RsaGeneratorBase
    {
        public override Task<RsaKeysReply> GenerateKeys(Empty request, ServerCallContext context)
        {
            var (publicKey, privateKey) = SecurityService.GenerateKeys();
            return Task.FromResult(new RsaKeysReply()
            {
                PublicKey = publicKey,
                PrivateKey = privateKey
            });
        }
    }
}