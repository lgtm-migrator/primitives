﻿using System.Diagnostics.CodeAnalysis;
using Dodo.Primitives.Internal;

namespace Dodo.Primitives
{
    /// <summary>
    ///     Utility methods to work with hexadecimal strings.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public static unsafe class Hex
    {
        private const ushort MaximalChar = InternalHexTables.MaximalChar;
        private static readonly uint* TableToHex;
        private static readonly byte* TableFromHexToBytes;

        static Hex()
        {
            TableToHex = InternalHexTables.TableToHex;
            TableFromHexToBytes = InternalHexTables.TableFromHexToBytes;
        }

        /// <summary>
        ///     Checks that provided string is hexadecimal.
        /// </summary>
        /// <param name="possibleHexString">String to check.</param>
        /// <returns></returns>
        public static bool IsHexString(string? possibleHexString)
        {
            if (string.IsNullOrWhiteSpace(possibleHexString))
            {
                return false;
            }

            if (possibleHexString!.Length % 2 != 0)
            {
                return false;
            }

            int length = possibleHexString.Length;

#if NETCOREAPP3_1 || NET5_0 || NET6_0
            fixed (char* stringPtr = &possibleHexString.GetPinnableReference())
#endif
#if NETSTANDARD2_0
            fixed (char* stringPtr = possibleHexString)
#endif
            {
                for (var i = 0; i < length;)
                {
                    if (stringPtr[i] < MaximalChar
                        && TableFromHexToBytes[stringPtr[i]] != 0xFF
                        && stringPtr[i + 1] < MaximalChar
                        && TableFromHexToBytes[stringPtr[i + 1]] != 0xFF)
                    {
                        i += 2;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns bytes of hexadecimal string.
        /// </summary>
        /// <param name="hexString">Hexadecimal string</param>
        /// <returns></returns>
        public static byte[]? GetBytes(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
            {
                return null;
            }

            if (hexString.Length % 2 != 0)
            {
                return null;
            }

            int length = hexString.Length;
            var result = new byte[length / 2];
#if NETCOREAPP3_1 || NET5_0 || NET6_0
            fixed (char* stringPtr = &hexString.GetPinnableReference())
#endif
#if NETSTANDARD2_0
            fixed (char* stringPtr = hexString)
#endif
            fixed (byte* resultPtr = &result[0])
            {
                var resultIndex = 0;
                for (var i = 0; i < length;)
                {
                    byte hexByteHi;
                    byte hexByteLow;
                    if (stringPtr[i] < MaximalChar
                        && (hexByteHi = TableFromHexToBytes[stringPtr[i]]) != 0xFF
                        && stringPtr[i + 1] < MaximalChar
                        && (hexByteLow = TableFromHexToBytes[stringPtr[i + 1]]) != 0xFF)
                    {
                        var resultByte = (byte) ((byte) (hexByteHi << 4) | hexByteLow);
                        resultPtr[resultIndex] = resultByte;
                        i += 2;
                        resultIndex += 1;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Return hexadecimal string representation of provided bytes.
        /// </summary>
        /// <param name="bytes">Bytes that need to be presented as hexadecimal string.</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static string? GetString(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            var resultString = new string('\0', bytes.Length * 2);
#if NETCOREAPP3_1 || NET5_0 || NET6_0
            fixed (char* stringPtr = &resultString.GetPinnableReference())
#endif
#if NETSTANDARD2_0
            fixed (char* stringPtr = resultString)
#endif
            {
                var destUints = (uint*) stringPtr;
                for (var i = 0; i < bytes.Length; i++)
                {
                    destUints[i] = TableToHex[bytes[i]];
                }
            }

            return resultString;
        }
    }
}
