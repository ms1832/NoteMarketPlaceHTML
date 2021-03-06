using NoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace NoteMarketPlace.Controllers
{

    [AllowAnonymous]
    public class LoginController : Controller
    {

        private ApplicationContext _Context;
        
        public LoginController()
        {
            _Context = new ApplicationContext();
        }



        [Route("Login")]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Search_Notes","User");
            }

            TempData["WrongPass"] = "";
            return View();
        }


        [HttpPost]
        [Route("Login")]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel user)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Login","Login");
            }

            var result = _Context.Users.Where(m => m.Email == user.Email).FirstOrDefault();

            if(result == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // user is not active
            if (result.IsActive == false)
            {
                return RedirectToAction("Login", "Login");
            }

            // email not verified
            if (result.IsEmailVerified == false)
            {
                return RedirectToAction("Login", "Login");
            }

            // check password
            if (result.Password.Equals(user.Password))
            {
                // if Remember me is checked
                if(user.RememberMe.Equals("on"))
                {
                    FormsAuthentication.SetAuthCookie(result.Email, true);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(result.Email, false);
                }

                // user is Member
                if (result.RoleId == 3)
                {
                    // if first time login
                    var firsttime = _Context.User_Details.FirstOrDefault(m => m.UserId == result.UserId);

                    if (firsttime == null)
                    {
                        return RedirectToAction("MyProfile", "User");
                    }
                    else
                    {
                        return RedirectToAction("Search_Notes", "User");
                    }
                }
                // user is Admin
                else
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

            }
            else
            {
                TempData["WrongPass"] = "wrong_password";
                return View("Login");
            }


        }


    }
}