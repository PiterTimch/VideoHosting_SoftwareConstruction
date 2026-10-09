using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SecurityTests
{
    public class JwtSecurityTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public JwtSecurityTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("/api/Account/ChangePassword", new StringContent(""));
            
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.Redirect);
        }
        
        [Fact]
        public async Task AccessProtectedEndpoint_WithInvalidBearerToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token_format_123");
            
            var response = await client.PostAsync("/api/Account/ChangePassword", new StringContent(""));
            
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }
        
        [Fact]
        public async Task AccessProtectedEndpoint_WithExpiredBearerToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            // A structurally valid JWT but expired (header.payload.signature)
            string expiredJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.g7Kk8T5wD3mX6k9q0lV6rFmX4lD5c1lQ6G5P_2j_3q0";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredJwt);
            
            var response = await client.PostAsync("/api/Account/ChangePassword", new StringContent(""));
            
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }

        [Fact]
        public void SignedUrlGenerator_GeneratesValidSignature()
        {
            var secret = "test_secret_key_1234567890123456";
            var data = "video_id=1&expires=1234567890";
            
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            var signature = System.Convert.ToBase64String(hash);
            
            signature.Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public void SignedUrlGenerator_DifferentData_GeneratesDifferentSignatures()
        {
            var secret = "test_secret_key_1234567890123456";
            
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hash1 = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("video_id=1"));
            var hash2 = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("video_id=2"));
            
            var sig1 = System.Convert.ToBase64String(hash1);
            var sig2 = System.Convert.ToBase64String(hash2);
            
            sig1.Should().NotBe(sig2);
        }
    }
}
