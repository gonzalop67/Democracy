using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class UserView
    {

        public int UserId { get; set; }

        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(100, ErrorMessage =
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength = 7)]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Display(Name = "First name")]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage =
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength = 2)]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage =
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength = 2)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(20, ErrorMessage =
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength = 7)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(100, ErrorMessage =
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength = 10)]
        public string Address { get; set; }

        public string Grade { get; set; }

        public string Group { get; set; }

        public HttpPostedFileBase Photo { get; set; }

    }
}