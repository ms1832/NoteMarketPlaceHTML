using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models.AdminModel
{
    public class SystemConfig
    {

        [Required(ErrorMessage = "Required field")]
        [EmailAddress]
        public string SupportEmail { get; set; }

        [Required(ErrorMessage = "Required field")]
        public string Contact { get; set; }

        public string Emails { get; set; }

        public string FacebookUrl { get; set; }

        public string TwitterUrl { get; set; }

        public string LinkedinUrl { get; set; }

        [Required(ErrorMessage = "Required field")]
        public string DefaulDpImg { get; set; }

        [Required(ErrorMessage ="Required field")]
        public string DefaultNoteImg { get; set; }

    }
}