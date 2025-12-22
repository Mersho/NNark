using NArk.Abstractions.Contracts;
using NArk.Abstractions.Wallets;
using NArk.Contracts;
using NArk.Transport;

namespace NArk.Services;

public class ContractService(
    IWallet wallet,
    IContractStorage contractStorage,
    IClientTransport transport
): IContractService
{
    public async Task<ArkContract> DerivePaymentContract(string walletId)
    {
        var info = await transport.GetServerInfoAsync();
        var signingEntity = await wallet.GetNewSigningEntity(walletId);
        var contract = new ArkPaymentContract(
            info.SignerKey,
            info.UnilateralExit,
            await signingEntity.GetOutputDescriptor()
        );
        await contractStorage.SaveContract(walletId, contract.ToEntity(walletId));
        return contract;
    }

    public async Task ImportContract(string walletId, ArkContract contract)
    {
        var info = await transport.GetServerInfoAsync();
        if (contract.Server is not null && !contract.Server.Equals(info.SignerKey))
            throw new InvalidOperationException("Cannot import contract with different server key");
        await contractStorage.SaveContract(walletId, contract.ToEntity(walletId));
    }
}