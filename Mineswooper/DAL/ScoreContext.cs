using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mineswooper.DAL
{
    public class ScoreContext : DbContext
    {
        public DbSet<ScoreEntry> ScoreEntries { get; set; }
    }
}
