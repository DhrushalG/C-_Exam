using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Exam_attempt1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Exam_attempt1.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Users.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.SetString("UserEmail", newUser.Email);
                return RedirectToAction("dashboard");
            }
            else
            {
                return View("Index");
            }
        }
        [HttpGet("Logout")]
        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        [HttpPost("login")]
        public IActionResult Login(LogUser logUser)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(s => s.Email == logUser.LEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("LEmail", "Invalid login attempt");
                    return View("Index");
                }
                PasswordHasher<LogUser> Hasher = new PasswordHasher<LogUser>();
                PasswordVerificationResult result = Hasher.VerifyHashedPassword(logUser, userInDb.Password, logUser.LPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("LEmail", "Invalid login attempt");
                    return View("Index");
                }
                HttpContext.Session.SetString("UserEmail", userInDb.Email);
                return RedirectToAction("dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            User loggedIn = _context.Users
            .Include(s => s.isParticipating)
            .FirstOrDefault(d => d.Email == email);

            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return View("Index");
            }
            else
            {
                foreach (GroupActivity w in _context.GroupActivities)
                {
                    if (w.Date.Month == DateTime.Now.Month && w.Date.Day < DateTime.Now.Day)
                    {
                        _context.GroupActivities.Remove(w);
                    }
                    else if (DateTime.Now.Month > w.Date.Month)
                    {
                        _context.GroupActivities.Remove(w);
                    }
                    else if (DateTime.Now.Year > w.Date.Year)
                    {
                        _context.GroupActivities.Remove(w);
                    }
                    if(DateTime.Now.Hour <= w.Time.Hour)
                    {
                        if(DateTime.Now.Minute < w.Time.Minute)
                        {
                            _context.GroupActivities.Remove(w);
                        }
                    }
                }
                _context.SaveChanges();
                ViewBag.AllActivities = _context.GroupActivities
                .Include(s => s.inGroup)
                .OrderBy(o => o.Date >= DateTime.Today ? o.Date : DateTime.MinValue)
                // .ThenByDescending(x => x.Date >= DateTime.Today ? DateTime.MinValue : x.Date)
                .ToList();
                return View(loggedIn);
            }
        }

        [HttpGet("new")]
        public IActionResult NewGActivity()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            ViewBag.loggedIn = _context.Users
                .FirstOrDefault(d => d.Email == email);
            if (email == null)
            {
                return View("Index");
            }
            else
            {

                return View();
            }
        }
        [HttpPost("processGActivity")]
        public IActionResult ProcessGActivity(GroupActivity newGA)
        {
            string email = HttpContext.Session.GetString("UserEmail");
            ViewBag.loggedIn = _context.Users
            .FirstOrDefault(d => d.Email == email);
            if (ModelState.IsValid)
            {
                if (DateTime.Now.Year <= newGA.Date.Year)
                {
                    if (DateTime.Now.Month <= newGA.Date.Month)
                    {
                        if (newGA.Date.Month == DateTime.Now.Month && newGA.Date.Day > DateTime.Now.Day)
                        {
                            ModelState.AddModelError("Date", "Activity cannot be in the past");
                            return View("NewGActivity");
                        }
                        else
                        {
                            _context.GroupActivities.Add(newGA);
                            _context.SaveChanges();
                            return RedirectToAction("Dashboard");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Date", "Activity cannot be in the past");
                        return View("NewGActivity");
                    }
                }
                else
                {
                    ModelState.AddModelError("Date", "Activity cannot be in the past");
                    return View("NewGActivity");
                }
            }
            else
            {
                return View("NewGActivity");
            }

        }
        [HttpGet("deleteGActivity/{gaid}")]
        public IActionResult DeleteGA(int gaid)
        {
            GroupActivity DeleteOne = _context.GroupActivities.SingleOrDefault(s => s.GroupActivityId == gaid);
            _context.GroupActivities.Remove(DeleteOne);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpPost("processJoin")]
        public IActionResult ProcessJoin(Participant newJoin)
        {
            _context.Participants.Add(newJoin);
            _context.SaveChanges();
            string email = HttpContext.Session.GetString("UserEmail");
            User loggedIn = _context.Users.FirstOrDefault(d => d.Email == email);
            return RedirectToAction("Dashboard", loggedIn);
        }

        [HttpPost("processLeave")]
        public IActionResult ProcessLeave(Participant newLeave)
        {
            Participant DeleteOne = _context.Participants.SingleOrDefault(s => s.UserId == newLeave.UserId && s.GroupActivityId == newLeave.GroupActivityId);
            _context.Participants.Remove(DeleteOne);
            _context.SaveChanges();
            string email = HttpContext.Session.GetString("UserEmail");
            User loggedIn = _context.Users.FirstOrDefault(d => d.Email == email);
            return RedirectToAction("Dashboard", loggedIn);
        }

        [HttpGet("OneAct/{gaid}")]
        public IActionResult ViewOneGroupActivity(int gaid)
        {
            GroupActivity GA = _context.GroupActivities
            .Include(s => s.inGroup)
            .FirstOrDefault(d => d.GroupActivityId == gaid);
            // User loggedIn = _context.Users
            // .FirstOrDefault(s => s.Name == GA.Owner);
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return View("Index");
            }
            else
            {
                string email = HttpContext.Session.GetString("UserEmail");
                ViewBag.loggedIn = _context.Users
                .FirstOrDefault(d => d.Email == email);

                ViewBag.InGroup = _context.Users
                .Include(a => a.isParticipating)
                .ToList();
                return View(GA);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
