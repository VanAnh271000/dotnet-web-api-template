using Domain.Entities.Identity;
namespace Application.Interfaces.Services.Identity
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(ApplicationUser user);
        string GenerateRefreshToken();
        void RevokeExpireRefreshToken(string userId);
    }
}
