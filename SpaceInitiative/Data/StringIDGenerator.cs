using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public interface IStringIDGenerator
    {
        string GenerateIDString(int size = 5);
    }
    public class StringIDGenerator : IStringIDGenerator
    {
        private Random _random = null;

        private List<string> _validOptions = new List<string>()
        {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
        };

        public string GenerateIDString(int size = 5)
        {
            if (_random == null)
            {
                _random = new Random();
            }

            StringBuilder sb = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                sb.Append(_validOptions[_random.Next(0, _validOptions.Count)]);
            }

            return sb.ToString();
        }
    }
}
