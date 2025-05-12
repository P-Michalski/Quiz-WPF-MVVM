using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Quiz.Services {
    public class EncryptionService {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(byte[] key, byte[] iv) {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _iv = iv ?? throw new ArgumentNullException(nameof(iv));
        }

        public byte[] Encrypt(string plainText) {
            using (var aes = Aes.Create()) {
                aes.Key = _key;
                aes.IV = _iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                    using (var ms = new MemoryStream()) {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                            using (var sw = new StreamWriter(cs, Encoding.UTF8)) {
                                sw.Write(plainText);
                            }
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        public string Decrypt(byte[] cipherText) {
            using (var aes = Aes.Create()) {
                aes.Key = _key;
                aes.IV = _iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
                    using (var ms = new MemoryStream(cipherText)) {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                            using (var sr = new StreamReader(cs, Encoding.UTF8)) {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
