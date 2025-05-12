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
        private readonly EncryptionService _encryptionService;

        public QuizFileService() {
            var key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            var iv = Encoding.UTF8.GetBytes("1234567890123456");
            _encryptionService = new EncryptionService(key, iv);
        }

        public void SaveQuiz(QuizModel quiz, string filePath) {
            var json = JsonSerializer.Serialize(quiz);
            var encryptedData = _encryptionService.Encrypt(json);
            File.WriteAllBytes(filePath, encryptedData);
        }

        public QuizModel LoadQuiz(string filePath) {
            var encryptedData = File.ReadAllBytes(filePath);
            var decryptedJson = _encryptionService.Decrypt(encryptedData);
            return JsonSerializer.Deserialize<QuizModel>(decryptedJson);
        }
    }
}
