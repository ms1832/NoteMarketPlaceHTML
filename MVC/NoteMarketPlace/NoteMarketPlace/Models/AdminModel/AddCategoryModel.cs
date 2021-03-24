using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models.AdminModel
{
    public class AddCategoryModel
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }


        public void MaptoModel(Category_Details category)
        {
            category.Category_Name = Name;
            category.Description = Description;
        }


    }
}