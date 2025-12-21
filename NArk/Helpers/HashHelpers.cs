using System.Text;

namespace NArk.Helpers;

internal static class HashHelpers
{
    internal static byte[] CreateTaggedMessageHash(string tag, string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        using var sha = new NBitcoin.Secp256k1.SHA256();
        sha.InitializeTagged(tag);
        sha.Write(bytes);
        return sha.GetHash();
    }
}