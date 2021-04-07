using NoteMarketPlace.Models;
using NoteMarketPlace.Models.AdminModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace NoteMarketPlace.Controllers
{
    [Authorize(Roles = "Admin, Super_Admin")]
    [RoutePrefix("Admin")]
    public class AdminController : Controller
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


        // get Dashboard
        [Route("Dashboard")]
        public ActionResult Dashboard(int? month)
        {
            using (var _Context = new ApplicationContext())
            {
                // total notes in review
                var inReviewNote = _Context.Note_Details.Where(m => (m.Status == 4 || m.Status == 5) && m.IsActive == true).Count();
                // total notes downloaded (last 7 days)
                var condition = DateTime.Now.Date.AddDays(-7);
                var downloads = _Context.Purchase_Details.Where(m => m.Allow_Download == true && m.IsAttachment_Downloaded == true && m.AttachmentDownload_Date >= condition).Count();
                // total new Registration (last 7 days)
                var registration = _Context.Users.Where(m => m.Create_Date >= condition).Count();

                ViewBag.InReview = inReviewNote;
                ViewBag.Downloads = downloads;
                ViewBag.Registration = registration;


                // last 6 month from today
                var monthList = new List<MonthModel>();
                for (int i = 0; i <= 6; i++)
                {
                    monthList.Add(new MonthModel
                    {
                        digit = DateTime.Today.AddMonths(-i).Month,
                        Month = DateTime.Today.AddMonths(-i).ToString("MMMM")
                    });
                }
                ViewBag.MonthList = monthList;


                // published note
                var note = (from Note in _Context.Note_Details
                            join Attachment in _Context.NotesAttachments on Note.Id equals Attachment.NoteId
                            join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                            join User in _Context.Users on Note.User_Id equals User.UserId
                            where Note.Status == 6
                            let total = (_Context.Purchase_Details.Where(m => m.Note_Id == Note.Id).Count())
                            select new DashboardModel
                            {
                                Id = Note.Id,
                                Title = Note.Title,
                                Category = Category.Category_Name,
                                Price = Note.Price,
                                Publisher = User.First_Name + " " + User.Last_Name,
                                PublishDate = (DateTime)Note.Published_Date,
                                publishMonth = Note.Published_Date.Value.Month,
                                TotalDownloads = total,
                                userid = Note.User_Id,
                                filename = Attachment.FileName
                            }).OrderByDescending(x=> x.TotalDownloads).ToList();

                // append attachment size
                foreach (var data in note)
                {
                    data.AttachmentSize = GetSize(data.userid, data.Id, data.filename);
                }

                if (month == null)
                {
                    var filternote = note.Where(m => m.publishMonth == DateTime.Now.Month).ToList();
                    return View(filternote);
                }
                else
                {
                    var filternote = note.Where(m => m.publishMonth == month).ToList();
                    return View(filternote);
                }

            }

        }


        // returns file size in KB
        public float GetSize(int user, int note, string filename)
        {
            string filePath = Server.MapPath("../Members/" + user + "/" + note + "/Attachment/" + filename);
            System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
            return (fs.Length / 1000);
        }


        // return file
        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
            {
                throw new System.IO.IOException(s);
            }
            return data;
        }


        // download note over browser
        [Route("DownloadFile")]
        public FileResult DownloadFile(int noteid)
        {
            using (var _Context = new ApplicationContext())
            {
                var file = (from Attachment in _Context.NotesAttachments
                            where Attachment.NoteId == noteid
                            join Note in _Context.Note_Details on Attachment.NoteId equals Note.Id
                            select new
                            {
                                Note.User_Id,
                                Attachment.FileName
                            }).SingleOrDefault();

                string filepath = Server.MapPath("../Members/" + file.User_Id + "/" + noteid + "/Attachment/" + file.FileName);
                byte[] filebyte = GetFile(filepath);

                return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, file.FileName);
            }
        }



        // unpublish note
        [HttpPost]
        [Route("Unpublishnote")]
        public ActionResult Unpublishnote(int noteid, string Remarks)
        {
            using (var _Context = new ApplicationContext())
            {
                int currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                var note = _Context.Note_Details.Single(m => m.Id == noteid);

                var seller = _Context.Users.Single(m => m.UserId == note.User_Id);

                note.Status = 8;
                note.AdminReview = Remarks;
                note.Review_By = currentAdmin;
                note.Edited_Date = DateTime.Now;

                _Context.SaveChanges();


                // send mail to admins
                string subject = "Sorry! We need to remove your notes from our portal.";
                string body = "Hello " + seller.First_Name+" "+seller.Last_Name + ",\n"
                    + "We want to inform you that, your note <Note Title> has been removed from the portal. Please find our remarks as below -\n";
                body += Remarks;
                body += "\nRegards,\nNotes Marketplace";

                bool isSend = SendEmail.EmailSend(seller.Email, subject, body, false);


                return RedirectToAction("PublishedNotes");
            }
        }



        // get notesUnderReview
        [Route("NotesUnderReview")]
        public ActionResult NotesUnderReview(int? sellerId)
        {
            using (var _Context = new ApplicationContext())
            {
                // seller names
                var seller = (from Notes in _Context.Note_Details
                              join User in _Context.Users on Notes.User_Id equals User.UserId
                              where Notes.Status == 3 || Notes.Status == 4 || Notes.Status == 5
                              group new { Notes, User } by Notes.User_Id into grp
                              select new SellerModel
                              {
                                  SellerId = grp.Select(x => x.User.UserId).FirstOrDefault(),
                                  SellerName = grp.Select(x => x.User.First_Name).FirstOrDefault() + " " + grp.Select(x => x.User.Last_Name).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;

                // model data
                var model = (from Notes in _Context.Note_Details
                             join Status in _Context.Status on Notes.Status equals Status.Id
                             join Category in _Context.Category_Details on Notes.Category_Id equals Category.Category_Id
                             join User in _Context.Users on Notes.User_Id equals User.UserId
                             where Notes.Status == 3 || Notes.Status == 4 || Notes.Status == 5
                             select new NotesUnderReviewModel
                             {
                                 NoteId = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Category_Name,
                                 SellerId = Notes.User_Id,
                                 Seller = User.First_Name + " " + User.Last_Name,
                                 status = Status.Value,
                                 DateAdded = Notes.Added_Date
                             }).OrderByDescending(x=> x.DateAdded).ToList();

                if (sellerId == null)
                {
                    return View(model);
                }
                else
                {
                    var filtermodel = model.Where(m => m.SellerId == sellerId).ToList();
                    return View(filtermodel);
                }

            }

        }



        // change note status
        [HttpPost]
        [Route("NoteStatusUpdate")]
        public void NoteStatusUpdate(int noteid,string status)
        {
            using(var _Context = new ApplicationContext())
            {
                var currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                var note = _Context.Note_Details.Single(m => m.Id == noteid);

                switch (status)
                {
                    case "InReview":
                        note.Status = 5;
                        break;
                    case "Approve":
                        note.Status = 6;
                        note.Published_Date = DateTime.Now;
                        break;
                }

                note.Review_By = currentAdmin;
                note.Edited_Date = DateTime.Now;

                _Context.SaveChanges();
            }

        }



        // reject note
        [HttpPost]
        [Route("RejectNote")]
        public ActionResult RejectNote(int noteId, string Reject)
        {
            using(var _Context = new ApplicationContext())
            {
                var currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                var note = _Context.Note_Details.Single(m => m.Id == noteId);
                note.Status = 7;
                note.Review_By = currentAdmin;
                note.AdminReview = Reject;
                note.Edited_Date = DateTime.Now;

                _Context.SaveChanges();

                return RedirectToAction("NotesUnderReview");
            }
        }



        // get published notes
        [Route("PublishedNotes")]
        public ActionResult PublishedNotes(int? sellerId)
        {
            using (var _Context = new ApplicationContext())
            {
                // seller names
                var seller = (from Notes in _Context.Note_Details
                              join User in _Context.Users on Notes.User_Id equals User.UserId
                              where Notes.Status == 6
                              group new { Notes, User } by Notes.User_Id into grp
                              select new SellerModel
                              {
                                  SellerId = grp.Select(x => x.User.UserId).FirstOrDefault(),
                                  SellerName = grp.Select(x => x.User.First_Name).FirstOrDefault() + " " + grp.Select(x => x.User.Last_Name).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;


                // set model value
                var model = (from Notes in _Context.Note_Details
                             join User in _Context.Users on Notes.User_Id equals User.UserId
                             join Admin in _Context.Users on Notes.Review_By equals Admin.UserId
                             join Category in _Context.Category_Details on Notes.Category_Id equals Category.Category_Id
                             join Attachment in _Context.NotesAttachments on Notes.Id equals Attachment.NoteId
                             where Notes.Status == 6
                             let total = (_Context.Purchase_Details.Where(m => m.Note_Id == Notes.Id && m.Allow_Download == true).Count())
                             select new PublishedNoteModel
                             {
                                 NoteId = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Category_Name,
                                 Price = Notes.Price,
                                 SellerId = Notes.User_Id,
                                 Seller = User.First_Name + " " + User.Last_Name,
                                 ApprovedBy = Admin.First_Name + " " + Admin.Last_Name,
                                 PublishDate = (DateTime)Notes.Published_Date,
                                 TotalDownloads = total
                             }).OrderByDescending(x=> x.PublishDate).ToList();

                if (sellerId == null)
                {
                    return View(model);
                }
                else
                {
                    var filtermodel = model.Where(m => m.SellerId == sellerId).ToList();
                    return View(filtermodel);
                }


            }


        }



        // get downloaded notes
        [Route("DownloadedNotes")]
        public ActionResult DownloadedNotes(int? noteId, int? sellerId, int? buyerId)
        {
            using (var _Context = new ApplicationContext())
            {
                // seller names
                var seller = (from Notes in _Context.Note_Details
                              join User in _Context.Users on Notes.User_Id equals User.UserId
                              where Notes.Status == 6
                              group new { Notes, User } by Notes.User_Id into grp
                              select new SellerModel
                              {
                                  SellerId = grp.Select(x => x.User.UserId).FirstOrDefault(),
                                  SellerName = grp.Select(x => x.User.First_Name).FirstOrDefault() + " " + grp.Select(x => x.User.Last_Name).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;

                // buyer names
                var buyer = (from Purchase in _Context.Purchase_Details
                             join User in _Context.Users on Purchase.Downloader equals User.UserId
                             where Purchase.Allow_Download == true && Purchase.IsAttachment_Downloaded == true
                             group new { Purchase, User } by Purchase.Downloader into grp
                             select new BuyerModel
                             {
                                 BuyerId = grp.Select(x => x.User.UserId).FirstOrDefault(),
                                 BuyerName = grp.Select(x => x.User.First_Name).FirstOrDefault() + " " + grp.Select(x => x.User.Last_Name).FirstOrDefault()
                             }).ToList();

                ViewBag.BuyerList = buyer;

                // notes title
                var note = (from Purchase in _Context.Purchase_Details
                            join Note in _Context.Note_Details on Purchase.Note_Id equals Note.Id
                            where Purchase.Allow_Download == true && Purchase.IsAttachment_Downloaded == true
                            group new { Note, Purchase } by Note.Id into grp
                            select new NoteModel
                            {
                                NoteId = grp.Select(x => x.Purchase.Note_Id).FirstOrDefault(),
                                NoteTitle = grp.Select(x => x.Note.Title).FirstOrDefault()
                            }).ToList();

                ViewBag.NoteList = note;

                // set model data
                var model = (from Purchase in _Context.Purchase_Details
                             join Note in _Context.Note_Details on Purchase.Note_Id equals Note.Id
                             join Downloader in _Context.Users on Purchase.Downloader equals Downloader.UserId
                             join Seller in _Context.Users on Purchase.Seller equals Seller.UserId
                             join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                             where Purchase.Allow_Download == true && Purchase.IsAttachment_Downloaded == true
                             select new DownloadedNotes
                             {
                                 NoteId = Purchase.Note_Id,
                                 Title = Note.Title,
                                 Category = Category.Category_Name,
                                 Price = Purchase.PurchasedPrice,
                                 SellerId = Seller.UserId,
                                 BuyerId = Downloader.UserId,
                                 SellerName = Seller.First_Name + " " + Seller.Last_Name,
                                 BuyerName = Downloader.First_Name + " " + Downloader.Last_Name,
                                 DownloadedDate = (DateTime)Purchase.AttachmentDownload_Date
                             }).OrderByDescending(x=> x.DownloadedDate).ToList();


                var filtermodel = model;

                if (!noteId.Equals(null))
                {
                    filtermodel = filtermodel.Where(m => m.NoteId == noteId).ToList();
                }
                if (!sellerId.Equals(null))
                {
                    filtermodel = filtermodel.Where(m => m.SellerId == sellerId).ToList();
                }
                if (!buyerId.Equals(null))
                {
                    filtermodel = filtermodel.Where(m => m.BuyerId == buyerId).ToList();
                }

                return View(filtermodel);

            }
        }



        // get rejected notes
        [Route("RejectedNotes")]
        public ActionResult RejectedNotes(int? sellerId)
        {
            using (var _Context = new ApplicationContext())
            {
                // seller names
                var seller = (from Notes in _Context.Note_Details
                              join User in _Context.Users on Notes.User_Id equals User.UserId
                              where Notes.Status == 7
                              group new { Notes, User } by Notes.User_Id into grp
                              select new SellerModel
                              {
                                  SellerId = grp.Select(x => x.User.UserId).FirstOrDefault(),
                                  SellerName = grp.Select(x => x.User.First_Name).FirstOrDefault() + " " + grp.Select(x => x.User.Last_Name).FirstOrDefault()
                              }).ToList();

                ViewBag.SellerList = seller;


                // set model data
                var notes = (from Note in _Context.Note_Details
                             join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                             join Seller in _Context.Users on Note.User_Id equals Seller.UserId
                             join Admin in _Context.Users on Note.Review_By equals Admin.UserId
                             where Note.Status == 7
                             select new RejectedNotesModel
                             {
                                 NoteId = Note.Id,
                                 Title = Note.Title,
                                 Category = Category.Category_Name,
                                 SellerId = Note.User_Id,
                                 SellerName = Seller.First_Name + " " + Seller.Last_Name,
                                 RejectedBy = Admin.First_Name + " " + Admin.Last_Name,
                                 Remarks = Note.AdminReview,
                                 DateEdited = (DateTime)Note.Edited_Date
                             }).ToList();


                // if filter applied
                if (!sellerId.Equals(null))
                {
                    notes = notes.Where(x => x.SellerId == sellerId).ToList();
                }


                return View(notes);
            }

        }



        // get members
        [Route("Members")]
        public ActionResult Members()
        {
            using (var _Context = new ApplicationContext())
            {

                var model = (from User in _Context.Users
                             where User.RoleId == 3 && User.IsActive == true
                             let underReview = (from Notes in _Context.Note_Details
                                                where Notes.User_Id == User.UserId && (Notes.Status == 4 || Notes.Status == 5)
                                                select Notes).Count()
                             let published = (from Notes in _Context.Note_Details
                                              where Notes.User_Id == User.UserId && Notes.Status == 6
                                              select Notes).Count()
                             let downloaded = (from Purchase in _Context.Purchase_Details
                                               where Purchase.Downloader == User.UserId && Purchase.IsAttachment_Downloaded == true
                                               select Purchase)
                             let sell = (from Purchase in _Context.Purchase_Details
                                         where Purchase.Seller == User.UserId && Purchase.Allow_Download == true
                                         select Purchase)
                             select new MembersModel
                             {
                                 Id = User.UserId,
                                 FirstName = User.First_Name,
                                 LastName = User.Last_Name,
                                 Email = User.Email,
                                 JoinDate = User.Create_Date,
                                 UnderReviewNotes = underReview,
                                 PublishedNotes = published,
                                 DownloadedNotes = downloaded.Count(),
                                 TotalExpense = downloaded.Sum(x => x.PurchasedPrice),
                                 TotalEarning = sell.Sum(x => x.PurchasedPrice)
                             }).OrderByDescending(x=> x.JoinDate).ToList();

                return View(model);
            }

        }


        // deactivate member
        [Route("DeactivateUser")]
        public void DeactivateUser(int userId)
        {
            using(var _Context = new ApplicationContext())
            {
                var currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                var user = _Context.Users.Single(m => m.UserId == userId);

                user.IsActive = false;
                user.Modified_By = currentAdmin;
                user.Modified_Date = DateTime.Now;
                _Context.SaveChanges();

                var notes = _Context.Note_Details.Where(m => m.User_Id == userId).ToList();
                
                for(int i=0; i< notes.Count; i++)
                {
                    var note = notes[i];

                    var Attachment = _Context.NotesAttachments.Single(m => m.NoteId == note.Id);
                    Attachment.IsActive = false;
                    Attachment.Modified_By = currentAdmin;
                    Attachment.Modified_Date = DateTime.Now;
                    
                    notes[i].IsActive = false;
                    notes[i].Status = 8;
                    notes[i].Review_By = currentAdmin;
                    notes[i].Edited_Date = DateTime.Now;

                    _Context.SaveChanges();
                }
                
            }
        }



        // get member details
        [Route("MemberDetails")]
        public ActionResult MemberDetails(int id)
        {
            using (var _Context = new ApplicationContext())
            {
                // default img
                var DefaultImg = _Context.System_Config.SingleOrDefault(m => m.Name == "DefaultProfileImage").Value;

                // member details
                var details = (from User in _Context.Users
                               where User.UserId == id
                               join Details in _Context.User_Details on User.UserId equals Details.UserId
                               join Country in _Context.Country_Details on Details.Country_Id equals Country.Country_Id
                               select new MembersModel
                               {
                                   FirstName = User.First_Name,
                                   LastName = User.Last_Name,
                                   ProfileImage = Details.Profile_Img == null ? DefaultImg.Remove(0, 2) : Details.Profile_Img.Remove(0, 2),
                                   Email = User.Email,
                                   DOB = Details.DOB,
                                   Phone = Details.Phone_No,
                                   Collage_University = Details.University,
                                   Address1 = Details.Address_Line1,
                                   Address2 = Details.Address_Line2,
                                   City = Details.City,
                                   State = Details.State,
                                   Country = Country.Country_Name,
                                   Zipcode = Details.ZipCode
                               }).SingleOrDefault();

                ViewBag.Details = details;


                // member notes
                var notes = (from Note in _Context.Note_Details
                             where Note.User_Id == id
                             join Status in _Context.Status on Note.Status equals Status.Id
                             join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                             let downloadedNotes = (_Context.Purchase_Details.Where(m => m.Note_Id == Note.Id && m.Seller == id && m.IsAttachment_Downloaded == true).Count())
                             let earning = (_Context.Purchase_Details.Where(m => m.Note_Id == Note.Id && m.Seller == id).Sum(x => x.PurchasedPrice))
                             select new MemberNoteModel
                             {
                                 NoteId = Note.Id,
                                 Title = Note.Title,
                                 Category = Category.Category_Name,
                                 Status = Status.Value,
                                 DownloadedNote = downloadedNotes,
                                 Earning = earning,
                                 DateAdded = Note.Added_Date,
                                 PublishedDate = Note.Published_Date
                             }).ToList();


                return View(notes);
            }

        }



        // get manage category
        [Route("ManageCategory")]
        public ActionResult ManageCategory()
        {
            using (var _Context = new ApplicationContext())
            {
                var data = (from Category in _Context.Category_Details
                            join User in _Context.Users on Category.Added_By equals User.UserId
                            select new ManageCategory
                            {
                                Id = Category.Category_Id,
                                Name = Category.Category_Name,
                                Desctiption = Category.Description,
                                AddedDate = Category.Added_Date,
                                AddedBy = User.First_Name + " " + User.Last_Name,
                                Active = Category.IsActive == true ? "Yes" : "No"
                            }).OrderByDescending(x=> x.AddedDate).ToList();

                return View(data);
            }
        }


        // get add/edit category
        [Route("AddCategory")]
        public ActionResult AddCategory(int? edit)
        {
            ViewBag.Edit = false;

            if (edit != null)
            {
                using (var _Context = new ApplicationContext())
                {
                    var data = _Context.Category_Details.Where(m => m.Category_Id == edit)
                        .Select(x => new AddCategoryModel
                        {
                            Id = x.Category_Id,
                            Name = x.Category_Name,
                            Description = x.Description
                        }).Single();

                    ViewBag.Edit = true;

                    return View(data);
                }
            }

            return View();
        }


        [HttpPost]
        [Route("AddCategory")]
        public ActionResult AddCategory(AddCategoryModel model, int? id)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var _Context = new ApplicationContext())
            {
                // current admin id
                var currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                // if new category
                if (id.Equals(null))
                {
                    // add new category
                    var create = _Context.Category_Details;
                    create.Add(new Category_Details
                    {
                        Category_Name = model.Name,
                        Description = model.Description,
                        Added_By = currentAdmin,
                        Added_Date = DateTime.Now,
                        IsActive = true
                    });

                    _Context.SaveChanges();
                }
                // update existing category
                else
                {
                    var update = _Context.Category_Details.Single(m => m.Category_Id == id);
                    model.MaptoModel(update);
                    update.Modified_By = currentAdmin;
                    update.Modified_date = DateTime.Now;

                    _Context.SaveChanges();
                }

            }
            return RedirectToAction("ManageCategory");
        }


        // get manage type
        [Route("ManageType")]
        public ActionResult ManageType()
        {
            using (var _Context = new ApplicationContext())
            {
                var data = (from Type in _Context.Type_Details
                            join User in _Context.Users on Type.Added_By equals User.UserId
                            select new ManageType
                            {
                                Id = Type.Type_Id,
                                Name = Type.Type_Name,
                                Desctiption = Type.Description,
                                AddedDate = Type.Added_Date,
                                AddedBy = User.First_Name + " " + User.Last_Name,
                                Active = Type.IsActive == true ? "Yes" : "No"
                            }).OrderByDescending(x => x.AddedDate).ToList();

                return View(data);
            }
        }


        // add/edit type
        [Route("AddType")]
        public ActionResult AddType(int? edit)
        {
            ViewBag.Edit = false;

            if (edit != null)
            {
                using (var _Context = new ApplicationContext())
                {
                    var data = _Context.Type_Details.Where(m => m.Type_Id == edit)
                        .Select(x => new AddTypeModel
                        {
                            Id = x.Type_Id,
                            Name = x.Type_Name,
                            Description = x.Description
                        }).Single();

                    ViewBag.Edit = true;

                    return View(data);
                }
            }

            return View();
        }


        [HttpPost]
        [Route("AddType")]
        public ActionResult AddType(AddTypeModel model, int? id)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var _Context = new ApplicationContext())
            {
                // current admin id
                var currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                // if new category
                if (id.Equals(null))
                {
                    // add new category
                    var create = _Context.Type_Details;
                    create.Add(new Type_Details
                    {
                        Type_Name = model.Name,
                        Description = model.Description,
                        Added_By = currentAdmin,
                        Added_Date = DateTime.Now,
                        IsActive = true
                    });

                    _Context.SaveChanges();
                }
                // update existing category
                else
                {
                    var update = _Context.Type_Details.Single(m => m.Type_Id == id);
                    model.MaptoModel(update);
                    update.Edited_By = currentAdmin;
                    update.Edited_Date = DateTime.Now;

                    _Context.SaveChanges();
                }

            }
            return RedirectToAction("ManageType");
        }


        // get manage category
        [Route("ManageCountry")]
        public ActionResult ManageCountry()
        {
            using (var _Context = new ApplicationContext())
            {
                var data = (from Country in _Context.Country_Details
                            join User in _Context.Users on Country.Added_By equals User.UserId
                            select new ManageCountry
                            {
                                Id = Country.Country_Id,
                                CountryName = Country.Country_Name,
                                CountryCode = Country.Country_Code,
                                AddedDate = Country.Added_Date,
                                AddedBy = User.First_Name + " " + User.Last_Name,
                                Active = Country.IsActive == true ? "Yes" : "No"
                            }).OrderByDescending(x => x.AddedDate).ToList();

                return View(data);
            }
        }



        // add/edit country
        [Route("AddCountry")]
        public ActionResult AddCountry(int? edit)
        {
            ViewBag.Edit = false;

            if (edit != null)
            {
                using (var _Context = new ApplicationContext())
                {
                    var data = _Context.Country_Details.Where(m => m.Country_Id == edit)
                        .Select(x => new AddCountryModel
                        {
                            Id = x.Country_Id,
                            CountryName = x.Country_Name,
                            CountryCode = x.Country_Code
                        }).Single();

                    ViewBag.Edit = true;

                    return View(data);
                }
            }

            return View();
        }


        [HttpPost]
        [Route("AddCountry")]
        public ActionResult AddCountry(AddCountryModel model, int? id)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var _Context = new ApplicationContext())
            {
                // current admin id
                var currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                // if new category
                if (id.Equals(null))
                {
                    // add new category
                    var create = _Context.Country_Details;
                    create.Add(new Country_Details
                    {
                        Country_Name = model.CountryName,
                        Country_Code = model.CountryCode,
                        Added_By = currentAdmin,
                        Added_Date = DateTime.Now,
                        IsActive = true
                    });

                    _Context.SaveChanges();
                }
                // update existing category
                else
                {
                    var update = _Context.Country_Details.Single(m => m.Country_Id == id);
                    model.MaptoModel(update);
                    update.Modified_By = currentAdmin;
                    update.Modified_Date = DateTime.Now;

                    _Context.SaveChanges();
                }

            }
            return RedirectToAction("ManageCountry");
        }


        // delete category/type/country
        [HttpPost]
        [Route("DeleteSystemConfigItem")]
        public void DeleteSystemConfigItem(int id,string item)
        {
            using(var _Context = new ApplicationContext())
            {
                var currentAdmin = _Context.Users.Single(m => m.Email == User.Identity.Name).UserId;

                switch (item)
                {
                    case "Category":
                        var category = _Context.Category_Details.Single(m => m.Category_Id == id);
                        category.IsActive = false;
                        category.Modified_By = currentAdmin;
                        category.Modified_date = DateTime.Now;
                        _Context.SaveChanges();
                        break;
                    case "type":
                        var type = _Context.Type_Details.Single(m => m.Type_Id == id);
                        type.IsActive = false;
                        type.Edited_By = currentAdmin;
                        type.Edited_Date = DateTime.Now;
                        _Context.SaveChanges();
                        break;
                    case "Country":
                        var country = _Context.Country_Details.Single(m => m.Country_Id == id);
                        country.IsActive = false;
                        country.Modified_By = currentAdmin;
                        country.Modified_Date = DateTime.Now;
                        _Context.SaveChanges();
                        break;
                }
            }
        }


        // spam reports
        [Route("SpamReports")]
        public ActionResult SpamReports()
        {
            using (var _Context = new ApplicationContext())
            {
                var data = (from Spam in _Context.Spam_Notes
                            join Note in _Context.Note_Details on Spam.NoteId equals Note.Id
                            join User in _Context.Users on Spam.UserId equals User.UserId
                            join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                            select new SpamNotesModel
                            {
                                ID = Spam.Id,
                                NoteId = Spam.NoteId,
                                Title = Note.Title,
                                ReportedBy = User.First_Name + " " + User.Last_Name,
                                Remarks = Spam.Remarks,
                                Category = Category.Category_Name,
                                DateAdded = Spam.Reported_Date
                            }).OrderByDescending(x=> x.DateAdded).ToList();

                return View(data);
            }

        }


        // delete spam report
        [HttpPost]
        [Route("DeleteSpamReport")]
        public void DeleteSpamReport(int Id)
        {
            using(var _Context = new ApplicationContext())
            {
                var report = _Context.Spam_Notes.Single(m => m.Id == Id);
                _Context.Spam_Notes.Remove(report);
                _Context.SaveChanges();
            }
        }


        // get myprofile
        [Route("MyProfile")]
        public ActionResult MyProfile()
        {
            using(var _Context = new ApplicationContext())
            {
                var currentAdmin = (from Admin in _Context.Users
                                    where Admin.Email == User.Identity.Name
                                    join Details in _Context.User_Details on Admin.UserId equals Details.UserId
                                    select new MyProfile
                                    {
                                        ID = Admin.UserId,
                                        First_Name = Admin.First_Name,
                                        Last_Name = Admin.Last_Name,
                                        Email = Admin.Email,
                                        SecondaryEmail = Details.Secondary_Email,
                                        Phonecode = Details.Phone_No_Country_Code,
                                        Phone = Details.Phone_No,
                                        ProfileImage = Details.Profile_Img
                                    }).Single();

                currentAdmin.PhoneCodeModel = _Context.Country_Details.Where(m => m.IsActive == true).Select(x=> new CountryModel { Country_Code = x.Country_Code }).ToList();

                return View(currentAdmin);
            }

        }


        // edit myprofile
        [HttpPost]
        [Route("MyProfile")]
        public ActionResult MyProfile(MyProfile profile)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("MyProfile");
            }

            using(var _Context = new ApplicationContext())
            {
                var user = _Context.Users.Single(x => x.Email == User.Identity.Name);
                var details = _Context.User_Details.Single(x => x.UserId == user.UserId);

                profile.MAptoModel(user,details);

                if (profile.Image == null)
                {
                    details.Profile_Img = details.Profile_Img;
                }
                else
                {
                    details.Profile_Img = "../Members/" + user.UserId + "/" + profile.ProfileImage;
                    string _path = System.IO.Path.Combine(Server.MapPath("~/Members/" + user.UserId), profile.ProfileImage);
                    profile.Image.SaveAs(_path);
                }

                user.Modified_By = user.UserId;
                user.Modified_Date = DateTime.Now;
                details.Modified_date = DateTime.Now;

                _Context.SaveChanges();

                return RedirectToAction("MyProfile");
            }
        }


        // logout
        [Route("LogOut")]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }


    }

}