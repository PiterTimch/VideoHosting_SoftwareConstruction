using Application.Constants;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.Services;

public class CookieAuthService(IHttpContextAccessor httpContextAccessor) : ICookieAuthService
{
    public void SetAuthCookie(string token)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/"
        };

        httpContext.Response.Cookies.Append(AuthConstants.AuthCookieName, token, cookieOptions);
    }

    public void ClearAuthCookie()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Path = "/"
        };

        httpContext.Response.Cookies.Delete(AuthConstants.AuthCookieName, cookieOptions);
    }
}
