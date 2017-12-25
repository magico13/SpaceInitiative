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

        [BindProperty]
        public string EncounterTitle { get; set; }


        private List<Encounter> _recentEncounters;
        public List<Encounter> RecentEncounters
        {
            get
            {
                if (_recentEncounters == null)
                {
                    _recentEncounters = new List<Encounter>();
                    if (Request.Cookies.TryGetValue("RecentEncounters", out string recentList))
                    {
                        foreach (string recent in recentList.Split(";"))
                        {
                            if (!string.IsNullOrWhiteSpace(recent))
                            {
                                if (_recentEncounters.Find(e => e.EncounterStringID == recent) == null)
                                {
                                    if (_db.Encounters.Any(e => e.EncounterStringID == recent))
                                    {
                                        _recentEncounters.Add(_db.Encounters.First(e => e.EncounterStringID == recent));
                                    }
                                }
                            }
                        }
                    }
                }
                _recentEncounters = _recentEncounters.OrderByDescending(e => e.LastUpdate).ToList();
                return _recentEncounters;
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

            removeOldEncountersFromCookie();
            addEncounterToCookie(encounter);

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
                    removeOldEncountersFromCookie();
                    addEncounterToCookie(encounter);
                    return Redirect("/Encounter?id=" + EncounterStringID);
                }
            }
            await Task.Delay(0);
            return RedirectToPage();
        }


        private void addEncounterToCookie(Encounter encounter)
        {
            if (RecentEncounters.Find(e => e.EncounterStringID == encounter.EncounterStringID) == null)
            {
                RecentEncounters.Add(encounter);
                Response.Cookies.Append("RecentEncounters", string.Join(";", RecentEncounters.Select(e => e.EncounterStringID)));
            }
        }

        private void removeOldEncountersFromCookie()
        {
            // remove old encounters from cookie
 
            int encounterMax = 9;
            if (RecentEncounters.Count > encounterMax)
            {
                List<Encounter> oldestList = RecentEncounters.OrderBy(o => o.LastUpdate).ToList();
                while (RecentEncounters.Count > encounterMax)
                {
                    Encounter oldest = oldestList.First();
                    oldestList.Remove(oldest);
                    RecentEncounters.Remove(oldest);
                }
            }
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