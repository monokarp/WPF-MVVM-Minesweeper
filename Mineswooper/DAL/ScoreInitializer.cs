using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mineswooper.DAL
{
    public class ScoreInitializer : DropCreateDatabaseIfModelChanges<ScoreContext>
    {
        protected override void Seed(ScoreContext context)
        {
            ScoreEntry scoreOne = new ScoreEntry { Name = "Jesus", Score = 1, Date = new DateTime(25, 6, 15) };
            ScoreEntry scoreTwo = new ScoreEntry { Name = "Man", Score = 40, Date = new DateTime(1672, 3, 8) };
            ScoreEntry scoreThree = new ScoreEntry { Name = "Manlet", Score = 120, Date = new DateTime(1995, 10, 22) };

            context.ScoreEntries.Add(scoreOne);
            context.ScoreEntries.Add(scoreTwo);
            context.ScoreEntries.Add(scoreThree);

            context.SaveChanges();
        }
    }
}
