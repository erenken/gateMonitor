using System.Security.Cryptography;
using System.Text;

namespace myNOC.Remootio;

/// <summary>
/// Ports the crypto logic from remootioApiCrypto.ts.
/// Remootio uses AES-CBC encryption with PKCS7 padding and HMAC-SHA256 message authentication.
/// Keys are 64-char hex strings (256-bit). The session key is base64-encoded.
/// Decrypted payloads are Latin1-encoded JSON strings (matching crypto-js enc.Latin1).
/// </summary>
internal static class RemootioApiCrypto
{
    /// <summary>
    /// Decrypts an incoming ENCRYPTED frame from the Remootio device.
    /// </summary>
    /// <param name="rawDataJson">The raw JSON string of frame.data as received (used verbatim for MAC verification).</param>
    /// <param name="iv">Base64-encoded IV from frame.data.iv.</param>
    /// <param name="payload">Base64-encoded ciphertext from frame.data.payload.</param>
    /// <param name="mac">Base64-encoded expected MAC from the frame.</param>
    /// <param name="apiSecretKey">64-char hex API secret key.</param>
    /// <param name="apiAuthKey">64-char hex API auth key.</param>
    /// <param name="apiSessionKey">Base64 session key (null if not yet authenticated).</param>
    /// <returns>The decrypted JSON string, or null if MAC fails or decryption errors.</returns>
    public static string? Decrypt(
        string rawDataJson,
        string iv,
        string payload,
        string mac,
        string apiSecretKey,
        string apiAuthKey,
        string? apiSessionKey)
    {
        byte[] authKey = Convert.FromHexString(apiAuthKey);

        // Verify HMAC-SHA256 over the exact raw data JSON received from the device
        using var hmac = new HMACSHA256(authKey);
        byte[] computedMacBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawDataJson));
        string computedMac = Convert.ToBase64String(computedMacBytes);

        if (computedMac != mac)
        {
            Console.WriteLine($"[RemootioApiCrypto] MAC mismatch. Computed: {computedMac}, Received: {mac}");
            return null;
        }

        byte[] keyBytes = apiSessionKey is null
            ? Convert.FromHexString(apiSecretKey)
            : Convert.FromBase64String(apiSessionKey);
        byte[] ivBytes = Convert.FromBase64String(iv);
        byte[] cipherBytes = Convert.FromBase64String(payload);

        try
        {
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.Latin1.GetString(decrypted);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RemootioApiCrypto] Decryption failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Constructs an outgoing ENCRYPTED frame JSON string to send to the Remootio device.
    /// Only valid in an authenticated session (apiSessionKey must be set).
    /// </summary>
    /// <param name="plainTextJson">Action JSON to encrypt (e.g. {"action":{"type":"QUERY","id":1}}).</param>
    /// <param name="apiAuthKey">64-char hex API auth key.</param>
    /// <param name="apiSessionKey">Base64 session key received during authentication.</param>
    /// <returns>The full ENCRYPTED frame as a JSON string, or null on error.</returns>
    public static string? Encrypt(string plainTextJson, string apiAuthKey, string apiSessionKey)
    {
        byte[] sessionKey = Convert.FromBase64String(apiSessionKey);
        byte[] authKey = Convert.FromHexString(apiAuthKey);

        byte[] ivBytes = new byte[16];
        RandomNumberGenerator.Fill(ivBytes);

        byte[] payloadBytes = Encoding.Latin1.GetBytes(plainTextJson);

        try
        {
            using var aes = Aes.Create();
            aes.Key = sessionKey;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] encrypted = encryptor.TransformFinalBlock(payloadBytes, 0, payloadBytes.Length);

            string ivBase64 = Convert.ToBase64String(ivBytes);
            string payloadBase64 = Convert.ToBase64String(encrypted);

            // Key order {"iv":...,"payload":...} must match what the device expects for MAC verification
            string dataJson = $"{{\"iv\":\"{ivBase64}\",\"payload\":\"{payloadBase64}\"}}";

            using var hmac = new HMACSHA256(authKey);
            byte[] macBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataJson));
            string macBase64 = Convert.ToBase64String(macBytes);

            return $"{{\"type\":\"ENCRYPTED\",\"data\":{{\"iv\":\"{ivBase64}\",\"payload\":\"{payloadBase64}\"}},\"mac\":\"{macBase64}\"}}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RemootioApiCrypto] Encryption failed: {ex.Message}");
            return null;
        }
    }
}
