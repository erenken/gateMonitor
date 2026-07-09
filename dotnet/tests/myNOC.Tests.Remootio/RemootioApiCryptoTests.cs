using System.Security.Cryptography;
using System.Text.Json;

namespace myNOC.Tests.Remootio;

[TestClass]
public class RemootioApiCryptoTests
{
    // 64-char hex test keys (256-bit) — not real device keys
    private const string TestSecretKey = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string TestAuthKey   = "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210";

    private static string RandomSessionKey()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    [TestMethod]
    public void Encrypt_ThenDecrypt_WithSessionKey_RoundTrips()
    {
        string sessionKey = RandomSessionKey();
        const string payload = "{\"action\":{\"type\":\"QUERY\",\"id\":1}}";

        string? frame = RemootioApiCrypto.Encrypt(payload, TestAuthKey, sessionKey);

        Assert.IsNotNull(frame);

        using var doc = JsonDocument.Parse(frame!);
        var root = doc.RootElement;
        var dataEl = root.GetProperty("data");
        string rawDataJson = dataEl.GetRawText();
        string iv = dataEl.GetProperty("iv").GetString()!;
        string encPayload = dataEl.GetProperty("payload").GetString()!;
        string mac = root.GetProperty("mac").GetString()!;

        string? decrypted = RemootioApiCrypto.Decrypt(
            rawDataJson, iv, encPayload, mac,
            TestSecretKey, TestAuthKey, sessionKey);

        Assert.AreEqual(payload, decrypted);
    }

    [TestMethod]
    public void Encrypt_ProducesValidEncryptedFrameStructure()
    {
        string sessionKey = RandomSessionKey();

        string? frame = RemootioApiCrypto.Encrypt("{\"action\":{\"type\":\"OPEN\",\"id\":2}}", TestAuthKey, sessionKey);

        Assert.IsNotNull(frame);
        using var doc = JsonDocument.Parse(frame!);
        var root = doc.RootElement;

        Assert.AreEqual("ENCRYPTED", root.GetProperty("type").GetString());
        Assert.IsTrue(root.GetProperty("data").TryGetProperty("iv", out _), "Missing iv");
        Assert.IsTrue(root.GetProperty("data").TryGetProperty("payload", out _), "Missing payload");
        Assert.IsTrue(root.TryGetProperty("mac", out _), "Missing mac");
    }

    [TestMethod]
    public void Decrypt_WithTamperedMac_ReturnsNull()
    {
        string sessionKey = RandomSessionKey();
        const string payload = "{\"action\":{\"type\":\"QUERY\",\"id\":1}}";

        string? frame = RemootioApiCrypto.Encrypt(payload, TestAuthKey, sessionKey);
        Assert.IsNotNull(frame);

        using var doc = JsonDocument.Parse(frame!);
        var root = doc.RootElement;
        var dataEl = root.GetProperty("data");
        string rawDataJson = dataEl.GetRawText();
        string iv = dataEl.GetProperty("iv").GetString()!;
        string encPayload = dataEl.GetProperty("payload").GetString()!;

        string? result = RemootioApiCrypto.Decrypt(
            rawDataJson, iv, encPayload,
            mac: "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            TestSecretKey, TestAuthKey, sessionKey);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Decrypt_WithInvalidBase64_ReturnsNull()
    {
        string? result = RemootioApiCrypto.Decrypt(
            rawDataJson: "{\"iv\":\"not-base64!\",\"payload\":\"not-base64!\"}",
            iv: "not-base64!",
            payload: "not-base64!",
            mac: "not-base64!",
            apiSecretKey: TestSecretKey,
            apiAuthKey: TestAuthKey,
            apiSessionKey: null);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Encrypt_DifferentCallsProduceDifferentIVs()
    {
        string sessionKey = RandomSessionKey();
        const string payload = "{\"action\":{\"type\":\"CLOSE\",\"id\":3}}";

        string? frame1 = RemootioApiCrypto.Encrypt(payload, TestAuthKey, sessionKey);
        string? frame2 = RemootioApiCrypto.Encrypt(payload, TestAuthKey, sessionKey);

        Assert.IsNotNull(frame1);
        Assert.IsNotNull(frame2);

        using var doc1 = JsonDocument.Parse(frame1!);
        using var doc2 = JsonDocument.Parse(frame2!);

        string iv1 = doc1.RootElement.GetProperty("data").GetProperty("iv").GetString()!;
        string iv2 = doc2.RootElement.GetProperty("data").GetProperty("iv").GetString()!;

        // Random IVs should differ (astronomically unlikely to collide)
        Assert.AreNotEqual(iv1, iv2);
    }
}
