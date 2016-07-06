using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubStats.Models
{
   public class DayDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

       
        public int DayNumber { get; set; }
        public int CommitCount { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey(typeof(WeekDb))]
        public int WeekDbId { get; set; }

        [ManyToOne]      // Many to one relationship with Repository
        public WeekDb WeekDb { get; set; }
    }
}
