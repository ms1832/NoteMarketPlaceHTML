using NoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace NoteMarketPlace.Controllers
{

    [Authorize(Roles = "Member")]
    public class UserController : Controller
    {
        // constructor
        public UserController()
        {    
            using (var _Context = new ApplicationContext())
            {
                // set social urls
                var socialUrl = _Context.System_Config.Where(m=> m.Name == "Facebook" || m.Name == "Twitter" || m.Name == "Linkedin").ToList();
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


        // get Dashboard3
        [Route("User/Dashboard")]
        public ActionResult Dashboard()
        {
            using(var _Context = new ApplicationContext())
            {
                // get current user
                var currentUser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name);

                // total earning
                var Earning = (from Purchase in _Context.Purchase_Details
                               where Purchase.Seller == currentUser.UserId && Purchase.Allow_Download == true
                               group Purchase by Purchase.Seller into grp
                               select grp.Sum(m => m.PurchasedPrice)).ToList();
                ViewBag.Earning = Earning.Count() == 0 ? 0 : Earning[0];


                // total notes sold
                var SoldNotes = (from Purchase in _Context.Purchase_Details
                                where Purchase.Seller == currentUser.UserId && Purchase.Allow_Download == true
                                group Purchase by Purchase.Seller into grp
                                select grp.Count()).ToList();
                ViewBag.SoldNotes = SoldNotes.Count() == 0 ? 0 : SoldNotes[0];


                // My download notes
                var DownloadedNotes = (from Purchase in _Context.Purchase_Details
                                       where Purchase.Downloader == currentUser.UserId && Purchase.Allow_Download == true
                                       group Purchase by Purchase.Downloader into grp
                                       select grp.Count()).ToList();
                ViewBag.DownloadNotes = DownloadedNotes.Count() == 0 ? 0 : DownloadedNotes[0];


                // My Rejected Notes
                var RejectedNotes = (from Notes in _Context.Note_Details
                                        join Status in _Context.Status on Notes.Status equals Status.Id
                                        where Status.RefCategory == "Notes Status" && Status.Value == "Rejected" && Notes.User_Id == currentUser.UserId
                                        group Notes by Notes.User_Id into grp
                                        select grp.Count()).ToList();
                ViewBag.RejectedNotes = RejectedNotes.Count() == 0 ? 0 : RejectedNotes[0];


                // Buyer Requests
                var BuyerRequests = (from Purchase in _Context.Purchase_Details
                                        where Purchase.Allow_Download == false && Purchase.Seller == currentUser.UserId
                                        group Purchase by Purchase.Seller into grp
                                        select grp.Count()).ToList();
                ViewBag.BuyerRequest = BuyerRequests.Count() == 0 ? 0 : BuyerRequests[0];



                // in progress notes
                var ProgressNotes = (from Notes in _Context.Note_Details
                                     join Category in _Context.Category_Details on Notes.Category_Id equals Category.Category_Id
                                     join Status in _Context.Status on Notes.Status equals Status.Id
                                     where Status.RefCategory == "Notes Status" &&  Notes.User_Id == currentUser.UserId &&
                                     (Status.Value == "Draft" || Status.Value == "Submitted" || Status.Value == "In Review")
                                     select new UserDashboardInProgressModel
                                     {
                                         Id = Notes.Id,
                                         Title = Notes.Title,
                                         Category = Category.Category_Name,
                                         Status = Status.Value,
                                         AddedDate = Notes.Added_Date
                                     }).OrderByDescending(m=> m.AddedDate).ToList();

                ViewBag.ProgressNotes = ProgressNotes;
                
                // published notes
                var PublishedNotes = (from Notes in _Context.Note_Details
                                      join Category in _Context.Category_Details on Notes.Category_Id equals Category.Category_Id
                                      join Status in _Context.Status on Notes.Status equals Status.Id
                                      where Status.RefCategory == "Notes Status" && Status.Value == "Published" && Notes.User_Id == currentUser.UserId
                                      select new UserDashboardPublishedNoteModel
                                      {
                                          Id = Notes.Id,
                                          Title = Notes.Title,
                                          Category = Category.Category_Name,
                                          Price = Notes.Price,
                                          SellType = (Notes.Price == 0 ? "Free" : "Paid" ),
                                          AddedDate = Notes.Added_Date
                                      }).OrderByDescending(m => m.AddedDate).ToList();

                ViewBag.PublishedNotes = PublishedNotes;


                return View();
            }

            
        }



        // get search note
        [AllowAnonymous]
        [Route("Search_Notes")]
        public ActionResult Search_Notes(int? Type, int? Category, string University, string Course, int? Country, int? Rating, string search)
        {
            using (var _Context = new ApplicationContext())
            {
                // get all types
                var type = _Context.Type_Details.ToList();
                // get all category
                var category = _Context.Category_Details.ToList();
                // get distinct university
                var university = _Context.Note_Details.Where(m => m.University != null).Select(x => x.University).Distinct().ToList();
                // get distinct courses
                var course = _Context.Note_Details.Where(m => m.Course != null).Select(x => x.Course).Distinct().ToList();
                // get all countries
                var country = _Context.Country_Details.ToList();

                // get all book details
                var notes = (from Notes in _Context.Note_Details
                             join Status in _Context.Status on Notes.Status equals Status.Id
                             where Status.Value == "Published" && Notes.IsActive == true
                             let avgRatings = (from Review in _Context.Review_Details
                                               where Review.Note_Id == Notes.Id
                                               group Review by Review.Note_Id into grp
                                               select new AvgRatings
                                               {
                                                   Rating = Math.Round(grp.Average(m => m.Rating)),
                                                   Total = grp.Count()
                                               })
                             let spamNote = (from Spam in _Context.Spam_Notes
                                             where Spam.NoteId == Notes.Id
                                             group Spam by Spam.NoteId into grp
                                             select new SpamNote
                                             {
                                                 Total = grp.Count()
                                             })
                             select new Search_Notes_Model
                             {
                                 Id = Notes.Id,
                                 Title = Notes.Title,
                                 University = Notes.University,
                                 Pages = Notes.Pages == null ? 0 : Notes.Pages,
                                 Image = Notes.Image,
                                 PublishDate = Notes.Published_Date,
                                 Type_Id = Notes.Type_Id,
                                 Category = Notes.Category_Id,
                                 Country = Notes.Country_Id,
                                 Course = Notes.Course,
                                 Rating = avgRatings.Select(a => a.Rating).FirstOrDefault(),
                                 TotalRating = avgRatings.Select(a => a.Total).FirstOrDefault(),
                                 TotalSpams = spamNote.Select(a => a.Total).FirstOrDefault()
                             }).ToList();

                ViewBag.TypeList = type;
                ViewBag.CategoryList = category;
                ViewBag.University = university;
                ViewBag.Course = course;
                ViewBag.Country = country;


                var filternotes = notes;

                // if filter value is available
                if (!Type.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Type_Id == Type).ToList();
                }
                if (!Category.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Category == Category).ToList();
                }
                if (University != null)
                {
                    filternotes = filternotes.Where(m => m.University == University).ToList();
                }
                if (Course != null)
                {
                    filternotes = filternotes.Where(m => m.Course == Course).ToList();
                }
                if (!Country.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Country == Country).ToList();
                }
                if (!Rating.Equals(null))
                {
                    filternotes = filternotes.Where(m => m.Rating >= Rating).ToList();
                }
                if(search != null)
                {
                    filternotes = filternotes.Where(m => m.Title.ToLower().Contains(search.ToLower())).ToList();
                }
                

                return View(filternotes);
            }


        }



        // get note details
        [AllowAnonymous]
        [Route("Note_Details/{id}")]
        public ActionResult Note_Details(int id, bool? ReadOnly)
        {
            using (var _Context = new ApplicationContext())
            {
                // default user image
                var defaultuserImg = _Context.System_Config.FirstOrDefault(m => m.Name == "DefaultProfileImage").Value;

                // get note details
                var notes = (from Notes in _Context.Note_Details
                             join Category in _Context.Category_Details on Notes.Category_Id equals Category.Category_Id
                             let Country = _Context.Country_Details.FirstOrDefault(m => m.Country_Id == Notes.Country_Id)
                             join Users in _Context.Users on Notes.User_Id equals Users.UserId
                             where Notes.Id == id && (Notes.Status == 4 || Notes.Status == 5 || Notes.Status == 6 || Notes.Status == 7)
                             select new NoteDetailsModel
                             {
                                 Id = Notes.Id,
                                 Title = Notes.Title,
                                 Category = Category.Category_Name,
                                 Description = Notes.Description,
                                 Image = Notes.Image,
                                 Price = Notes.Price,
                                 Institute = Notes.University == null ? "--" : Notes.University,
                                 Country = Country.Country_Name == null ? "--" : Country.Country_Name,
                                 CourseName = Notes.Course == null ? "--" : Notes.Course,
                                 CourseCode = Notes.Course_Code == null ? "--" : Notes.Course_Code,
                                 Professor = Notes.Professor == null ? "--" : Notes.Professor,
                                 Pages = (decimal)(Notes.Pages == null ? 0 : Notes.Pages),
                                 ApprovedDate = Notes.Published_Date,
                                 NotePreview = Notes.Note_Preview,
                                 Seller = Users.First_Name + " " + Users.Last_Name,
                                 Status = Notes.Status
                             }).ToList();

                for(int i=0; i<notes.Count; i++)
                {
                    notes[i].ApproveDate = notes[i].ApprovedDate.HasValue ? notes[i].ApprovedDate.GetValueOrDefault().ToString("MMMM dd yyyy") : "N/A";
                }


                // average ratings
                var avg = _Context.Review_Details.Where(m => m.Note_Id == id).ToList();
                if(avg.Count() > 0)
                {
                    var avgReview = Math.Round( Double.Parse( avg.Average(m => m.Rating).ToString() ));
                    var count = avg.Count();
                    ViewBag.TotalReview = count;
                    ViewBag.AverageReview = avgReview;
                }
                else
                {
                    ViewBag.TotalReview = 0;
                    ViewBag.AverageReview = 0;
                }
                

                // spam count
                var spam = _Context.Spam_Notes.Where(m => m.NoteId == id).Count();
                ViewBag.Spam = spam;

                // customer Review List
                var reviews = (from Review in _Context.Review_Details
                               join User in _Context.Users on Review.User_Id equals User.UserId
                               join UserDetail in _Context.User_Details on User.UserId equals UserDetail.UserId
                               where Review.Note_Id == id
                               select new CustomerReview
                               {
                                   First_Name = User.First_Name,
                                   Last_Name = User.Last_Name,
                                   Image = UserDetail.Profile_Img == null ? defaultuserImg : UserDetail.Profile_Img,
                                   Ratings = Review.Rating,
                                   Review = Review.Review_msg
                               }).OrderByDescending(m => m.Ratings).ToList();

                ViewBag.Reviews = reviews;

                if (ReadOnly != null && ReadOnly == true)
                {
                    ViewBag.NoteDetails = notes;

                    TempData["ReadOnly"] = "true";
                    return View();
                }
                else
                {
                    ViewBag.NoteDetails = notes.Where(m => m.Status == 6).ToList();
                    return View();
                }

                
            }

        }



        // make purchase to note
        [Route("Note_Details/Purchase")]
        public ActionResult Purchase_Note(string noteId)
        {
            int noteid = int.Parse(noteId);

            using(var _Context = new ApplicationContext())
            {

                var user = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name);
                var note = _Context.Note_Details.FirstOrDefault(m=> m.Id == noteid);
                

                if (note != null && !user.Equals(null))
                {
                        var create = _Context.Purchase_Details;

                        if (note.Price == 0)
                        {
                            create.Add(new Purchase_Details
                            {
                                Downloader = user.UserId,
                                Note_Id = noteid,
                                Seller = note.User_Id,
                                PurchasedPrice = note.Price,
                                Purchase_Date = DateTime.Now,
                                Allow_Download = true,
                                IsAttachment_Downloaded = true,
                                AttachmentDownload_Date = DateTime.Now
                            });

                            _Context.SaveChanges();

                            var attachment = _Context.NotesAttachments.FirstOrDefault(m => m.NoteId == noteid);

                            // download file direct
                            string filePath = Server.MapPath("../Members/" + note.User_Id.ToString() + "/" + noteid.ToString() + "/Attachment/" + attachment.FileName);
                            byte[] filebyte = GetFile(filePath);

                            return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.FileName);
                        }
                        else
                        {
                            // send download request to seller
                            create.Add(new Purchase_Details
                            {
                                Downloader = user.UserId,
                                Note_Id = noteid,
                                Seller = note.User_Id,
                                PurchasedPrice = note.Price,
                                Purchase_Date = DateTime.Now
                            });
                            _Context.SaveChanges();

                            // seller email
                            var seller = _Context.Users.FirstOrDefault(m => m.UserId == note.User_Id);

                            // send mail to seller
                            string subject = user.First_Name + " wants to purchase your notes";
                            string body = "Hello " + seller.First_Name + "\\n"
                                + "We would like to inform you that, " + user.First_Name + " wants to purchase your notes. Please see Buyer Requests tab and allow download access to Buyer if you have received the payment from him";
                            body += "\\nRegards,\\nNotes MarketPlace";

                            bool isSend = SendEmail.EmailSend(seller.Email, subject, body, false);


                            TempData["UserName"] = user.First_Name;

                            // show modal
                            TempData["ShowModal"] = 1;
                            return RedirectToAction("Note_Details", new { id = noteId });
                        }

                    }

                else
                {
                    return View("Search_Notes");
                }
            }

            
        }


        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if(br != fs.Length)
            {
                throw new System.IO.IOException(s);
            }
            return data;
        }



       // get sell notes
        [Route("User/Sell_Note")]
        public ActionResult Sell_Note(int? edit)
        {

            using(var _Context = new ApplicationContext())
            {
                // get all type
                var type = _Context.Type_Details.Where(x=> x.IsActive == true).ToList();
                // get all category
                var category = _Context.Category_Details.Where(x => x.IsActive == true).ToList();
                // get all country
                var country = _Context.Country_Details.Where(x => x.IsActive == true).ToList();

                ViewBag.TypeList = type;
                ViewBag.CategotyList = category;
                ViewBag.CountryList = country;
                ViewBag.Edit = false;

                // for edit details
                if (!edit.Equals(null))
                {

                    var note = (from Notes in _Context.Note_Details
                                join Attachment in _Context.NotesAttachments on Notes.Id equals Attachment.NoteId
                                where Notes.Id == edit && Notes.Status == 3
                                select new AddNoteModel
                                {
                                    ID = Notes.Id,
                                    Title = Notes.Title,
                                    Category = Notes.Category_Id,
                                    UploadNotes = Attachment.FileName,
                                    Type = Notes.Type_Id,
                                    Pages = (int)Notes.Pages,
                                    Country = Notes.Category_Id,
                                    Institute = Notes.University,
                                    Course = Notes.Course,
                                    CourseCode = Notes.Course_Code,
                                    Professor = Notes.Professor,
                                    Description = Notes.Description,
                                    SellType = Notes.Price == 0 ? "Free" : "Paid",
                                    Price = Notes.Price,
                                    NotePreview = Notes.Note_Preview
                                }).FirstOrDefault<AddNoteModel>();

                    ViewBag.Edit = true;
                    return View(note);

                }

                return View();
                

            }

        }

        
        // save notes to draft
        [HttpPost]
        [Route("User/Save")]
        [ValidateAntiForgeryToken]
        public ActionResult Save(int? id, AddNoteModel note)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Sell_Note");
            }

            using(var _Context = new ApplicationContext())
            {
                // current user
                var currentUser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name).UserId;

                //default book image
                string DefaultImg = _Context.System_Config.FirstOrDefault(m => m.Name == "DefaultBookImage").Value;

                // edit draft details
                if (!id.Equals(null))
                {
                    var note_details = _Context.Note_Details.SingleOrDefault(m => m.Id == id && m.Status == 3);
                    var Attachment = _Context.NotesAttachments.SingleOrDefault(m => m.NoteId == id);

                    note.MaptoModel(note_details, Attachment);

                    // note image
                    if(note.DisplayPicture == null)
                    {
                        note_details.Image = note_details.Image;
                    }
                    else
                    {
                        note_details.Image = "../Members/"+currentUser+"/"+id+"/"+note.DisplayPicture;
                    }

                    // preview
                    if (note.NotePreview == null)
                    {
                        note_details.Note_Preview = note_details.Note_Preview;
                    }
                    else
                    {
                        note_details.Note_Preview = "../Members/" + currentUser + "/" + id + "/" + note.NotePreview;
                    }

                    // attachment
                    if (note.UploadNotes == null)
                    {
                        Attachment.FileName = Attachment.FileName;
                    }
                    else
                    {
                        Attachment.FileName = note.UploadNotes;
                        Attachment.FilePath = "../Members/" + currentUser + "/" + id + "/Attachment/";
                    }


                    note_details.Edited_Date = DateTime.Now;
                    Attachment.Modified_Date = DateTime.Now;
                    _Context.SaveChanges();


                    return RedirectToAction("Dashboard");
                }
                // create note as draft
                else
                {
                    var note_details = _Context.Note_Details;
                    note_details.Add(new Note_Details
                    {
                        Title = note.Title,
                        Category_Id = note.Category,
                        Image = note.DisplayPicture,
                        Type_Id = note.Type,
                        Pages = note.Pages,
                        Description = note.Description,
                        University = note.Institute,
                        Country_Id = note.Country,
                        Course = note.Course,
                        Course_Code = note.CourseCode,
                        Professor = note.Professor,
                        Price = note.SellType == "Free" ? 0 : note.Price,
                        Note_Preview = note.NotePreview,
                        Status = 3,
                        Added_Date = DateTime.Now,
                        User_Id = currentUser,
                        IsActive = true
                    });

                    _Context.SaveChanges();

                    var createdNote = note_details.FirstOrDefault(m => m.User_Id == currentUser && m.Title == note.Title);

                    // set image
                    if(createdNote.Image == null)
                    {
                        createdNote.Image = DefaultImg;
                    }
                    else
                    {
                        createdNote.Image = "../Members/" + currentUser + "/" + createdNote.Id + "/" + createdNote.Image;
                    }

                    // set preview
                    if (createdNote.Image != null)
                    {
                        createdNote.Note_Preview = "../Members/" + currentUser + "/" + createdNote.Id + "/" + createdNote.Note_Preview;
                    }


                    // create folder
                    string path = CreateDirectory(currentUser, createdNote.Id);

                    var attachments = _Context.NotesAttachments;
                    attachments.Add(new NotesAttachment
                    {
                        NoteId = createdNote.Id,
                        FileName = note.UploadNotes,
                        FilePath = path,
                        Create_Date = DateTime.Now,
                        IsActive = true
                    });
                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard", "User");

                }

            }

        }



        // save note for publish
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/Publish")]
        public ActionResult Publish(int? id, AddNoteModel note)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Sell_Note");
            }

            using (var _Context = new ApplicationContext())
            {
                
                // current user
                var currentUser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name);

                //default book image
                string DefaultImg = _Context.System_Config.FirstOrDefault(m => m.Name == "DefaultBookImage").Value;


                // publish draft note
                if (!id.Equals(null))
                {
                    var draftNote = _Context.Note_Details.FirstOrDefault(m => m.Id == id && m.Status == 3);
                    var draftAttachent = _Context.NotesAttachments.FirstOrDefault(m => m.NoteId == id);
                    note.MaptoModel(draftNote, draftAttachent);

                    // note image
                    if (note.DisplayPicture == null)
                    {
                        draftNote.Image = draftNote.Image;
                    }
                    else
                    {
                        draftNote.Image = "../Members/" + currentUser.UserId + "/" + id + "/" + note.DisplayPicture;
                    }

                    // preview
                    if (note.NotePreview == null)
                    {
                        draftNote.Note_Preview = draftNote.Note_Preview;
                    }
                    else
                    {
                        draftNote.Note_Preview = "../Members/" + currentUser.UserId + "/" + id + "/" + note.NotePreview;
                    }

                    // attachment
                    if (note.UploadNotes == null)
                    {
                        draftAttachent.FileName = draftAttachent.FileName;
                    }
                    else
                    {
                        draftAttachent.FileName = note.UploadNotes;
                        draftAttachent.FilePath = "../Members/" + currentUser.UserId + "/" + id + "/Attachment/";
                    }

                    draftNote.Status = 4;
                    draftNote.Edited_Date = DateTime.Now;
                    draftAttachent.Modified_Date = DateTime.Now;

                    _Context.SaveChanges();


                    // email addressed
                    var emails = _Context.System_Config.Where(m => m.Name == "EmailAddresses").First().Value;


                    // send mail to admins
                    string subject = currentUser.First_Name+" "+currentUser.Last_Name+ " sent his note for review";
                    string body = "Hello Admins, \\n"
                        + "We want to inform you that, "+ currentUser.First_Name + " " + currentUser.Last_Name + " sent his note"
                        + note.Title +" for review.Please look at the notes and take required actions.";
                    body += "\\nRegards,\\nNotes Marketplace";

                    bool isSend = SendEmail.EmailSend(emails, subject, body, false);


                    return RedirectToAction("Dashboard","User");
                }
                // create note as publish
                else
                {

                    var note_details = _Context.Note_Details;
                    note_details.Add(new Note_Details
                    {
                        Title = note.Title,
                        Category_Id = note.Category,
                        Image = note.DisplayPicture,
                        Type_Id = note.Type,
                        Pages = note.Pages,
                        Description = note.Description,
                        University = note.Institute,
                        Country_Id = note.Country,
                        Course = note.Course,
                        Course_Code = note.CourseCode,
                        Professor = note.Professor,
                        Price = note.SellType == "Free" ? 0 : note.Price,
                        Note_Preview = note.NotePreview,
                        Status = 4,
                        Added_Date = DateTime.Now,
                        User_Id = currentUser.UserId,
                        IsActive = true
                    });

                    _Context.SaveChanges();

                    var createdNote = note_details.FirstOrDefault(m => m.User_Id == currentUser.UserId && m.Title == note.Title);

                    // set image
                    if (createdNote.Image == null)
                    {
                        createdNote.Image = DefaultImg;
                    }
                    else
                    {
                        createdNote.Image = "../Members/" + currentUser.UserId + "/" + createdNote.Id + "/Attachment/" + createdNote.Image;
                    }

                    // set preview
                    if(createdNote.Note_Preview != null)
                    {
                        createdNote.Note_Preview = "../Members/" + currentUser.UserId + "/" + createdNote.Id + createdNote.Note_Preview;
                    }


                    string path = CreateDirectory(currentUser.UserId, createdNote.Id);

                    var attachments = _Context.NotesAttachments;
                    attachments.Add(new NotesAttachment
                    {
                        NoteId = createdNote.Id,
                        FileName = note.UploadNotes,
                        FilePath = path,
                        Create_Date = DateTime.Now,
                        IsActive = true
                    });
                    _Context.SaveChanges();


                    // email addressed
                    var emails = _Context.System_Config.Where(m => m.Name == "EmailAddresses").First().Value;


                    // send mail to admins
                    string subject = currentUser.First_Name + " " + currentUser.Last_Name + " sent his note for review";
                    string body = "Hello Admins, \\n"
                        + "We want to inform you that, " + currentUser.First_Name + " " + currentUser.Last_Name + " sent his note"
                        + note.Title + " for review.Please look at the notes and take required actions.";
                    body += "\\nRegards,\\nNotes Marketplace";

                    bool isSend = SendEmail.EmailSend(emails, subject, body, false);


                    return RedirectToAction("Dashboard", "User");
                }

            }



        }



        // delete note details
        [HttpPost]
        [Route("User/Note/delete")]
        public string Delete(int id)
        {
            using(var _Context = new ApplicationContext())
            {
                var currentuser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name).UserId;

                var note = _Context.Note_Details.SingleOrDefault(m => m.Id == id && m.Status == 3 && m.User_Id == currentuser);
                var attachment = _Context.NotesAttachments.SingleOrDefault(m => m.NoteId == id);

                _Context.NotesAttachments.Remove(attachment);
                _Context.Note_Details.Remove(note);
                _Context.SaveChanges();

            }

            return "Dashboard";
        }


        public string CreateDirectory(int userid, int noteid)
        {

            string path = @"C:\Users\Lenovo\Desktop\projects\NoteMarketPlace\NoteMarketPlace\Members\" + userid + "\\" + noteid + "\\Attachment";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return path;
            }
            else
            {
                return null;
            }
        }



        // get buyer requests
        [Route("User/Buyer_Requests")]
        public ActionResult Buyer_Requests()
        {
            // current login user email
            string userEmail = User.Identity.Name;

            using(var _Context = new ApplicationContext())
            {
                var result = (from Purchase in _Context.Purchase_Details
                              join Note in _Context.Note_Details on Purchase.Note_Id equals Note.Id
                              join Downloader in _Context.Users on Purchase.Downloader equals Downloader.UserId
                              join Seller in _Context.Users on Purchase.Seller equals Seller.UserId
                              join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                              where Purchase.Allow_Download == false && Seller.Email == userEmail
                              select new BuyerRequestModel
                              {
                                  NoteId = Purchase.Note_Id,
                                  PurchaseId = Purchase.Purchase_Id,
                                  Title = Note.Title,
                                  Category = Category.Category_Name,
                                  Buyer = Downloader.Email,
                                  Selltype = Purchase.PurchasedPrice == 0 ? "Free" : "Paid",
                                  Price = Purchase.PurchasedPrice,
                                  RequestDate = Purchase.Purchase_Date
                              }).OrderByDescending(m => m.RequestDate).ToList();
                
                return View(result);

            }


        }



        // allow buyer to download note
        [HttpPost]
        [Route("User/AllowDownload")]
        public HttpStatusCodeResult AllowDownload(int id)
        {
            if(id.Equals(null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using(var _Context = new ApplicationContext())
            {
                // search purchase details by id
                var result = _Context.Purchase_Details.FirstOrDefault(m => m.Purchase_Id == id);
                var seller = _Context.Users.FirstOrDefault(m => m.UserId == result.Seller);
                var downloader = _Context.Users.FirstOrDefault(m => m.UserId == result.Downloader);

                // if result not available
                if (result != null)
                {
                    // set allowDownload true
                    result.Allow_Download = true;
                    _Context.SaveChanges();

                    // send mail to buyer
                    string subject = seller.First_Name + " Allows you to download a note";
                    string body = "Hello " + downloader.First_Name + "\\n"
                        + "We would like to inform you that, "+ seller.First_Name +" Allows you to download a note. Please login and see My Download tabs to download particular note.";
                    body += "\\nRegards,\\nNotes MarketPlace";

                    bool isSend = SendEmail.EmailSend(downloader.Email, subject, body, false);

                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

            }

        }



        // get my Profile
        [Route("User/MyProfile")]
        public ActionResult MyProfile()
        {

            using (var _Context = new ApplicationContext())
            {
                // get gender for dropdown
                var gender = _Context.Status.Where(m => m.RefCategory == "Gender").ToList();
                // get country
                var country = _Context.Country_Details.ToList();


                // get current userId
                var currentuser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name);

                // get user details
                var isDetailsAvailable = _Context.User_Details.FirstOrDefault(m => m.UserId == currentuser.UserId);


                var UserProfile = new UserProfileModel();

                // check user details available or not
                if (isDetailsAvailable != null)
                {
                    UserProfile = (from Detail in _Context.User_Details
                                   join User in _Context.Users on Detail.UserId equals User.UserId
                                   join Country in _Context.Country_Details on Detail.Country_Id equals Country.Country_Id
                                   where Detail.UserId == currentuser.UserId
                                   select new UserProfileModel
                                   {
                                       FirstName = User.First_Name,
                                       LastName = User.Last_Name,
                                       Email = User.Email,
                                       Gender = Detail.Gender,
                                       DOB = Detail.DOB,
                                       CountryCode = Detail.Phone_No_Country_Code,
                                       Phone = Detail.Phone_No,
                                       ProfilePicture = Detail.Profile_Img,
                                       Address1 = Detail.Address_Line1,
                                       Address2 = Detail.Address_Line2,
                                       City = Detail.City,
                                       State = Detail.State,
                                       Zipcode = Detail.ZipCode,
                                       Country = Detail.Country_Id,
                                       University = Detail.University,
                                       College = Detail.College
                                   }).FirstOrDefault<UserProfileModel>();

                    UserProfile.ProfilePicture = "DP.png";

                    UserProfile.genderModel = gender.Select(x => new GenderModel { Gender_Id = x.Id, Gender_Val = x.Value }).ToList();
                    UserProfile.countryModel = country.Select(x => new CountryModel { Country_Id = x.Country_Id, Country_Val = x.Country_Name }).ToList();
                    UserProfile.CountryCodeModel = country.Select(x => new CountryModel { Country_Code = x.Country_Code }).ToList();

                    return View(UserProfile);
                }
                // if user is first time login
                else
                {
                    UserProfile.FirstName = currentuser.First_Name;
                    UserProfile.LastName = currentuser.Last_Name;
                    UserProfile.Email = currentuser.Email;
                    UserProfile.genderModel = gender.Select(x => new GenderModel { Gender_Id = x.Id, Gender_Val = x.Value }).ToList();
                    UserProfile.countryModel = country.Select(x => new CountryModel { Country_Id = x.Country_Id, Country_Val = x.Country_Name }).ToList();
                    UserProfile.CountryCodeModel = country.Select(x => new CountryModel { Country_Code = x.Country_Code }).ToList();

                    return View(UserProfile);
                }

            }


        }


        // Update my Profile
        [HttpPost]
        [Route("User/MyProfile")]
        public ActionResult MyProfile(UserProfileModel user)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction("MyProfile");
            }

            using(var _Context = new ApplicationContext())
            {
                // get current userId
                int currentuser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name).UserId;

                // get user details
                var isDetailsAvailable = _Context.User_Details.FirstOrDefault(m => m.UserId == currentuser);

                // check user details available or not
                if(isDetailsAvailable != null && user != null)
                {
                    // update details
                    var userUpdate = _Context.Users.FirstOrDefault(m => m.UserId == currentuser);
                    var detailsUpdate = _Context.User_Details.FirstOrDefault(m=> m.UserId == currentuser);

                    user.MaptoModel(userUpdate, detailsUpdate);
                    if(user.ProfilePicture == null)
                    {
                        detailsUpdate.Profile_Img = detailsUpdate.Profile_Img;
                    }
                    else
                    {
                        detailsUpdate.Profile_Img = "../Members/"+currentuser+"/"+user.ProfilePicture;
                    }
                    userUpdate.Modified_Date = DateTime.Now;
                    detailsUpdate.Modified_date = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("MyProfile");
                }
                else
                {
                    // create new details
                    var create = _Context.User_Details;
                    create.Add(new User_Details
                    {
                        UserId = currentuser,
                        DOB = user.DOB,
                        Gender = user.Gender,
                        Phone_No_Country_Code = user.CountryCode,
                        Phone_No = user.Phone,
                        Profile_Img = "../Members/"+currentuser+"/"+user.ProfilePicture,
                        Address_Line1 = user.Address1,
                        Address_Line2 = user.Address2,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.Zipcode,
                        Country_Id = (user.Country),
                        University = user.University,
                        College = user.College,
                        Create_Date = DateTime.Now
                    });

                    _Context.SaveChanges();

                    CreateDirectory(create.FirstOrDefault(m => m.UserId == currentuser).UserId);
                    return RedirectToAction("MyProfile");

                }
                
            }

        }


        public string CreateDirectory(int userid)
        {

            string path = @"C:\Users\Lenovo\Desktop\projects\NoteMarketPlace\NoteMarketPlace\Members\" + userid;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return path;
            }
            else
            {
                return null;
            }
        }


        // get my downloads
        [Route("User/MyDownloads")]
        public ActionResult MyDownloads()
        {
            using (var _Context = new ApplicationContext())
            {
                // current login userId
                int currentUser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name).UserId;

                var result = (from Purchase in _Context.Purchase_Details
                              join Note in _Context.Note_Details on Purchase.Note_Id equals Note.Id
                              join Downloader in _Context.Users on Purchase.Downloader equals Downloader.UserId
                              join Seller in _Context.Users on Purchase.Seller equals Seller.UserId
                              join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                              where Purchase.Allow_Download == true && Purchase.Downloader == currentUser
                              select new MyDownloads
                              {
                                  NoteId = Purchase.Note_Id,
                                  PurchaseId = Purchase.Purchase_Id,
                                  Title = Note.Title,
                                  Category = Category.Category_Name,
                                  Buyer = Downloader.Email,
                                  Price = Purchase.PurchasedPrice,
                                  SellType = Purchase.PurchasedPrice == 0 ? "Free" : "Paid",
                                  DownloadDate = Purchase.AttachmentDownload_Date
                              }).ToList();

                return View(result);
            }
        }


        // user review
        [HttpPost]
        [Route("User/UserReview")]
        public ActionResult UserReview(int purchaseId, int review_rating, string review_comment)
        {
            using(var _Context = new ApplicationContext())
            {
                var currentuser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name).UserId;

                var purchase = _Context.Purchase_Details.FirstOrDefault(m => m.Purchase_Id == purchaseId && m.Downloader == currentuser );

                var review = _Context.Review_Details;
                review.Add(new Review_Details
                {
                    Note_Id = purchase.Note_Id,
                    User_Id = currentuser,
                    Purchase_Id = purchaseId,
                    Review_msg = review_comment,
                    Rating = review_rating,
                    Review_Date = DateTime.Now
                });

                _Context.SaveChanges();

                return RedirectToAction("MyDownloads");
            }
        }


        // user report spam
        [HttpPost]
        [Route("User/UserReport")]
        public ActionResult UserReport(int Id, string UserRemarks)
        {
            using (var _Context = new ApplicationContext())
            {
                var currentuser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name);

                var purchase = _Context.Purchase_Details.FirstOrDefault(m => m.Purchase_Id == Id && m.Downloader == currentuser.UserId);

                var note = _Context.Note_Details.First(m => m.Id == purchase.Note_Id);

                var seller = _Context.Users.First(m => m.UserId == note.User_Id);

                var review = _Context.Spam_Notes;
                review.Add(new Spam_Notes
                {
                    NoteId = purchase.Note_Id,
                    UserId = currentuser.UserId,
                    Purchase_Id = Id,
                    Remarks = UserRemarks,
                    Reported_Date = DateTime.Now
                });

                _Context.SaveChanges();

                // email addresse(es) for event
                var Adminemails = _Context.System_Config.Where(m => m.Name == "EmailAddresses").First().Value;

                // send mail to admin
                string subject = currentuser.First_Name + " " + currentuser.Last_Name + "  Reported an issue for "+ note.Title  ;
                string body = "Hello Admins, \\n" 
                    + "We want to inform you that, "+ currentuser.First_Name + " " + currentuser.Last_Name + " Reported an issue for "+ seller.First_Name+" "+ seller.Last_Name  +"’s Note with"
                    + "title "+ note.Title +".Please look at the notes and take required actions.";
                body += "\\nRegards,\\nNotes Marketplace";

                bool isSend = SendEmail.EmailSend(Adminemails, subject, body, false);


                return RedirectToAction("MyDownloads");
            }
        }


        // get my sold notes
        [Route("User/MySoldNotes")]
        public ActionResult MySoldNotes()
        {
            using (var _Context = new ApplicationContext())
            {
                // current login userId
                int currentUser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name).UserId;

                var result = (from Purchase in _Context.Purchase_Details
                              join Note in _Context.Note_Details on Purchase.Note_Id equals Note.Id
                              join Downloader in _Context.Users on Purchase.Downloader equals Downloader.UserId
                              join Seller in _Context.Users on Purchase.Seller equals Seller.UserId
                              join Category in _Context.Category_Details on Note.Category_Id equals Category.Category_Id
                              where Purchase.Allow_Download == true && Purchase.Seller == currentUser
                              select new MySoldNotesModel
                              {
                                  Id = Purchase.Purchase_Id,
                                  NoteId = Purchase.Note_Id,
                                  Title = Note.Title,
                                  Category = Category.Category_Name,
                                  Buyer = Downloader.Email,
                                  SellType = Purchase.PurchasedPrice == 0 ? "Free":"Paid",
                                  Price = Purchase.PurchasedPrice,
                                  DownloadDate = Purchase.Purchase_Date
                              }).ToList();

                return View(result);

            }

        }


        // get my rejected notes
        [Route("User/MyRejectedNotes")]
        public ActionResult MyRejectedNotes()
        {
            using (var _Context = new ApplicationContext())
            {
                // current login userId
                int currentUser = _Context.Users.FirstOrDefault(m => m.Email == User.Identity.Name).UserId;

                var result = (from Notes in _Context.Note_Details
                              join Status in _Context.Status on Notes.Status equals Status.Id
                              join Category in _Context.Category_Details on Notes.Category_Id equals Category.Category_Id
                              join Attachment in _Context.NotesAttachments on Notes.Id equals Attachment.NoteId
                              where Status.RefCategory == "Notes Status" && Status.Value == "Rejected" && Notes.User_Id == currentUser
                              select new MyRejectedNotes
                              {
                                  Id = Notes.Id,
                                  Title = Notes.Title,
                                  Category = Category.Category_Name,
                                  Remark = Notes.AdminReview,
                                  DownloadNote = Attachment.FilePath
                              }).ToList();

                return View(result);
            }

                
        }


        // clone rejected note
        [Route("User/CloneNote")]
        public ActionResult CloneNote(int noteId)
        {
            using(var _Context = new ApplicationContext())
            {
                var currentuser = _Context.Users.SingleOrDefault(m => m.Email == User.Identity.Name).UserId;

                var oldnote = (from Note in _Context.Note_Details
                               join Attachment in _Context.NotesAttachments on Note.Id equals Attachment.NoteId
                               where Note.Id == noteId && Note.Status == 7 && Note.User_Id == currentuser
                               select new { Note, Attachment }).SingleOrDefault();

                // old note status set to remove
                oldnote.Note.Status = 8;
                oldnote.Note.IsActive = false;
                oldnote.Attachment.IsActive = false;
                oldnote.Note.Edited_Date = DateTime.Now;
                oldnote.Attachment.Modified_Date = DateTime.Now;


                var clonenote = _Context.Note_Details;
                clonenote.Add(new Note_Details {
                    Title = oldnote.Note.Title,
                    Category_Id = oldnote.Note.Category_Id,
                    Image = oldnote.Note.Image,
                    Type_Id = oldnote.Note.Type_Id,
                    Pages = oldnote.Note.Pages,
                    Description = oldnote.Note.Description,
                    University = oldnote.Note.University,
                    Country_Id = oldnote.Note.Country_Id,
                    Course = oldnote.Note.Course,
                    Course_Code = oldnote.Note.Course_Code,
                    Professor = oldnote.Note.Professor,
                    Price = oldnote.Note.Price,
                    Note_Preview = oldnote.Note.Note_Preview,
                    Status = 3,
                    Added_Date = DateTime.Now,
                    User_Id = currentuser,
                    IsActive = true
                });

                _Context.SaveChanges();

                // get created note Id
                var newnote = _Context.Note_Details.FirstOrDefault(m => m.Status == 3 && m.Title == oldnote.Note.Title && m.User_Id == currentuser);

                //set preview
                if(oldnote.Note.Note_Preview != null)
                {
                    newnote.Note_Preview = "../Members/"+ currentuser + "/" + newnote.Id + "/" + oldnote.Note.Note_Preview ;
                }


                string path = CreateDirectory(currentuser, newnote.Id);

                // create attachments
                var cloneattachment = _Context.NotesAttachments;
                cloneattachment.Add(new NotesAttachment {
                    NoteId = newnote.Id,
                    FileName = oldnote.Attachment.FileName,
                    FilePath = path,
                    Create_Date = DateTime.Now,
                    IsActive = true
                });

                _Context.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }



        // download note over browser
        [Route("User/DownloadNote")]
        public FileResult DownloadFile(int? purchaseId,int? noteId)
        {
            // seller download rejected note
            if (!noteId.Equals(null))
            {
                using(var _Context = new ApplicationContext())
                {
                    int currentuser = _Context.Users.SingleOrDefault(m => m.Email == User.Identity.Name).UserId;
                    var note = (from Note in _Context.Note_Details
                                join Attachment in _Context.NotesAttachments on Note.Id equals Attachment.NoteId
                                where Note.Id == noteId && Note.Status == 7 && Note.User_Id == currentuser
                                select new {Attachment.FileName }).SingleOrDefault();

                    string file = Server.MapPath("../Members/" + currentuser + "/" + noteId + "/Attachment/" + note.FileName);
                    byte[] filebyte = GetFile(file);
                    return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, note.FileName);

                }

            }
            // seller download sell notes  or downloader download purchased note
            else
            {
                using (var _Context = new ApplicationContext())
                {
                    int currentuser = _Context.Users.SingleOrDefault(m => m.Email == User.Identity.Name).UserId;
                    var note = (from Purchase in _Context.Purchase_Details
                                join Attachment in _Context.NotesAttachments on Purchase.Note_Id equals Attachment.NoteId
                                where Purchase.Purchase_Id == purchaseId &&
                                (Purchase.Seller == currentuser || (Purchase.Downloader == currentuser && Purchase.Allow_Download == true))
                                select new { Attachment.NoteId, Attachment.FilePath, Attachment.FileName, Purchase.Downloader, Purchase.Seller }).SingleOrDefault();

                    if (note != null)
                    {

                        string file = Server.MapPath("../Members/" + note.Seller + "/" + note.NoteId + "/Attachment/" + note.FileName);

                        if (currentuser == note.Downloader)
                        {
                            var update = _Context.Purchase_Details.FirstOrDefault(m => m.Purchase_Id == purchaseId && m.Downloader == currentuser);
                            update.IsAttachment_Downloaded = true;
                            update.AttachmentDownload_Date = DateTime.Now;
                            _Context.SaveChanges();
                        }

                        // download file direct
                        byte[] filebyte = GetFile(file);
                        return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, note.FileName);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }



        // logout
        [Route("User/LogOut")]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }


    }
}