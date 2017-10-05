using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class State
    {
        [Key]
        public int StateId { get; set; }

        [Required(ErrorMessage="The field {0} is required")]
        [StringLength(50,ErrorMessage=
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength=3)]
        [Display(Name = "State description")]
        public string Description { get; set; }

        public virtual ICollection<Voting> Votings { get; set; }
    }
}