using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaceInitiative.Data;
using Microsoft.EntityFrameworkCore;

namespace SpaceInitiative.Pages
{
    public class EncounterHolder
    {
        public string Id { get; set; }

        public static EncounterHolder Create(Encounter encounter)
        {
            return new EncounterHolder() { Id = encounter.EncounterStringID };
        }
    }

    public class EncounterModel : PageModel
    {
        private readonly AppDbContext _db;

        public EncounterModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Ship Ship { get; set; }

        [BindProperty]
        public int EncounterID { get; set; }

        private Encounter _encounter;
        public Encounter Encounter
        {
            get
            {
                if (_encounter == null)
                {
                    _encounter = _db.Encounters.Find(EncounterID);
                }
                if (_encounter != null)
                {
                    _encounter.LastUpdate = DateTime.UtcNow;
                }
                return _encounter;
            }
        }

        private List<Ship> _ships;
        public IList<Ship> Ships
        {
            get
            {
                if (_ships == null)
                {
                    _ships = _db.Ships.Where(s => s.EncounterID == Encounter.EncounterID).OrderByDescending(s => s.Roll).ToList();
                }
                return _ships;
            }
            //private set
            //{
            //    _ships = value?.OrderByDescending(s => s.Roll).ToList();
            //}
        }

        public RoundHolder CurrentRound
        {
            get
            {
                IQueryable<RoundHolder> rounds = _db.RoundHolders.Where(r => r.EncounterID == Encounter.EncounterID);
                if (!rounds.Any())
                {
                    lock (_db)
                    {
                        _db.RoundHolders.Add(new RoundHolder() { Round = 1, EncounterID = Encounter.EncounterID });
                        _db.SaveChanges();
                    }
                    return _db.RoundHolders.Where(r => r.EncounterID == Encounter.EncounterID).First();
                }
                else
                {
                    return rounds.First();
                }
                
            }
        }

        public async Task OnGetAsync(string id)
        {
            id = id?.ToUpper();
            if (!string.IsNullOrEmpty(id) && await _db.Encounters.AnyAsync(e => e.EncounterStringID == id))
            {
                _encounter = await _db.Encounters.FirstAsync(e => e.EncounterStringID == id);
                EncounterID = _encounter.EncounterID;
            }
            else
            {
                throw new Exception($"No Encounter found with the provided ID '{id}'");
            }
            //Ships = await _db.Ships.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostAddShipAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Ship.EncounterID = Encounter.EncounterID;
            _db.Ships.Add(Ship);
            await _db.SaveChangesAsync();
            return RedirectToPage(EncounterHolder.Create(Encounter));
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, int encounterID)
        {
            EncounterID = encounterID;
            var ship = await _db.Ships.FindAsync(id);

            if (ship != null)
            {
                _db.Ships.Remove(ship);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage(EncounterHolder.Create(Encounter));
        }

        public async Task<IActionResult> OnPostRollAsync(int encounterID)
        {
            EncounterID = encounterID;
            CurrentRound.Step = (ROUND_STEP)(((int)CurrentRound.Step + 1) % 3);
            if (CurrentRound.Step == ROUND_STEP.HELM)
            {
                Random r = new Random();
                foreach (var ship in Ships)
                {
                    ship.Roll = r.Next(20) + 1 + ship.BonusCurrent;
                    _db.Attach(ship).State = EntityState.Modified;
                }
            }
            else if (CurrentRound.Step == ROUND_STEP.ENGINEERING)
            {
                CurrentRound.Round++;
            }
            await _db.SaveChangesAsync();
            return RedirectToPage(EncounterHolder.Create(Encounter));
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, int index, int encounterID)
        {
            EncounterID = encounterID;
            if (ModelState.TryGetValue("Ship.BonusCurrent", out var bonusVal) && ModelState.TryGetValue("Ship.Name", out var nameVal))
            {
                string newName = nameVal.AttemptedValue.Split(',')[index];
                if (int.TryParse(bonusVal.AttemptedValue.Split(',')[index], out int bonus))
                  {
                    var ship = await _db.Ships.FindAsync(id);

                    if (ship != null)
                    {
                        ship.Name = newName;
                        ship.BonusCurrent = bonus;
                        _db.Attach(ship).State = EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                }
            }
            return RedirectToPage(EncounterHolder.Create(Encounter));
        }

        public async Task<IActionResult> OnPostDuplicateAsync(int id, int encounterID)
        {
            EncounterID = encounterID;
            var ship = await _db.Ships.FindAsync(id);

            if (ship != null)
            {
                Ship newShip = new Ship
                {
                    Name = ship.Name + " (copy)",
                    BonusBase = ship.BonusBase,
                    EncounterID = Encounter.EncounterID
                };
                _db.Ships.Add(newShip);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage(EncounterHolder.Create(Encounter));
        }

        public async Task<IActionResult> OnPostResetCounterAsync(int encounterID)
        {
            EncounterID = encounterID;
            CurrentRound.Round = 1;
            CurrentRound.Step = ROUND_STEP.ENGINEERING;
            await _db.SaveChangesAsync();
            return RedirectToPage(EncounterHolder.Create(Encounter));
        }
    }
}
