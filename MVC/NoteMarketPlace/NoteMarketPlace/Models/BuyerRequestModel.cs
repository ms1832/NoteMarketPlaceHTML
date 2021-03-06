using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models
{
    public class BuyerRequestModel
    {

        public int NoteId { get; set; }

        public int PurchaseId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Buyer { get; set; }

        public string CountryCode { get; set; }

        public long Phone { get; set; }

        public string Selltype { get; set; }

        public decimal Price { get; set; }
        
        public DateTime RequestDate { get; set; }


    }
}