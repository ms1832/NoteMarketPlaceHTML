using NoteMarketPlace.Models.AdminModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NoteMarketPlace.Controllers
{
    [Authorize(Roles ="Admin, Super_Admin")]
    [RoutePrefix("Admin")]
    public class AdminController : Controller
    {
        
        // get Dashboard
        [Route("Dashboard")]
        public ActionResult Dashboard(int? month)
        {
            using(var _Context = new ApplicationContext())
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
                for(int i = 0; i<= 6; i++)
                {
                    monthList.Add( new MonthModel 
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
                            let total = (_Context.Purchase_Details.Where(m=> m.Note_Id == Note.Id).Count())
                            select new DashboardModel
                            {
                                Id = Note.Id,
                                Title = Note.Title,
                                Category = Category.Category_Name,
                                Price = Note.Price,
                                Publisher = User.First_Name +" "+ User.Last_Name,
                                PublishDate = (DateTime)Note.Published_Date,
                                publishMonth = Note.Published_Date.Value.Month,
                                TotalDownloads = total,
                                userid = Note.User_Id,
                                filename = Attachment.FileName
                            }).ToList();
                
                // append attachment size
                foreach(var data in note)
                {
                    data.AttachmentSize = GetSize(data.userid,data.Id,data.filename) ;
                }

                if(month == null)
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
        public float GetSize(int user,int note, string filename)
        {
            string filePath = Server.MapPath("../Members/" + user + "/" + note + "/Attachment/" + filename);
            System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
            return (fs.Length/1000);
        }


        // get notesUnderReview
        [Route("NotesUnderReview")]
        public ActionResult NotesUnderReview(int? sellerId)
        {
            using(var _Context = new ApplicationContext())
            {
                // seller names
                var seller = (from Notes in _Context.Note_Details
                              join User in _Context.Users on Notes.User_Id equals User.UserId
                              where Notes.Status == 3 || Notes.Status == 4
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
                             where Notes.Status == 3 || Notes.Status == 4
                             select new NotesUnderReviewModel
                             {
                                 NoteId = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Category_Name,
                                 SellerId = Notes.User_Id,
                                 Seller = User.First_Name+" "+User.Last_Name,
                                 status = Status.Value,
                                 DateAdded = Notes.Added_Date
                             }).ToList();

                if(sellerId == null)
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



        // get published notes
        [Route("PublishedNotes")]
        public ActionResult PublishedNotes(int? sellerId)
        {
            using(var _Context = new ApplicationContext())
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
                             let total = (_Context.Purchase_Details.Where(m => m.Note_Id == Notes.Id).Count())
                             select new PublishedNoteModel
                             {
                                 NoteId = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Category_Name,
                                 Price = Notes.Price,
                                 SellerId = Notes.User_Id,
                                 Seller = User.First_Name+" "+User.Last_Name,
                                 ApprovedBy = Admin.First_Name+" "+Admin.Last_Name,
                                 PublishDate = (DateTime)Notes.Published_Date,
                                 TotalDownloads = total
                             }).ToList();

                if(sellerId == null)
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



        
        [Route("DownloadedNotes")]
        public ActionResult DownloadedNotes(int? noteId,int? sellerId,int? buyerId)
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
                             }).ToList();


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








        [Route("MyProfile")]
        public ActionResult MyProfile()
        {
            return View();
        }


        [HttpPost]
        [Route("MyProfile")]
        public ActionResult MyProfile(MyProfile profile)
        {

            return View(profile);
        }


    }
}