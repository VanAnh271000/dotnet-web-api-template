namespace Domain.Entities.Identity
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty; 
        public string Module { get; set; }
        public string Action { get; set; } = string.Empty;  
        public string? Description { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
