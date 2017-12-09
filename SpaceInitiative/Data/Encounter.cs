using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public class Encounter
    {
        [Key]
        public int EncounterID { get; set; }

        [Required]
        public string EncounterStringID { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
