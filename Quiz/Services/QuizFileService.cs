using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Quiz.Models;
using System.Security.Cryptography;

namespace Quiz.Services {
    public class QuizFileService {
        private readonly byte[] _key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // Klucz AES (16 bajtów)
        private readonly byte[] _iv = Encoding.UTF8.GetBytes("1234567890123456");   // Wektor inicjalizacyjny (16 bajtów)

        public void SaveQuiz(QuizModel quiz, string filePath) {
            var json = JsonSerializer.Serialize(quiz);
            var encryptedData = Encrypt(json);
            File.WriteAllBytes(filePath, encryptedData);
        }

        public QuizModel LoadQuiz(string filePath) {
            var encryptedData = File.ReadAllBytes(filePath);
            var decryptedJson = Decrypt(encryptedData);
            return JsonSerializer.Deserialize<QuizModel>(decryptedJson);
        }

        private byte[] Encrypt(string plainText) {
            using (var aes = Aes.Create()) {
                aes.Key = _key;
                aes.IV = _iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                    using (var ms = new MemoryStream()) {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                            using (var sw = new StreamWriter(cs)) {
                                sw.Write(plainText);
                            }
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        private string Decrypt(byte[] cipherText) {
            using (var aes = Aes.Create()) {
                aes.Key = _key;
                aes.IV = _iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
                    using (var ms = new MemoryStream(cipherText)) {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                            using (var sr = new StreamReader(cs)) {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
