namespace Application.Interfaces;

public interface IVideoAccessTokenService
{
    string GenerateToken(long videoId, TimeSpan expiresIn);
    bool ValidateToken(string token, long videoId);
}
