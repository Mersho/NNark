using NBitcoin;

namespace NArk.Abstractions.VTXOs;

public interface IVtxoStorage
{
    Task SaveVtxo(ArkVtxo vtxo);
    Task<ArkVtxo?> GetVtxoByOutPoint(OutPoint outpoint);
    Task<IReadOnlyCollection<ArkVtxo>> GetVtxosByScripts(IReadOnlyCollection<string> scripts, bool allowSpent = false);
    Task<IReadOnlyCollection<ArkVtxo>> GetUnspentVtxos();
    Task<IReadOnlyCollection<ArkVtxo>> GetAllVtxos();
}