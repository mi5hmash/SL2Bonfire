using System.Security.Cryptography;

namespace BonfireCore.Models.BND4;

public class Bnd4Entry
{
    /// <summary>
    /// Length of MD5 Checksum Signature.
    /// </summary>
    private const int S = 16;

    /// <summary>
    /// Length of AES IV.
    /// </summary>
    private const int K = 16;

    /// <summary>
    /// Array with the footer paddings.
    /// </summary>
    private static readonly int[] PaddingArray = { 0, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
    
    /// <summary>
    /// EntryHeader of this BND4 entry.
    /// </summary>
    public Bnd4EntryHeader EntryHeader;

    /// <summary>
    /// EntryName of this BND4 entry.
    /// </summary>
    public string EntryName { get; set; }

    /// <summary>
    /// Raw EntryData stored in the entry.
    /// </summary>
    public byte[] EntryData { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Bnd4Entry(string entryName, byte[] entryData)
    {
        EntryName = entryName;
        EntryData = entryData;
        EntryHeader = new Bnd4EntryHeader();
    }

    /// <summary>
    /// Decrypt the <see cref="EntryData"/>, assuming 128bit AES-CBC for DS2 and DS3 saves.
    /// </summary>
    /// <param name="encryptionKey"></param>
    public void DecryptEntryData(byte[] encryptionKey)
    {
        // create decryptor
        using var aes = AesDs128Cbc();
        // set keys
        aes.Key = encryptionKey;
        Array.Copy(EntryData, 0, aes.IV, 0, K);
        // decrypt method
        Decryptor(aes);
    }

    /// <summary>
    /// Encrypt the <see cref="EntryData"/>, assuming 128bit AES-CBC for DS2 and DS3 saves.
    /// </summary>
    /// <param name="encryptionKey"></param>
    public void EncryptEntryData(byte[] encryptionKey)
    {
        // create encryptor
        using var aes = AesDs128Cbc();
        // set keys
        aes.Key = encryptionKey;
        aes.GenerateIV();
        // decrypt method
        Encryptor(aes);
    }

    /// <summary>
    /// Returns an instance of Advanced Encryption Standard used in decryption and encryption of DS2 and DS3 save files.  
    /// </summary>
    /// <returns></returns>
    private static Aes AesDs128Cbc()
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.BlockSize = 128;
        aes.Padding = PaddingMode.None;
        return aes;
    }

    /// <summary>
    /// Common decryption method.
    /// </summary>
    /// <param name="aes"></param>
    private void Decryptor(SymmetricAlgorithm aes)
    {
        // create streams and decrypt
        using MemoryStream ms = new(EntryData);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using MemoryStream ms2 = new();
        cs.CopyTo(ms2);
        // finalize
        var dataLength = new byte[4];
        var data = ms2.ToArray();
        Array.Copy(data, K, dataLength, 0, dataLength.Length);
        var prefix = K + dataLength.Length;
        var postfix = data.Length - prefix - BitConverter.ToInt32(dataLength, 0);
        EntryData = data.Skip(prefix).SkipLast(postfix).ToArray();
    }

    /// <summary>
    /// Common encryption method.
    /// </summary>
    /// <param name="aes"></param>
    private void Encryptor(SymmetricAlgorithm aes)
    {
        // create streams and encrypt
        using MemoryStream ms = new();
        var dataLength = BitConverter.GetBytes((uint)EntryData.Length);
        ms.Write(dataLength);
        ms.Write(EntryData);
        CustomPkcs7Padding(ms);
        ms.Position = 0;
        using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Read);
        using MemoryStream ms2 = new();
        ms2.Write(aes.IV);
        cs.CopyTo(ms2);
        // finalize
        EntryData = ms2.ToArray();
    }

    /// <summary>
    /// Adds custom padding made of 0xN bytes of N length at the end of <paramref name="ms"/> stream.
    /// <b>Example: <paramref name="ms"/> length is 12 bytes, which means that 4 bytes is missing to 16 bytes. Function will add 0x0404_0404 at the end of <paramref name="ms"/></b>
    /// </summary>
    /// <param name="ms"></param>
    private static void CustomPkcs7Padding(Stream ms)
    {
        var len = (uint)PaddingArray[(uint)ms.Length % 16];
        var bytes = new byte[len];
        for (var i = 0; i < len; i++)
        {
            bytes[i] = (byte)len;
        }
        ms.Write(bytes);
    }

    /// <summary>
    /// Sign EntrySize with a calculated MD5 Checksum Signature and add footer of an EntryFooterLength size.
    /// </summary>
    public void SignEntryData()
    {
        using var md5 = MD5.Create();
        var checksum = md5.ComputeHash(EntryData);
        using MemoryStream ms = new();
        ms.Write(checksum);
        ms.Write(EntryData);
        EntryHeader.EntrySize = (uint)ms.Length;
        EntryHeader.EntryFooterLength = (uint)PaddingArray[EntryHeader.EntrySize % 16];
        var buff = new byte[EntryHeader.EntrySize + EntryHeader.EntryFooterLength];
        ms.Position = 0;
        var readLength = ms.Read(buff);
        EntryData = buff;
    }

    /// <summary>
    /// Remove MD5 Checksum Signature and the footer from the <see cref="EntryData"/>.
    /// </summary>
    public void UnsignEntryData()
    {
        using MemoryStream ms = new();
        ms.Write(EntryData, S, (int)(EntryHeader.EntrySize - S));
        EntryHeader.EntrySize = (uint)ms.Length;
        EntryHeader.EntryFooterLength = 0;
        EntryData = ms.ToArray();
    }
}