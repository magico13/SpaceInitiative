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

        public IList<Ship> Ships { get; private set; }

        public async Task OnGetAsync()
        {
            Ships = await _db.Ships.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
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
            Random r = new Random();
            foreach (var ship in _db.Ships)
            {
                ship.Roll = r.Next(20) + 1 + ship.BonusCurrent;
                _db.Attach(ship).State = EntityState.Modified;
            }
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateBonusAsync(int id, int bonus)
        {
            var ship = await _db.Ships.FindAsync(id);

            if (ship != null)
            {
                ship.BonusCurrent = bonus;
                _db.Attach(ship).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
