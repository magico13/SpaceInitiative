﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Encounter> Encounters { get; set; }
        public DbSet<Ship> Ships { get; set; }
        public DbSet<RoundHolder> RoundHolders { get; set; }

        public DbSet<Stats> Stats { get; set; }
    }
}
