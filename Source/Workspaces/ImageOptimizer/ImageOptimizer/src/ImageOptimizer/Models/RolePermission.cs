using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ImageOptimizer.Models
{
    public class RolePermission : RolePermissionBase
    {
        public int Id { get; set; }

        public string RoleId { get; set; }
        public virtual IdentityRole Role { get; set; }
    }
}
