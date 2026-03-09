using Application.DTOs.Identity;
using Application.Interfaces.Commons;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Identity;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Shared.Constants;
using Shared.Results;

namespace Application.Services.Identity
{
    public class PermissionService : IPermissionService
    {
        private readonly IMemoryCache _cache;
        private readonly IPermissionRepository _permissionRepository;
        public PermissionService(IMemoryCache cache, 
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService,
            IPermissionRepository permissionRepository)
        {
            _cache = cache;
            _permissionRepository = permissionRepository;
        }
        public ServiceResult<List<string>> GetUserPermissionsAsync(string userId)
        {
            try
            {
                var cacheKey = $"permissions_{userId}";

                if (_cache.TryGetValue(cacheKey, out List<string> cachedPermissions))
                {
                    return ServiceResult<List<string>>.Success(cachedPermissions);
                }
                var permissions = _permissionRepository.GetUserPermissions(userId);
                _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(5));
                return ServiceResult<List<string>>.Success(permissions);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving permissions for user {UserId}", userId);
                return ServiceResult<List<string>>.InternalServerError($"{ErrorMessages.ErrorRetrivingPermissions}: {ex.Message}");
            }
        }

        public ServiceResult<bool> HasPermissionAsync(string userId, string permission)
        {
            try
            {
                var permissions = GetUserPermissionsAsync(userId);
                if (!permissions.IsSuccess)
                    return ServiceResult<bool>.InternalServerError(permissions.Message);
                return ServiceResult<bool>.Success(permissions.Data.Contains(permission));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
                return ServiceResult<bool>.InternalServerError($"{ErrorMessages.ErrorRetrivingPermissions}: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<PermissionDto>> GetList()
        {
            try
            {
                var result = _permissionRepository.GetAll().Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Module = p.Module,
                    Description = p.Description,
                });
                if (result == null) return ServiceResult<IEnumerable<PermissionDto>>.NotFound(ErrorMessages.PermissionNotFound);
                return ServiceResult<IEnumerable<PermissionDto>>.Success(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving permissions list");
                return ServiceResult<IEnumerable<PermissionDto>>.InternalServerError($"{ErrorMessages.ErrorRetrivingPermissions}: {ex.Message}");
            }
        }
    }
}
