using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public class Stats
    {
        [Key]
        public int ID { get; set; }

        public int TotalEncountersCreated { get; set; }
    }
}
