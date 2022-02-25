using System;
using System.ComponentModel.DataAnnotations;

namespace Exam_attempt1.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantId {get;set;}
        public int UserId {get;set;}
        public int GroupActivityId {get;set;}
    }
}