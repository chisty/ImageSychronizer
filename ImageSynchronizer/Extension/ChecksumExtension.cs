using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ImageSynchronizer.Extension
{
    public static class ChecksumExtension
    {
        public static string Checksum(this string source)
        {
            return Checksum(source.GetBytes());
        }

        public static string Checksum(this byte[] bytes)
        {
            int i;
            var md5 = MD5.Create();

            var hashValue = md5.ComputeHash(bytes);
            var sbuilder = new StringBuilder();
            for (i = 0; i < hashValue.Length; i++)
            {
                sbuilder.Append(hashValue[i].ToString("X2"));
            }
            md5.Clear();
            return sbuilder.ToString();
        }

        public static string Checksum(this FileInfo fileInfo)
        {
            var bytes = File.ReadAllBytes(fileInfo.FullName);
            return Checksum(bytes);
        }

        private static byte[] GetBytes(this string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
