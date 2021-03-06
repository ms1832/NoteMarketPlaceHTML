using NoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NoteMarketPlace.Controllers
{
    [AllowAnonymous]
    public class SignUpController : Controller
    {

        private ApplicationContext _Context;

        public SignUpController()
        {
            _Context = new ApplicationContext();
        }


        [Route("SignUp")]
        public ActionResult SignUp()
        {
            return View();
        }


        [HttpPost]
        [Route("SignUp")]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpModel user)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("SignUp", "SignUp");
            }

            // password & confirm password does not match
            if(user.Password != user.ConfPassword)
            {
                return View("SignUp");
            }

            // find if user is registered or not
            var finduser = _Context.Users.Where(m => m.Email == user.Email).FirstOrDefault();
            if (finduser != null)
            {
                TempData["HasEmail"] = "1";
                return RedirectToAction("SignUp");
            }

            // new user
            var create = _Context.Users;
            create.Add(new User
            {
                First_Name = user.Firstname,
                Last_Name = user.Lastname,
                Email = user.Email,
                Password = user.Password,
                Create_Date = DateTime.Now,
                IsActive = true,
                RoleId = 3
            });

            // Email Body
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/MailTemplate/AccountConfirmation.html")))
            {
                body = reader.ReadToEnd();
            }

            var callbackUrl = Url.Action("ConfirmEmail", "SignUp", new { userId = user.Email, pass = user.Password }, protocol: Request.Url.Scheme);

            body = body.Replace("{Username}",user.Firstname);
            body = body.Replace("{ConfirmationLink}", callbackUrl);


            try
            {
                // send email
                bool isSend = SendEmail.EmailSend(user.Email, "Note MarketPlace - Email Verification", body , true);
                
                // save to database
                _Context.SaveChanges();
                _Context.Dispose();

                TempData["Message"] = "User Registration Successfull. Please Verify Email";
            }
            catch (Exception e)
            {
            }

            return RedirectToAction("SignUp", "SignUp");

        }




        [Route("SignUp/ConfirmEmail")]
        public ActionResult ConfirmEmail(string userId, string pass)
        {
            var check = _Context.Users.Where(m => m.Email == userId).FirstOrDefault();
            
            if(check != null)
            {
                if (check.Password.Equals(pass))
                {
                    check.IsEmailVerified = true;
                    
                    _Context.SaveChanges();
                    _Context.Dispose();

                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    return Content("Invalid Credentials");
                }
            }

            return Content("Invalid Credentials");
        }


    }
}