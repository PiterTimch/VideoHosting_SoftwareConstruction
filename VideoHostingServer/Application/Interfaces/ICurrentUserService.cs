namespace Application.Interfaces;

public interface ICurrentUserService
{
    long GetCurrentUserId();
    long? TryGetCurrentUserId();
    bool IsInRole(string role);
}
