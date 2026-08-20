using System.Diagnostics;
using LoginForm.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoginForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CodeFirstDbContext context;

        public HomeController(ILogger<HomeController> logger, CodeFirstDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("userSession") != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User user)
        {
            var Myuser = context.Users.Where(u => u.Email == user.Email && u.Password == user.Password).FirstOrDefault();
            if (Myuser != null)
            {
                HttpContext.Session.SetString("userSession", Myuser.Email);
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.Message = "Invalid Email or Password";
                return View();
            }

        }
        public IActionResult Dashboard()
        {
            var email = HttpContext.Session.GetString("userSession");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }


            // User information

            ViewBag.mySession = user.Name;

            ViewBag.AccountNumber = user.AccountNumber;

            ViewBag.Balance = user.Balance;


            // Recent transactions

            var transactions = context.Transactions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.TransactionDate)
                .Take(5)
                .ToList();

            ViewBag.Transactions = transactions;


            return View();
        }
        public IActionResult Logout()
        {
            if (HttpContext.Session.GetString("userSession") != null)
            {
                HttpContext.Session.Remove("userSession");
                return RedirectToAction("Login");
            }
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.Balance = 0;

                    await context.Users.AddAsync(user);
                    await context.SaveChangesAsync();

                    user.AccountNumber = "100000" + user.Id;

                    await context.SaveChangesAsync();

                    TempData["Success"] = "Registered Successfully";

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    return Content(ex.ToString());
                }
            }

            return View(user);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: Deposit
        public IActionResult Deposit()
        {
            var email = HttpContext.Session.GetString("userSession");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Balance = user.Balance;

            return View();
        }


        // POST: Deposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deposit(decimal amount)
        {
            var email = HttpContext.Session.GetString("userSession");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (amount <= 0)
            {
                ViewBag.Message = "Please enter a valid amount.";
                return View();
            }

            // Add money
            user.Balance += amount;

            // Create transaction
            var transaction = new Transaction
            {
                UserId = user.Id,
                Type = "Deposit",
                Amount = amount,
                Description = "Money deposited",
                TransactionDate = DateTime.Now
            };

            context.Transactions.Add(transaction);

            context.SaveChanges();

            TempData["Success"] =
                $"{amount:N2} deposited successfully.";

            return RedirectToAction("Dashboard");
        }



        // GET: Withdraw
        public IActionResult Withdraw()
        {
            var email = HttpContext.Session.GetString("userSession");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Balance = user.Balance;

            return View();
        }


        // POST: Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Withdraw(decimal amount)
        {
            var email = HttpContext.Session.GetString("userSession");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (amount <= 0)
            {
                ViewBag.Message = "Please enter a valid amount.";
                return View();
            }

            if (amount > user.Balance)
            {
                ViewBag.Message = "Insufficient balance.";
                return View();
            }

            // Remove money
            user.Balance -= amount;

            // Create transaction
            var transaction = new Transaction
            {
                UserId = user.Id,
                Type = "Withdrawal",
                Amount = amount,
                Description = "Money withdrawn",
                TransactionDate = DateTime.Now
            };

            context.Transactions.Add(transaction);

            context.SaveChanges();

            TempData["WithdrawSuccess"] =
                $"{amount:N2} withdrawn successfully.";

            return RedirectToAction("Dashboard");
        }


        // GET: Transfer
        public IActionResult Transfer()
        {
            var email = HttpContext.Session.GetString("userSession");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Balance = user.Balance;
            ViewBag.AccountNumber = user.AccountNumber;

            return View();
        }

        // POST: Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Transfer(string accountNumber, decimal amount)
        {
            var email = HttpContext.Session.GetString("userSession");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            // Sender
            var sender = context.Users
                .FirstOrDefault(x => x.Email == email);

            if (sender == null)
            {
                return RedirectToAction("Login");
            }

            // Validate amount
            if (amount <= 0)
            {
                ViewBag.Message = "Please enter a valid amount.";
                ViewBag.Balance = sender.Balance;
                ViewBag.AccountNumber = sender.AccountNumber;

                return View();
            }

            // Check balance
            if (amount > sender.Balance)
            {
                ViewBag.Message = "Insufficient balance.";
                ViewBag.Balance = sender.Balance;
                ViewBag.AccountNumber = sender.AccountNumber;

                return View();
            }

            // Find receiver
            var receiver = context.Users
                .FirstOrDefault(x => x.AccountNumber == accountNumber);

            if (receiver == null)
            {
                ViewBag.Message = "Recipient account not found.";
                ViewBag.Balance = sender.Balance;
                ViewBag.AccountNumber = sender.AccountNumber;

                return View();
            }

            // Prevent self transfer
            if (sender.Id == receiver.Id)
            {
                ViewBag.Message =
                    "You cannot transfer money to your own account.";

                ViewBag.Balance = sender.Balance;
                ViewBag.AccountNumber = sender.AccountNumber;

                return View();
            }



            // Update balances
            sender.Balance -= amount;

            receiver.Balance += amount;


            // Sender transaction
            var senderTransaction = new Transaction
            {
                UserId = sender.Id,
                Type = "Transfer",
                Amount = amount,
                Description = $"Transfer to {receiver.AccountNumber}",
                TransactionDate = DateTime.Now
            };



            // Receiver transaction
            var receiverTransaction = new Transaction
            {
                UserId = receiver.Id,
                Type = "Transfer",
                Amount = amount,
                Description = $"Transfer from {sender.AccountNumber}",
                TransactionDate = DateTime.Now
            };


            context.Transactions.Add(senderTransaction);
            context.Transactions.Add(receiverTransaction);


            context.SaveChanges();


            TempData["TransferSuccess"] =
                $"{amount:N2} transferred successfully.";

            return RedirectToAction("Dashboard");
        }
    }
}
