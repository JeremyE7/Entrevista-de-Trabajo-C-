using Microsoft.AspNetCore.Identity;

namespace TodoApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
