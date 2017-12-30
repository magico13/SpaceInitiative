using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaceInitiative.Data;
using Microsoft.AspNetCore.Http;

namespace SpaceInitiative.Pages
{
    public class IndexModel : PageModel, IRequestResponse
    {
        private readonly AppDbContext _db;
        private readonly StringIDGenerator _generator = new StringIDGenerator();

        [BindProperty]
        public string EncounterStringID { get; set; }

        [BindProperty]
        public string EncounterTitle { get; set; }

        public HttpRequest PageRequest { get { return Request; } }
        public HttpResponse PageResponse { get { return Response; } }

        private RecentEncounterCookie _encounterCookie;
        public RecentEncounterCookie EncounterCookie
        {
            get
            {
                if (_encounterCookie == null)
                {
                    _encounterCookie = new RecentEncounterCookie(this, _db);
                }
                return _encounterCookie;
            }
        }


        public IndexModel(AppDbContext db)
        {
            _db = db;
        }


        [HttpGet]
        public void OnGet()
        {
            
        }


        [HttpPost]
        public async Task<IActionResult> OnPostCreateEncounterAsync()
        {
            Encounter encounter = new Encounter()
            {
                EncounterStringID = _generator.GenerateIDString(),
                LastUpdate = DateTime.UtcNow,
                EncounterTitle = EncounterTitle
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
            //add to the stats
            if (!_db.Stats.Any())
            {
                _db.Stats.Add(new Stats());
                _db.SaveChanges();
            }
            _db.Stats.First().TotalEncountersCreated++;
            _db.SaveChanges();

            EncounterCookie.RemoveOldEncountersFromCookie();
            EncounterCookie.AddEncounterToCookie(encounter);

            await removeOldEncountersFromDB();

            return Redirect("/Encounter?id=" + encounter.EncounterStringID);
        }

        [HttpPost]
        public async Task<IActionResult> OnPostResumeEncounterAsync()
        {
            if (!string.IsNullOrWhiteSpace(EncounterStringID))
            {
                Encounter encounter = null;
                if (_db.Encounters.Any(e => e.EncounterStringID == EncounterStringID))
                {
                    encounter = _db.Encounters.First(e => e.EncounterStringID == EncounterStringID);
                }

                if (encounter != null)
                {
                    EncounterCookie.RemoveOldEncountersFromCookie();
                    EncounterCookie.AddEncounterToCookie(encounter);
                    return Redirect("/Encounter?id=" + EncounterStringID);
                }
            }
            await Task.Delay(0);
            return RedirectToPage();
        }

        private async Task removeOldEncountersFromDB()
        {
            TimeSpan cutoff = TimeSpan.FromDays(14);
            foreach (Encounter encounter in new List<Encounter>(_db.Encounters))
            {
                if (DateTime.UtcNow - encounter.LastUpdate > cutoff)
                {
                    _db.Encounters.Remove(encounter);
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}