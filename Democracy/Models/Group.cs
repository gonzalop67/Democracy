using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage =
            "The field {0} can contain maximum {1} and minimum {2} characters",
            MinimumLength = 3)]
        public string Description { get; set; }

        public virtual ICollection<GroupMember> GroupMembers { get; set; }

        public virtual ICollection<VotingGroup> VotingGroups { get; set; }
    }
}