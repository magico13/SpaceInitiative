using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public class RecentEncounterCookie
    {
        private IRequestResponse _page;
        private AppDbContext _db;

        public RecentEncounterCookie(IRequestResponse page, AppDbContext dbContext)
        {
            _page = page;
            _db = dbContext;
        }

        private List<Encounter> _recentEncounters;
        public List<Encounter> RecentEncounters
        {
            get
            {
                if (_recentEncounters == null)
                {
                    _recentEncounters = new List<Encounter>();
                    if (_page.PageRequest.Cookies.TryGetValue("RecentEncounters", out string recentList))
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

        public void AddEncounterToCookie(Encounter encounter)
        {
            if (RecentEncounters.Find(e => e.EncounterStringID == encounter.EncounterStringID) == null)
            {
                RecentEncounters.Add(encounter);
                _page.PageResponse.Cookies.Append("RecentEncounters", string.Join(";", RecentEncounters.Select(e => e.EncounterStringID)));
            }
        }

        public void RemoveOldEncountersFromCookie()
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

    }
}
