using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models.AdminModel
{
    public class ManageType
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public string Desctiption { get; set; }

        public string AddedBy { get; set; }

        public DateTime AddedDate { get; set; }

        public string Active { get; set; }

    }
}