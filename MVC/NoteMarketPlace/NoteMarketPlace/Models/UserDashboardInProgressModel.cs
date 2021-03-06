using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models
{
    public class UserDashboardInProgressModel
    {

        public int Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public DateTime AddedDate { get; set; }

    }

    public class UserDashboardPublishedNoteModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string SellType { get; set; }

        public decimal Price { get; set; }

        public DateTime AddedDate { get; set; }
    }


}