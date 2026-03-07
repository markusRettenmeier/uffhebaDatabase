using System.Security.Cryptography;

namespace Sammlerplattform.Services
{
    public class ImageSigner
    {
        public static byte[] SignImage(string imagePath)
        {
            RSA privateKey = RSA.Create(2048);
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            return privateKey.SignData(imageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public static bool VerifySignature(string imagePath, byte[] signature, RSA publicKey)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            return publicKey.VerifyData(imageBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}