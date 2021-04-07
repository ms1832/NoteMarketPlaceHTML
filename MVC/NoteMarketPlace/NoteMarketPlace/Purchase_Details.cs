//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoteMarketPlace
{
    using System;
    using System.Collections.Generic;
    
    public partial class Purchase_Details
    {
        public Purchase_Details()
        {
            this.Review_Details = new HashSet<Review_Details>();
            this.Spam_Notes = new HashSet<Spam_Notes>();
        }
    
        public int Purchase_Id { get; set; }
        public int Downloader { get; set; }
        public int Note_Id { get; set; }
        public int Seller { get; set; }
        public System.DateTime Purchase_Date { get; set; }
        public bool Allow_Download { get; set; }
        public bool IsAttachment_Downloaded { get; set; }
        public Nullable<System.DateTime> AttachmentDownload_Date { get; set; }
        public decimal PurchasedPrice { get; set; }
        public Nullable<int> Modified_By { get; set; }
        public Nullable<System.DateTime> Modified_Date { get; set; }
    
        public virtual Note_Details Note_Details { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public virtual User User2 { get; set; }
        public virtual ICollection<Review_Details> Review_Details { get; set; }
        public virtual ICollection<Spam_Notes> Spam_Notes { get; set; }
    }
}