using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Exam_attempt1.Models
{
    public class GroupActivity{

        [Key]
        public int GroupActivityId {get;set;}

        [Required]
        public string Title {get;set;}

        [Required]
        public string Description {get;set;}

        [DataType(DataType.Date)]
        public DateTime Date { get; set;}

        [DataType(DataType.Time)]
        public DateTime Time { get; set;}

        public int DurationInt { get; set;}

        public string DurationVal { get; set;}
        public string Duration
        {
            get { return DurationInt + " " + DurationVal; }
        }

        [Required]
        public string Coordinator { get; set; }
        
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        public List<Participant> inGroup { get; set; }
    }
}