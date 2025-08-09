namespace HR.MAUI.Shared.Services
{
    public interface IAuthService
    {
        Task<bool> IsAuthenticatedAsync();
        Task<bool> IsInRoleAsync(string role);
        Task<string?> GetUserNameAsync();
        Task<UserInfo?> GetUserAsync();
    }

    public class UserInfo
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}
