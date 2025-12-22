using NArk.Contracts;

namespace NArk.Services;

public interface IContractService
{
    Task<ArkContract> DerivePaymentContract(string walletId);
    Task ImportContract(string walletId, ArkContract contract);
}