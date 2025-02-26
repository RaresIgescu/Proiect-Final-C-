using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<ArticleOrder>? ArticleOrders { get; set; }
    }
}

