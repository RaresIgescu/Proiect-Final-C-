using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ArticlesApp.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ArticlesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Main()
        {
            return View();
        }

            [AllowAnonymous]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var articles = db.Articles.Include("Category")
                                      .Include("User")
                                      .OrderByDescending(a => a.Date);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); 

                List<int> articleIds = db.Articles.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();

                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => (int)c.ArticleId).ToList();

                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();


                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                                      .Include("Category")
                                      .Include("User")
                                      .OrderByDescending(a => a.Date);

            }

            ViewBag.SearchString = search;

            var sortOrder = HttpContext.Request.Query["sortOrder"].ToString();

            if (!string.IsNullOrEmpty(sortOrder))
            {
                if (sortOrder == "asc")
                {
                    articles = articles.OrderBy(a => a.Pret);
                }
                else if (sortOrder == "desc")
                {
                    articles = articles.OrderByDescending(a => a.Pret);
                }
            }

            ViewBag.CurrentSortOrder = sortOrder;

            int _perPage = 12;

            int totalItems = articles.Count();

            int currentPage = 1; 

            if (!string.IsNullOrEmpty(HttpContext.Request.Query["page"]))
            {
                int.TryParse(HttpContext.Request.Query["page"], out currentPage);
            }

            if (currentPage < 1)
            {
                currentPage = 1;
            }

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedArticles = articles.Skip(offset).Take(_perPage);


            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Articles = paginatedArticles;


            if (search != "" && sortOrder != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&sortOrder=" + sortOrder + "&page";
            }
            else if(search != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&page";
            }
            else if(sortOrder != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?sortOrder=" + sortOrder + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";
            }

            return View();
        }

        [AllowAnonymous]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Article article = db.Articles.Include("Category")
                                         .Include("Comments")
                                         .Include("User")
                                         .Include("Comments.User")
                              .Where(art => art.Id == id)
                              .First();

            SetAccessRights();

            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            comment.UserId = _userManager.GetUserId(User);

            if (comment.Rating < 1 || comment.Rating > 5)
            {
                TempData["Error"] = "Ratingul trebuie să fie între 1 și 5.";
                Article art = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                               .Where(art => art.Id == comment.ArticleId)
                               .First();

                SetAccessRights();
                return View(art);
            }

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();

                var article = db.Articles.FirstOrDefault(a => a.Id == comment.ArticleId);
                if (article != null)
                {
                    var averageRating = db.Comments
                        .Where(c => c.ArticleId == comment.ArticleId)
                        .Average(c => c.Rating);

                    article.Rating = averageRating;
                    db.Articles.Update(article);
                    db.SaveChanges();
                }

                TempData["Message"] = "Comentariul a fost adăugat cu succes!";
                return Redirect("/Articles/Show/" + comment.ArticleId);
            }
            else
            {
                Article art = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                               .Where(art => art.Id == comment.ArticleId)
                               .First();

                SetAccessRights();
                return View(art);
            }
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {
            Article article = new Article();

            article.Categ = GetAllCategories();

            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> New(Article article, IFormFile ImageFile)
        {
            article.Date = DateTime.Now;

            article.UserId = _userManager.GetUserId(User);

            article.ImagePath = " ";

            ModelState.Remove("ImagePath");

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                article.ImagePath = Path.Combine("images", uniqueFileName).Replace("\\", "/");
            }

            if (ModelState.IsValid)
            {
                db.Articles.Add(article);
                db.SaveChanges();

                TempData["message"] = "Articolul a fost adăugat cu succes.";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Index");
            }
            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Article article = db.Articles.Include("Category")
                                         .Where(art => art.Id == id)
                                         .First();

            article.Categ = GetAllCategories();

            if ((article.UserId == _userManager.GetUserId(User)) || 
                User.IsInRole("Admin"))
            {
                return View(article);
            }
            else
            {    
                
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }  
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int id, Article requestArticle)
        {
            Article article = db.Articles.Find(id);

            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                if((article.UserId == _userManager.GetUserId(User)) 
                    || User.IsInRole("Admin"))
                {
                    article.Title = requestArticle.Title;
                    article.Content = requestArticle.Content;
                    article.Date = DateTime.Now;
                    article.CategoryId = requestArticle.CategoryId;
                    article.Pret = requestArticle.Pret;
                    article.Stoc = requestArticle.Stoc;
                    TempData["message"] = "Articolul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {                    
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestArticle.Categ = GetAllCategories();
                return View(requestArticle);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            Article article = db.Articles.Include("Comments")
                                         .Where(art => art.Id == id)
                                         .FirstOrDefault();

            if (article == null)
            {
                TempData["message"] = "Articolul nu există.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if ((article.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                var cartItems = db.Carts.Where(c => c.ArticleId == id).ToList();
                db.Carts.RemoveRange(cartItems);

                db.Articles.Remove(article);

                db.SaveChanges();

                TempData["message"] = "Articolul și înregistrările asociate au fost șterse.";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți un articol care nu vă aparține.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index");
        }


        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            
            return selectList;
        }
        public IActionResult IndexNou()
        {
            return View();
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult AddToCart(int id, int quantity)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Nu ești autentificat!";
                return RedirectToAction("Index", "Home");
            }

            var cartItem = db.Carts.FirstOrDefault(c => c.UserId == userId && c.ArticleId == id);

            if (cartItem == null)
            {
                cartItem = new Cart
                {
                    UserId = userId,
                    ArticleId = id,
                    Quantity = quantity 
                };
                db.Carts.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity; 
            }

            db.SaveChanges();

            TempData["Message"] = "Articolul a fost adăugat în coș!";
            return RedirectToAction("Cart");
        }



        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Cart()
        {
            var userId = _userManager.GetUserId(User);

            var articles = db.Carts.Where(c => c.UserId == userId)
                           .Join(db.Articles,
                                 cart => cart.ArticleId,
                                 article => article.Id,
                                 (cart, article) => article) 
                           .ToList();

            ViewBag.Articles = articles.ToList();

            return View();
        }
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult RemoveFromCart(int Id)
        {
            var userId = _userManager.GetUserId(User);

            Cart cartItem = db.Carts.FirstOrDefault(c => c.UserId == userId && c.ArticleId == Id);

            if (cartItem != null)
            {
                db.Carts.Remove(cartItem); 
                db.SaveChanges();

                TempData["Message"] = "Articolul a fost eliminat din coș."; 
            }
            return RedirectToAction("Cart");
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult PlaceOrder()
        {
            var userId = _userManager.GetUserId(User);

            var cartItems = db.Carts.Where(c => c.UserId == userId).ToList();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Coșul este gol. Nu poți plasa o comandă fără articole.";
                return RedirectToAction("Cart");
            }

            foreach (var cartItem in cartItems)
            {
                var article = db.Articles.FirstOrDefault(a => a.Id == cartItem.ArticleId);
                if (article == null)
                {
                    TempData["Error"] = "Articolul nu mai există.";
                    return RedirectToAction("Cart");
                }

                if (Int32.Parse(article.Stoc) < cartItem.Quantity)
                {
                    TempData["Error"] = $"Nu există suficient stoc pentru articolul: {article.Title}. Disponibile: {article.Stoc}";
                    return RedirectToAction("Cart");
                }
            }

            var newOrder = new Order
            {
                UserId = userId,
            };
            db.Orders.Add(newOrder);
            db.SaveChanges();

            foreach (var cartItem in cartItems)
            {
                var orderArticle = new ArticleOrder
                {
                    OrderId = newOrder.Id, 
                    ArticleId = cartItem.ArticleId,
                    Quantity = cartItem.Quantity.ToString(),
                };
                db.ArticleOrders.Add(orderArticle);

                var article = db.Articles.FirstOrDefault(a => a.Id == cartItem.ArticleId);
                if (article != null)
                {
                    article.Stoc = (int.Parse(article.Stoc) - cartItem.Quantity).ToString();
                    db.Articles.Update(article);
                }
            }
            db.SaveChanges();

            db.Carts.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["Message"] = "Comanda a fost plasată cu succes!";
            return RedirectToAction("Cart");
        }
    }
}
