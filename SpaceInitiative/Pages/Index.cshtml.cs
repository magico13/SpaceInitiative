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

        private List<Ship> _ships;
        public IList<Ship> Ships
        {
            get
            {
                return _ships;
            }
            private set
            {
                _ships = value?.OrderByDescending(s => s.Roll).ToList();
            }
        }

        public RoundHolder CurrentRound
        {
            get
            {
                if (!_db.CurrentRound.Any())
                {
                    lock (_db)
                    {
                        if (!_db.CurrentRound.Any())
                        {
                            _db.CurrentRound.Add(new RoundHolder());
                            _db.SaveChanges();
                        }
                    }
                }
                return _db.CurrentRound.Single();
            }
            set
            {
                _db.CurrentRound.Single().CopyFrom(value);
                _db.SaveChanges();
            }
        }

        public async Task OnGetAsync()
        {
            Ships = await _db.Ships.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostAddShipAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

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
            if (CurrentRound.Round == 0)
            {
                CurrentRound.Round++;
                CurrentRound.Step = ROUND_STEP.ENGINEERING;
            }
            else
            {
                CurrentRound.Step = (ROUND_STEP)(((int)CurrentRound.Step + 1) % 3);
                if (CurrentRound.Step == ROUND_STEP.HELM)
                {
                    Random r = new Random();
                    foreach (var ship in _db.Ships)
                    {
                        ship.Roll = r.Next(20) + 1 + ship.BonusCurrent;
                        _db.Attach(ship).State = EntityState.Modified;
                    }
                }
                else if (CurrentRound.Step == ROUND_STEP.ENGINEERING)
                {
                    CurrentRound.Round++;
                }
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
                Ship newShip = new Ship();
                newShip.Name = ship.Name + " (copy)";
                newShip.BonusBase = ship.BonusBase;
                _db.Ships.Add(newShip);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetCounterAsync()
        {
            CurrentRound.Round = 0;
            CurrentRound.Step = ROUND_STEP.ENGINEERING;
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
