using NoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace NoteMarketPlace.Controllers
{
    [Authorize(Roles ="Member, Admin, Super_Admin")]
    public class ChangePasswordController : Controller
    {
        
        [Route("User/ChangePassword")]
        public ActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/ChangePassword")]
        public ActionResult ChangePassword(ChangePassword model)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

            using(var _Context = new ApplicationContext())
            {
                // get current user
                var currentUser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name);

                // old password not match
                if (!currentUser.Password.Equals(model.OldPassword))
                {
                    TempData["Oldpwd"] = "1";
                    return View();
                }

                // new pwd & conf-pwd not match
                if (!model.NewPassword.Equals(model.ConfirmPassword))
                {
                    return View();
                }

                // old pwd & new pwd same
                if (currentUser.Password == model.ConfirmPassword)
                {
                    TempData["OldpwdSame"] = "1";
                    return View();
                }

                // update password
                currentUser.Password = model.ConfirmPassword;
                currentUser.Modified_Date = DateTime.Now;
                _Context.SaveChanges();

                FormsAuthentication.SignOut();

                return RedirectToAction("Login","Login");
            }

            
        }



    }
}