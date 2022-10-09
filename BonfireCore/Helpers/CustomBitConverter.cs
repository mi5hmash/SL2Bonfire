using System.Runtime.InteropServices;
using static System.Globalization.NumberStyles;

namespace BonfireCore.Helpers;

/// Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/GenBitConverter.cs
public static class CustomBitConverter
{
    /// <summary>
    /// Convert structure to bytes.
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static byte[] ToBytes<T>(T obj) where T : class
    {
        var size = Marshal.SizeOf(obj);
        var arr = new byte[size];

        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(obj!, ptr, false);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    /// <summary>
    /// Convert bytes to structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static T? ToStruct<T>(byte[] buffer) where T : class
    {
        var size = Marshal.SizeOf<T>();
        var ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(buffer, 0, ptr, size);
        var obj = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return obj;
    }

    /// <summary>
    /// Read a structure from a BinaryReader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bin"></param>
    /// <returns></returns>
    public static T? ReadStruct<T>(this BinaryReader bin) where T : class
    {
        var size = Marshal.SizeOf<T>();

        var buff = new byte[size];
        bin.Read(buff, 0, size);

        return ToStruct<T>(buff);
    }

    /// <summary>
    /// Write a structure to a BinaryWriter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bin"></param>
    /// <param name="obj"></param>
    public static void WriteStruct<T>(this BinaryWriter bin, T obj) where T : class
    {
        var buff = ToBytes(obj);
        bin.Write(buff, 0, buff.Length);
    }

    /// <summary>
    /// Turns a hex string into a byte array.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static byte[] HexStringToBytes(this string hex)
    {
        var data = new byte[hex.Length / 2];
        for (var i = 0; i < hex.Length; i += 2)
        {
            data[i / 2] = byte.Parse(hex.Substring(i, 2), HexNumber);
        }
        return data;
    }

    /// <summary>
    /// Turns a byte array into a hex string.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string BytesToHexString(this byte[] bytes) 
        => BitConverter.ToString(bytes).Replace("-", string.Empty);
}