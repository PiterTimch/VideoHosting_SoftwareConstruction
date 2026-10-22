using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class VideoAccessTokenService(IConfiguration configuration) : IVideoAccessTokenService
{
    private string SecretKey => configuration["Jwt:Key"]
        ?? "SecretKeyForVideoHostingProject2026LabWorks!!";

    public string GenerateToken(long videoId, TimeSpan expiresIn)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(expiresIn).ToUnixTimeSeconds();
        var payload = $"{videoId}:{expiresAt}";
        var signature = ComputeHmac(payload);
        var raw = $"{payload}:{signature}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public bool ValidateToken(string token, long videoId)
    {
        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = raw.Split(':');
            if (parts.Length != 3) return false;

            if (!long.TryParse(parts[0], out var tokenVideoId)) return false;
            if (!long.TryParse(parts[1], out var expiresAt)) return false;

            if (tokenVideoId != videoId) return false;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresAt) return false;

            var payload = $"{parts[0]}:{parts[1]}";
            var expectedSig = ComputeHmac(payload);
            return string.Equals(parts[2], expectedSig, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private string ComputeHmac(string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hash);
    }
}
