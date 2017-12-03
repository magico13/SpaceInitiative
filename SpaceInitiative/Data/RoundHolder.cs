using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public enum ROUND_STEP
    {
        ENGINEERING,
        HELM,
        GUNNERY
    }

    public class RoundHolder
    {
        [Key]
        public int ID { get; set; }
        public int Round { get; set; }
        public ROUND_STEP Step { get; set; }

        public void CopyFrom(RoundHolder round)
        {
            Round = round.Round;
            Step = round.Step;
        }
    }
}
