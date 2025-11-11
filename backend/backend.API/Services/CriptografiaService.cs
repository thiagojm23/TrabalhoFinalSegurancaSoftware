using System.Security.Cryptography;
using System.Text;

namespace backend.API.Services
{
    public class CriptografiaService
    {
        private readonly byte[] _chave;
        private readonly byte[] _iv;

        public CriptografiaService(IConfiguration configuration)
        {
            // Usar a mesma chave JWT como base para gerar chave AES
            var chaveBase = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");

            // Gerar chave de 256 bits (32 bytes) para AES-256
            using var sha256 = SHA256.Create();
            _chave = sha256.ComputeHash(Encoding.UTF8.GetBytes(chaveBase));

            // Gerar IV fixo (16 bytes) - em produção, considere usar IV único por arquivo
            _iv = sha256.ComputeHash(Encoding.UTF8.GetBytes(chaveBase + "IV")).Take(16).ToArray();
        }

        public string CriptografarNomeArquivo(string nomeArquivo)
        {
            using var aes = Aes.Create();
            aes.Key = _chave;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var nomeBytes = Encoding.UTF8.GetBytes(nomeArquivo);
            var criptografado = encryptor.TransformFinalBlock(nomeBytes, 0, nomeBytes.Length);

            // Converter para Base64 URL-safe (substituir caracteres problemáticos)
            return Convert.ToBase64String(criptografado)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public string DescriptografarNomeArquivo(string nomeCriptografado)
        {
            try
            {
                // Reverter Base64 URL-safe
                var base64 = nomeCriptografado
                    .Replace("-", "+")
                    .Replace("_", "/");

                // Adicionar padding se necessário
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                using var aes = Aes.Create();
                aes.Key = _chave;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var criptografadoBytes = Convert.FromBase64String(base64);
                var descriptografado = decryptor.TransformFinalBlock(criptografadoBytes, 0, criptografadoBytes.Length);

                return Encoding.UTF8.GetString(descriptografado);
            }
            catch
            {
                throw new InvalidOperationException("Não foi possível descriptografar o nome do arquivo");
            }
        }
    }
}
