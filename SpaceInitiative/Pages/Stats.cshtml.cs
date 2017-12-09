using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaceInitiative.Data;

namespace SpaceInitiative.Pages
{
    public class StatsModel : PageModel
    {
        private readonly AppDbContext _db;

        public StatsModel(AppDbContext db)
        {
            _db = db;
        }


        public Stats Stats
        {
            get
            {
                if (_db.Stats.Any())
                {
                    return _db.Stats.First();
                }
                return new Stats();
            }
        }

        public void OnGet()
        {
        }
    }
}