namespace Application.Interfaces;

public interface ICookieAuthService
{
    void SetAuthCookie(string token);
    void ClearAuthCookie();
}
