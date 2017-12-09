using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaceInitiative.Data;

namespace SpaceInitiative.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly StringIDGenerator _generator = new StringIDGenerator();

        [BindProperty]
        public string EncounterStringID { get; set; }


        public IndexModel(AppDbContext db)
        {
            _db = db;
        }


        [HttpGet]
        public void OnGet()
        {

        }


        [HttpPost]
        public IActionResult OnPostCreateEncounter()
        {
            Encounter encounter = new Encounter()
            {
                EncounterStringID = _generator.GenerateIDString(),
                LastUpdate = DateTime.UtcNow
            };

            //check for collisions
            //give up after 10 tries
            int tries = 0;
            while (_db.Encounters.Any(e => e.EncounterStringID == encounter.EncounterStringID) && tries++ < 10)
            {
                encounter.EncounterStringID = _generator.GenerateIDString();
            }
            if (tries >= 10)
            {
                throw new Exception("Could not generate a unique ID for the encounter, sorry!");
            }

            _db.Encounters.Add(encounter);
            _db.SaveChanges();

            return Redirect("/Encounter?id=" + encounter.EncounterStringID);
        }

        [HttpPost]
        public IActionResult OnPostResumeEncounter()
        {
            if (!string.IsNullOrWhiteSpace(EncounterStringID))
            {
                return Redirect("/Encounter?id=" + EncounterStringID);
            }
            return RedirectToPage();
        }
    }
}