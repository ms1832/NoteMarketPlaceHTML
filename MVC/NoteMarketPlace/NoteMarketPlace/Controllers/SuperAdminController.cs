using NoteMarketPlace.Models.AdminModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NoteMarketPlace.Controllers
{

    [Authorize(Roles = "Super_Admin")]
    [RoutePrefix("Admin")]
    public class SuperAdminController : Controller
    {

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



        // get manage Admin
        [Route("ManageAdmin")]
        public ActionResult ManageAdmin()
        {
            using(var _Context = new ApplicationContext())
            {
                var model = (from User in _Context.Users
                             where User.RoleId == 2
                             join Detail in _Context.User_Details on User.UserId equals Detail.UserId
                             select new ManageAdminModel
                             {
                                 Id = User.UserId,
                                 FirstName = User.First_Name,
                                 LastName = User.Last_Name,
                                 Email = User.Last_Name,
                                 Phone = Detail.Phone_No,
                                 Active = User.IsActive == true ? "Yes" : "No",
                                 DateAdded = User.Create_Date
                             }).OrderByDescending(x=> x.DateAdded).ToList();

                return View(model);
            }
        }


        // get add Admin
        [Route("AddAdmin")]
        public ActionResult AddAdmin(int? edit)
        {
            if (!edit.Equals(null))
            {
                // get admin details
                using(var _Context = new ApplicationContext())
                {
                    var model = (from User in _Context.Users
                                 where User.UserId == edit && User.IsActive == true
                                 join Detail in _Context.User_Details on User.UserId equals Detail.UserId
                                 select new AddAdmin
                                 {
                                     Id = User.UserId,
                                     FirstName = User.First_Name,
                                     LastName = User.Last_Name,
                                     Email = User.Email,
                                     CountryCode = Detail.Phone_No_Country_Code,
                                     Phone = Detail.Phone_No
                                 }).Single();

                    var countryCode = _Context.Country_Details.Where(m => m.IsActive == true).ToList();
                    //model.CountryModel = countryCode.Select(x=> new Models.CountryModel { Country_Code = x.Country_Code}).ToList();

                    ViewBag.PhoneCode = countryCode;
                    ViewBag.Edit = true;

                    return View(model);
                }
            }
            else
            {
                using(var _Context = new ApplicationContext())
                {
                    var countryCode = _Context.Country_Details.Where(m => m.IsActive == true).ToList();
                    ViewBag.PhoneCode = countryCode;
                    AddAdmin model = new AddAdmin();
                    //model.CountryModel = countryCode.Select(x => new Models.CountryModel { Country_Code = x.Country_Code }).ToList();


                    ViewBag.Edit = false;
                    return View();
                }
            }

        }



        // add Admin post
        [Route("AddAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAdmin(int? id, AddAdmin model)
        {

            if (!ModelState.IsValid)
            {
                if (id.Equals(null))
                {
                    return RedirectToAction("AddAdmin");
                }
                else
                {
                    return RedirectToAction("AddAdmin",id);
                }
                
            }

            using (var _Context = new ApplicationContext())
            {
                var CurrentUser = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                //for edit details
                if (!id.Equals(null))
                {
                    var data = _Context.Users.Single(m => m.UserId == id);
                    var details = _Context.User_Details.Single(m => m.UserId == id);

                    model.MaptoModel(data, details);
                    data.Modified_By = CurrentUser;
                    data.Modified_Date = DateTime.Now;
                    details.Modified_date = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("ManageAdmin");
                }
                //add new Admin
                else
                {
                    var create = _Context.Users;
                    create.Add(new User
                    {
                        First_Name = model.FirstName,
                        Last_Name = model.LastName,
                        Email = model.Email,
                        Password = "",
                        Create_Date = DateTime.Now
                    });

                    _Context.SaveChanges();

                    var newAdmin = _Context.Users.Single(m => m.Email == model.Email);

                    var details = _Context.User_Details;
                    details.Add(new User_Details
                    {
                        Phone_No_Country_Code = model.CountryCode,
                        Phone_No = model.Phone,
                        Address_Line1 = "",
                        City = "",
                        State = "",
                        ZipCode = "",
                        Country_Id =1,
                    });

                    _Context.SaveChanges();


                    return RedirectToAction("ManageAdmin");
                }


            }


        }


        // delete Admin
        [Route("DeleteAdmin")]
        public void DeleteAdmin(int id)
        {
            using(var _Context = new ApplicationContext())
            {
                var CurrentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                var Admin = _Context.Users.Single(m => m.UserId == id);
                Admin.IsActive = false;
                Admin.Modified_By = CurrentAdmin;
                Admin.Modified_Date = DateTime.Now;

                _Context.SaveChanges();
            }
        }



        // get system config
        [Route("ManageSystemConfig")]
        public ActionResult SystemConfig()
        {
            using(var _Context = new ApplicationContext())
            {
                var systemConfig = _Context.System_Config.ToList();

                if(systemConfig.Count != 0)
                {
                    SystemConfig model = new SystemConfig
                    {
                        SupportEmail = systemConfig.Single(m => m.Name == "SupportEmailAddress").Value,
                        Contact = systemConfig.Single(m => m.Name == "SupportContact").Value,
                        DefaultNoteImg = systemConfig.Single(m => m.Name == "DefaultBookImage").Value,
                        DefaulDpImg = systemConfig.Single(m => m.Name == "DefaultProfileImage").Value,
                        Emails = systemConfig.Single(m => m.Name == "EmailAddresses").Value,
                        FacebookUrl = systemConfig.Single(m => m.Name == "Facebook").Value,
                        TwitterUrl = systemConfig.Single(m => m.Name == "Twitter").Value,
                        LinkedinUrl = systemConfig.Single(m => m.Name == "Linkedin").Value
                    };

                    return View(model);
                }
                else
                {
                    return View();
                }

            }
        }



        [Route("ManageSystemConfig")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SystemConfig(SystemConfig model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using(var _Context = new ApplicationContext())
            {
                var systemConfig = _Context.System_Config.ToList();

                if(systemConfig.Single(m => m.Name == "SupportEmailAddress").Value != model.SupportEmail)
                {
                    systemConfig.Single(m => m.Name == "SupportEmailAddress").Value = model.SupportEmail;
                    systemConfig.Single(m => m.Name == "SupportEmailAddress").Modified_Date = DateTime.Now;
                }

                if (systemConfig.Single(m => m.Name == "SupportContact").Value != model.Contact)
                {
                    systemConfig.Single(m => m.Name == "SupportContact").Value = model.Contact;
                    systemConfig.Single(m => m.Name == "SupportContact").Modified_Date = DateTime.Now;
                }

                if (systemConfig.Single(m => m.Name == "DefaultBookImage").Value != model.DefaultNoteImg)
                {
                    systemConfig.Single(m => m.Name == "DefaultBookImage").Value = model.DefaultNoteImg;
                    systemConfig.Single(m => m.Name == "DefaultBookImage").Modified_Date = DateTime.Now;
                }

                if (systemConfig.Single(m => m.Name == "DefaultProfileImage").Value != model.DefaulDpImg)
                {
                    systemConfig.Single(m => m.Name == "DefaultProfileImage").Value = model.DefaulDpImg;
                    systemConfig.Single(m => m.Name == "DefaultProfileImage").Modified_Date = DateTime.Now;
                }

                if (systemConfig.Single(m => m.Name == "EmailAddresses").Value != model.Emails)
                {
                    systemConfig.Single(m => m.Name == "EmailAddresses").Value = model.Emails;
                    systemConfig.Single(m => m.Name == "EmailAddresses").Modified_Date = DateTime.Now;
                }

                if (systemConfig.Single(m => m.Name == "Facebook").Value != model.FacebookUrl)
                {
                    systemConfig.Single(m => m.Name == "Facebook").Value = model.FacebookUrl;
                    systemConfig.Single(m => m.Name == "Facebook").Modified_Date = DateTime.Now;
                }

                if (systemConfig.Single(m => m.Name == "Twitter").Value != model.TwitterUrl)
                {
                    systemConfig.Single(m => m.Name == "Twitter").Value = model.TwitterUrl;
                    systemConfig.Single(m => m.Name == "Twitter").Modified_Date = DateTime.Now;
                }

                if (systemConfig.Single(m => m.Name == "Linkedin").Value != model.LinkedinUrl)
                {
                    systemConfig.Single(m => m.Name == "Linkedin").Value = model.LinkedinUrl;
                    systemConfig.Single(m => m.Name == "Linkedin").Modified_Date = DateTime.Now;
                }

                _Context.SaveChanges();

                return View(model);
            }

        }


    }
}