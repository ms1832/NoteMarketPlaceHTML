﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NoteMarketPlace.Controllers
{
    public class SuperAdminController : Controller
    {

        // get manage Admin
        public ActionResult ManageAdmin()
        {
            return View();
        }


        // add Admin
        public ActionResult AddAdmin()
        {
            return View();
        }


    }
}