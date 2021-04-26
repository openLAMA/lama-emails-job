#region Copyright
// openLAMA is an open source platform which has been developed by the
// Swiss Kanton Basel Landschaft, with the goal of automating and managing
// large scale Covid testing programs or any other pandemic/viral infections.

// Copyright(C) 2021 Kanton Basel Landschaft, Switzerland
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// See LICENSE.md in the project root for license information.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see https://www.gnu.org/licenses/.
#endregion

using System;
using System.IO;
using System.Security.Cryptography;
using Elyon.Fastly.EmailJob.Domain.Services;

namespace Elyon.Fastly.EmailJob.PostgresRepositories.Helpers
{
    public class AESCryptography : IAESCryptography
    {
        private readonly string _encryptionKey;
        private readonly string _encryptionIV;

        public AESCryptography(string encryptionKey, string encryptionIV)
        {
            _encryptionKey = encryptionKey;
            _encryptionIV = encryptionIV;
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return null;
            }

            if (string.IsNullOrEmpty(_encryptionKey))
            {
                throw new ArgumentNullException(paramName: _encryptionKey);
            }

            if (string.IsNullOrEmpty(_encryptionIV))
            {
                throw new ArgumentNullException(paramName: _encryptionIV);
            }

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.BlockSize = 128;
                aesAlg.KeySize = 256;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Key = Convert.FromBase64String(_encryptionKey);
                aesAlg.IV = Convert.FromBase64String(_encryptionIV);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                encrypted = msEncrypt.ToArray();
            }

            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return null;
            }

            var bytes = Convert.FromBase64String(cipherText);

            if (string.IsNullOrEmpty(_encryptionKey))
            {
                throw new ArgumentNullException(paramName: _encryptionKey);
            }

            if (string.IsNullOrEmpty(_encryptionIV))
            {
                throw new ArgumentNullException(paramName: _encryptionIV);
            }

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.BlockSize = 128;
                aesAlg.KeySize = 256;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Key = Convert.FromBase64String(_encryptionKey);
                aesAlg.IV = Convert.FromBase64String(_encryptionIV);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msDecrypt = new MemoryStream(bytes);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    plaintext = srDecrypt.ReadToEnd();
                }
            }

            return plaintext;
        }

        byte[] IAESCryptography.Decrypt(byte[] cipherBytes)
        {
            if (string.IsNullOrEmpty(_encryptionKey))
            {
                throw new ArgumentNullException(paramName: _encryptionKey);
            }

            if (string.IsNullOrEmpty(_encryptionIV))
            {
                throw new ArgumentNullException(paramName: _encryptionIV);
            }

            byte[] plainBytes = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.BlockSize = 128;
                aesAlg.KeySize = 256;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Key = Convert.FromBase64String(_encryptionKey);
                aesAlg.IV = Convert.FromBase64String(_encryptionIV);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msDecrypt = new MemoryStream(cipherBytes);
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    csDecrypt.Read(cipherBytes, 0, cipherBytes.Length);
                }

                plainBytes = msDecrypt.ToArray();
            }

            return plainBytes;
        }

        byte[] IAESCryptography.Encrypt(byte[] plainBytes)
        {
            if (string.IsNullOrEmpty(_encryptionKey))
            {
                throw new ArgumentNullException(paramName: _encryptionKey);
            }

            if (string.IsNullOrEmpty(_encryptionIV))
            {
                throw new ArgumentNullException(paramName: _encryptionIV);
            }

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.BlockSize = 128;
                aesAlg.KeySize = 256;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Key = Convert.FromBase64String(_encryptionKey);
                aesAlg.IV = Convert.FromBase64String(_encryptionIV);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream msEncrypt = new MemoryStream();
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                }

                encrypted = msEncrypt.ToArray();
            }

            return encrypted;
        }
    }
}
