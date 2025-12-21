namespace NArk.Abstractions.Contracts;

public interface IContractStorage
{
    event EventHandler? ContractsChanged;
    Task<IReadOnlySet<ArkContractEntity>> LoadAllContracts(string walletIdentifier);
    Task<IReadOnlySet<ArkContractEntity>> LoadActiveContracts(IReadOnlyCollection<string> walletIdentifier);
    Task<ArkContractEntity?> LoadContractByScript(string script);
    Task SaveContract(string walletIdentifier, ArkContractEntity walletEntity);
}