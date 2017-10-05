using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class Voting
    {
        [Key]
        public int VotingId { get; set; }
        
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage =
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength = 3)]
        [Display(Name = "Voting description")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name="State")]
        public int StateId { get; set; }
        
        [DataType(DataType.MultilineText)]
        public string Remarks { get; set; }
        
        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Date time start")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString="{0:dd/MM/yyyy hh:mm}", ApplyFormatInEditMode=true)]
        public DateTime DateTimeStart { get; set; }
        
        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Date time end")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime DateTimeEnd { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Is for all users?")]
        public bool IsForAllUsers { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Is enabled blank votes?")]
        public bool IsEnabledBlankVotes { get; set; }

        [Display(Name = "Quantity votes")]
        public int QuantityVotes { get; set; }

        [Display(Name = "Winner")]
        public int QuantityBlankVotes { get; set; }
        
        public int CandidateWinId { get; set; }
    }
}