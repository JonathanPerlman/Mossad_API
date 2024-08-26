using System;
using System.ComponentModel.DataAnnotations;
using Mossad_API.Moddels.Enums;

namespace Mossad_API.Moddels.DBModdels
{
    public class Mission
    {
        [Key]
        public int Id { get; set; }
        public Agent _agent { get; set; }
        public Target _target { get; set; }
        public double? TimeLeft { get; set; }
        public double? ActualExecutionTime { get; set; }
        public MissionStatus? Status { get; set; }
    }
}

