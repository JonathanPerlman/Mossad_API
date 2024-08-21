using System;
using System.ComponentModel.DataAnnotations;

namespace Mossad_API.Moddels
{
    public class Mission
    {
        [Key]
        public int Id { get; set; }   
        public Agent _agent { get; set; }   
        public Target _target { get; set; } 
        public Double TimeLeft { get; set; }
        public Double ActualExecutionTime { get; set; }  
        public MissionStatus? Status { get; set; }  
    }
}

