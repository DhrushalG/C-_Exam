using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Exam_attempt1.Models
{
    public class User{
        [Key]
        public int UserId {get;set;}

        [Required]
        [MinLength(2)]
        public string Name {get;set;}
        [Required]
        [EmailAddress]
        public string Email {get;set;}
        
        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPass {get;set;}
        public List<Participant> isParticipating { get; set; }
    }
}