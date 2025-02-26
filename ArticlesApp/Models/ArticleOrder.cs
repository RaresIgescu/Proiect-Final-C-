using Microsoft.EntityFrameworkCore;
using ArticlesApp.Validations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticlesApp.Models
{
    public class ArticleOrder
    {
        [Key]
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int OrderId { get; set; }
        public Article? Article { get; set; }
        public Order? Order { get; set; }
        public DateTime Date { get; set; }
        public string Quantity { get; set; }
    }
}
