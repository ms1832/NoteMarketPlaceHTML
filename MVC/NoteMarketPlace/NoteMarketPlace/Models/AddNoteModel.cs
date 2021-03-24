using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models
{
    public class AddNoteModel
    {

        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int Category { get; set; }

        public string DisplayPicture { get; set; }

        [Required]
        public string UploadNotes { get; set; }

        [Required]
        public int Type { get; set; }

        public Nullable<int> Pages { get; set; }

        public Nullable<int> Country { get; set; }

        public string Institute { get; set; }

        public string Course { get; set; }

        public string CourseCode { get; set; }

        public string Professor { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string SellType { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string NotePreview { get; set; }



        public void MaptoModel(Note_Details note, NotesAttachment attachment)
        {
            note.Title = Title;
            note.Category_Id = Category;
            //note.Image = DisplayPicture;
            note.Type_Id = Type;
            note.Pages = Pages;
            note.Description = Description;
            note.University = Institute;
            note.Country_Id = Country;
            note.Course = Course;
            note.Course_Code = CourseCode;
            note.Professor = Professor;
            note.Price = Price;
            //note.Note_Preview = NotePreview;
            //attachment.FileName = UploadNotes;
        }


    }
}