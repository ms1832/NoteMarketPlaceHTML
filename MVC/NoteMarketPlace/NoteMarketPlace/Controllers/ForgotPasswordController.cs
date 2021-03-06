using NoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NoteMarketPlace.Controllers
{
    [AllowAnonymous]
    public class ForgotPasswordController : Controller
    {
        private readonly Random pass = new Random();

        
        [Route("Forgot_Password")]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [Route("Forgot_Password")]
        public ActionResult Change_Password(ForgotPassword model)
        {
            using(var _context = new ApplicationContext())
            {
                // random password generate
                int pass_num = pass.Next(100000, 999999);

                var result = _context.Users.FirstOrDefault(m => m.Email == model.Email);
                if(result != null)
                {
                    string body = " <h3>Hello, <br>We have generated a new password for you <br>Password: <b>"+ pass_num.ToString() + "</b></h3>";
                    try
                    {
                        // send email
                        bool isSend = SendEmail.EmailSend(model.Email, "New Temporary Password has been created for you", body, true);

                        result.Password = pass_num.ToString();
                        _context.SaveChanges();

                        TempData["Message"] = "Your new Password is sent to your email Address.";
                    }
                    catch(Exception e)
                    {
                    }

                    return View("ForgotPassword");
                }
                else
                {
                    TempData["HasEmail"] = "0";
                    return View("ForgotPassword");
                }

            }


            
        }




    }
}