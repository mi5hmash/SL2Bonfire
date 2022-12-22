using System.Security.Cryptography;

namespace BonfireCore.Helpers;

public static class SimpleDeencryptor
{

    private static byte[] SimpleDeEncryption(this byte[] bytesToDeEncrypt, byte[] magicBytes)
    {
        var spellLength = magicBytes.Length;
        for (var i = 0; i < bytesToDeEncrypt.Length; i++)
        {
            bytesToDeEncrypt[i] ^= magicBytes[i % spellLength];
        }
        return bytesToDeEncrypt;
    }

    public static string Encrypto(this string inputString, string magic)
    {
        var entryData = inputString.FromUtf8String();
        using var md5 = MD5.Create();
        var checksum = md5.ComputeHash(entryData);
        using MemoryStream msi = new();
        msi.Write(magic.FromAsciiString().SimpleDeEncryption(checksum));
        using MemoryStream mso = new();
        mso.Write(SimpleDeEncryption(entryData, msi.ToArray()));
        mso.Write(checksum);
        return mso.ToArray().Base64Encode();
    }

    public static string Decrypto(this string inputString, string magic)
    {
        var entryData = inputString.Base64Decode();
        using MemoryStream msi = new();
        msi.Write(magic.FromAsciiString().SimpleDeEncryption(entryData.TakeLast(16).ToArray()));
        using MemoryStream mso = new();
        mso.Write(entryData, 0, entryData.Length - 16);
        return SimpleDeEncryption(mso.ToArray(), msi.ToArray()).ToUtf8String();
    }
}