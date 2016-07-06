using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubStats.Models
{
    public class CommitDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime CommitDate { get; set; }

        [ForeignKey(typeof(RepositoryDb))]
        public int RepositoryDbId { get; set; }

        [ManyToOne]      // Many to one relationship with Repository
        public RepositoryDb RepositoryDb { get; set; }


    }
}
