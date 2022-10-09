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
        using MemoryStream ms = new();
        ms.Write(magic.FromAsciiString().SimpleDeEncryption(checksum));
        using MemoryStream ms2 = new();
        ms2.Write(SimpleDeEncryption(entryData, ms.ToArray()));
        ms2.Write(checksum);
        return ms2.ToArray().Base64Encode();
    }

    public static string Decrypto(this string inputString, string magic)
    {
        var entryData = inputString.Base64Decode();
        using MemoryStream ms = new();
        ms.Write(magic.FromAsciiString().SimpleDeEncryption(entryData.TakeLast(16).ToArray()));
        using MemoryStream ms2 = new();
        ms2.Write(entryData, 0, entryData.Length - 16);
        return SimpleDeEncryption(ms2.ToArray(), ms.ToArray()).ToUtf8String();
    }
}