using Microsoft.AspNetCore.Identity;


namespace ArticlesApp.Models
{
    public class ApplicationUser: IdentityUser
    {
        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Article>? Articles { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
