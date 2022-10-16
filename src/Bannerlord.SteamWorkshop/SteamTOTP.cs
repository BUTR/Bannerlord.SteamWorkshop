using System;
using System.Security.Cryptography;

namespace Bannerlord.SteamWorkshop
{
    public static class SteamTOTP
    {
        private static unsafe void WriteUInt32BE(this Span<byte> buffer, uint value, int start)
        {
            fixed (byte* ptr = &buffer[start])
            {
                unchecked
                {
                    ptr[0] = (byte)(value >> 24);
                    ptr[1] = (byte)(value >> 16);
                    ptr[2] = (byte)(value >> 8);
                    ptr[3] = (byte)value;
                }
            }
        }

        public static string GenerateAuthCode(string secret, long? time)
        {
            const string availableChars = "23456789BCDFGHJKMNPQRTVWXY";

            var secretBuffer = Convert.FromBase64String(secret);

            time ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Span<byte> buffer = stackalloc byte[8];
            // The first 4 bytes are the high 4 bytes of a 64-bit integer. To make things easier on ourselves, let's just pretend
            // that it's a 32-bit int and write 0 for the high bytes. Since we're dividing by 30, this won't cause a problem
            // until the year 6053.
            buffer.WriteUInt32BE(0, 0);
            buffer.WriteUInt32BE((uint) Math.Floor(time.Value / 30m), 4);

            var hmac = new HMACSHA1(secretBuffer);
            Span<byte> hmacDigest = stackalloc byte[20];
            hmac.TryComputeHash(buffer, hmacDigest, out _);

            var start = hmacDigest[19] & 0x0F;
            hmacDigest = hmacDigest.Slice(start, 4);

            hmacDigest.Reverse();
            var fullcode = BitConverter.ToUInt32(hmacDigest) & 0x7FFFFFFF;

            return string.Create(5, fullcode, static (chars, fullcode_) =>
            {
                for (var i = 0; i < 5; i++)
                {
                    chars[i] = availableChars[(int) (fullcode_ % availableChars.Length)];
                    fullcode_ /= (uint) availableChars.Length;
                }
            });
        }
    }
}