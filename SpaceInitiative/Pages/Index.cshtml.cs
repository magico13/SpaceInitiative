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
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Ship Ship { get; set; }

        public Encounter Encounter
        {
            get
            {
                if (!_db.Encounters.Any())
                {
                    _db.Encounters.Add(new Encounter() { EncounterStringID = "A1337", LastUpdate = DateTime.UtcNow });
                }
                Encounter encounter = _db.Encounters.Find(1);
                encounter.LastUpdate = DateTime.UtcNow;
                return encounter;
            }
        }

        private List<Ship> _ships;
        public IList<Ship> Ships
        {
            get
            {
                return _db.Ships.Where(s => s.EncounterID == Encounter.EncounterID).OrderByDescending(s => s.Roll).ToList();
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

        public async Task OnGetAsync()
        {
            await Task.Delay(1);
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
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var ship = await _db.Ships.FindAsync(id);

            if (ship != null)
            {
                _db.Ships.Remove(ship);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRollAsync()
        {
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
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, int index)
        {
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
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDuplicateAsync(int id)
        {
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetCounterAsync()
        {
            CurrentRound.Round = 1;
            CurrentRound.Step = ROUND_STEP.ENGINEERING;
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
