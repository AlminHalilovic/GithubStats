using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubStats.Models
{
   public class WeekDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Date { get; set; }
        public string DayOfWeek { get; set; }
        public int Month { get; set; }
        public int CommitCount { get; set; }


        [ForeignKey(typeof(RepositoryDb))]
        public int RepositoryDbId { get; set; }

        [ManyToOne]      // Many to one relationship with Repository
        public RepositoryDb RepositoryDb { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]      // One to many relationship with DayPunchCard
        public List<DayDb> Days
        {
            get; set;
        }


    }
}
