using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models.AdminModel
{
    public class AddTypeModel
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }


        public void MaptoModel(Type_Details type)
        {
            type.Type_Name = Name;
            type.Description = Description;
        }


    }
}