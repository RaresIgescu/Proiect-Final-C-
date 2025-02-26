namespace ArticlesApp.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ArticleId { get; set; }
        public int Quantity { get; set; }

    }
}
