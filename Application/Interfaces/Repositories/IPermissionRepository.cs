using Application.Interfaces.Commons;
using Domain.Entities.Identity;

namespace Application.Interfaces.Repositories
{
    public interface IPermissionRepository : IGenericRepository<Permission, int>
    {
        List<string> GetUserPermissions(string userId);
    }
}
