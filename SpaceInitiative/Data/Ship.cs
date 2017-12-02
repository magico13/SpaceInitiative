using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public class Ship
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int BonusBase { get; set; }

        private int? _bonusCurrent = null;
        public int BonusCurrent
        {
            get { return _bonusCurrent.HasValue ? _bonusCurrent.Value : BonusBase; }
            set
            {
                _bonusCurrent = value;
                if (value == BonusBase)
                {
                    _bonusCurrent = null;
                }
            }
        }

        public int Roll { get; set; }
    }
}
