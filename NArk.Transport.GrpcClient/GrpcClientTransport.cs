using Ark.V1;
using Grpc.Net.Client;
using NArk.Abstractions;
using NArk.Scripts;
using NArk.Transport.GrpcClient.Extensions;
using NBitcoin;
namespace NArk.Transport.GrpcClient;

public partial class GrpcClientTransport: IClientTransport
{
    private readonly ArkService.ArkServiceClient _client;

    public GrpcClientTransport(string uri)
    {
        var channel = GrpcChannel.ForAddress(uri);
        _client = new ArkService.ArkServiceClient(channel);
    }
}