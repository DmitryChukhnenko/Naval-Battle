using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData {
    internal class GameDataContext : DbContext {
        public DbSet<Save> Saves { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\.;Initial Catalog=GameData;Integrated Security=True");
        }
    }
}
