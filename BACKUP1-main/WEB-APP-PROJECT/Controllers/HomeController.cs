
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WEB_APP_PROJECT.Data;
using WEB_APP_PROJECT.Models;

namespace WEB_APP_PROJECT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable <FoodCourt>? allFood = _db.FoodCourts;
            IEnumerable<UserState>? allNinja = _db.UserStates;
            string userName = User.Identity.GetUserName();
            var obj = _db.UserStates.FirstOrDefault(n => n.UserName == userName);
            if (obj == null)
            {
                var naruto = new UserState
                {
                    UserName = userName,
                    State = "Ninja!!!"
                };
                _db.UserStates.Add(naruto);
                _db.SaveChanges();
            }

            return View(allFood);
        }
        public IActionResult Ask(string FoodShopName)
        {
            IEnumerable<UserState>? allNinja = _db.UserStates;
            ViewData["FoodShopName"] = FoodShopName;

            return View(allNinja);
        }

        public IActionResult UpdateState(string userName, string newState, string FoodShopName)
        {
            var obj = _db.UserStates.FirstOrDefault(n => n.UserName == userName);
            if (obj == null)
            {
                return NotFound();
            }
            obj.State = newState;
            _db.SaveChanges();
            if(newState == "ผู้ค้า")
            {
                return RedirectToAction("ChoosePage", new { FoodShopName = FoodShopName });
            }
            return RedirectToAction("BuiltOrder", new {FoodShopName = FoodShopName});
        }

        public IActionResult BuiltOrder(string FoodShopName)
        {
            var obj = _db.FoodCourts.SingleOrDefault(f => f.FoodShopName == FoodShopName);
            if (obj != null)
            {
                ViewData["FoodShopName"] = FoodShopName;
                ViewData["ShopSrc"] = obj.FoodShopImg;
                return View();
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuiltOrder(Order obj)
        {
            obj.userName = User.Identity.Name;
            Console.WriteLine(obj);
            _db.Orders.Add(obj);
            _db.SaveChanges();
            Console.WriteLine("Order has been created!");
            return RedirectToAction("Index");
        }

        public IActionResult Order()
        {
            var orderList = _db.Orders.Where(c =>  c.riderName == @User.Identity.Name).ToList();
            return View(orderList);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult StartPage()
        {
            return View();
        }

        public IActionResult LoginPage()
        {
            return Redirect("~/Identity/Account/Login");
        }

        public IActionResult ChoosePage(String FoodShopName)
        {
            var orderList = _db.Orders.Where(c => c.FoodShopName== FoodShopName && c.status=="wait").ToList();
            ViewData["FoodShopName"] = FoodShopName;

            return View(orderList);
        }



        public IActionResult AcceptOrder(int Orderid)
        {
            var order = _db.Orders.Find(Orderid);
            if (order != null)
            {
                order.status = "in process";
                order.riderName = @User.Identity.Name;
                _db.Orders.Update(order);
                _db.SaveChanges();
                return RedirectToAction("Order");
            }
            else
            {
                return NotFound();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}