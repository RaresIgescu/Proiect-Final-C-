using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public OrdersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = db.Orders.Where(o => o.UserId == userId).ToList();
            ViewBag.Orders = orders;
            return View();
        }
        public IActionResult Show(int id)
        {
            var order = db.Orders
                          .Include(o => o.ArticleOrders)
                          .ThenInclude(ao => ao.Article)
                          .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Comanda nu a fost găsită.");
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var order = db.Orders.Include("ArticleOrders")
                                 .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "Comanda nu a fost găsită.";
                return RedirectToAction("Index");
            }

            db.ArticleOrders.RemoveRange(order.ArticleOrders);

            db.Orders.Remove(order);

            db.SaveChanges();

            TempData["Success"] = "Comanda a fost ștearsă cu succes.";
            return RedirectToAction("Index");
        }

    }
}
