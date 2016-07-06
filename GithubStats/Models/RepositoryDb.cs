using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubStats.Models
{
    public class RepositoryDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }



        [OneToMany(CascadeOperations = CascadeOperation.All)]      // One to many relationship with Weeks
        public List<WeekDb> Weeks
        {
            get; set;
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]      // One to many relationship with DayPunchCard
        public List<DayPunchCardDb> DaysPunchCard
        {
            get; set;
        }

    }
}
