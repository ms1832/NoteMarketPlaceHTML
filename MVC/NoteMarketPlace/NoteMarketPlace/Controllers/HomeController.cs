using NoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NoteMarketPlace.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        // constructor
        public HomeController()
        {
            using (var _Context = new ApplicationContext())
            {
                // set social urls
                var socialUrl = _Context.System_Config.Where(m => m.Name == "Facebook" || m.Name == "Twitter" || m.Name == "Linkedin").ToList();
                ViewBag.URLs = socialUrl;
            }
        }

        // initialize user info
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (requestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                using (var _Context = new ApplicationContext())
                {
                    //current user profile img
                    var img = (from Details in _Context.User_Details
                               join Users in _Context.Users on Details.UserId equals Users.UserId
                               where Users.Email == requestContext.HttpContext.User.Identity.Name
                               select Details.Profile_Img).FirstOrDefault();

                    if (img == null)
                    {
                        // set default image
                        var defaultImg = _Context.System_Config.FirstOrDefault(m => m.Name == "DefaultProfileImage").Value;
                        ViewBag.UserProfile = defaultImg;
                    }
                    else
                    {
                        ViewBag.UserProfile = img;
                    }
                }
            }

        }


        [Route("")]
        [Route("Index")]
        public ActionResult Index()
        {
            // if user is logged in
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Member"))
                {
                    return RedirectToAction("Search_Notes", "User");
                }
                if(User.IsInRole("Admin") || User.IsInRole("Super_Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }   
            }

            return View();
        }


        [Route("FAQ")]
        [Route("User/FAQ")]
        public ActionResult FAQ()
        {
            return View();
        }


        [Route("ContactUs")]
        public ActionResult ContactUs()
        {
            return View();
        }


        [HttpPost]
        [Route("ContactUs")]
        public ActionResult ContactUs(ContactUsModal modal)
        {
            string Body = "Hello, \n" + modal.Comment + "\n" + "Regards,\n" + modal.FullName;

            bool isSend = SendEmail.EmailSend(modal.Email,modal.Subject,Body, false);

            return View();
        }



    }
}