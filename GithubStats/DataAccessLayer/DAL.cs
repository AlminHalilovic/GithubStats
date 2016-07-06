using GithubStats.Models;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.WinRT;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using Windows.Storage;

namespace GithubStats.DataAccessLayer
{
    internal static class Dal
    {
        private static string dbPath = string.Empty;
        private static string DbPath
        {
            get
            {
                if (string.IsNullOrEmpty(dbPath))
                {
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Storage.sqlite");
                }

                return dbPath;
            }
        }

        private static SQLiteConnection DbConnection
        {
            get
            {
                return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            }
        }

        public static async Task CreateDatabase()
        {
            
            // Create a new connection
            using (var db = DbConnection)
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Create the table if it does not exist
                var c = db.CreateTable<RepositoryDb>();
                var info = db.GetMapping(typeof(RepositoryDb));
                c = db.CreateTable<DayPunchCardDb>();
                info = db.GetMapping(typeof(DayPunchCardDb));
                c = db.CreateTable<WeekDb>();
                info = db.GetMapping(typeof(WeekDb));
                c = db.CreateTable<DayDb>();
                info = db.GetMapping(typeof(DayDb));
            }
            DropDatabase();
        }


        public static void DropDatabase()
        {
            try {
                // Create a new connection
                using (var db = DbConnection)
                {
                    // Activate Tracing
                    db.TraceListener = new DebugTraceListener();

                    db.DeleteAll<DayDb>();
                    db.DeleteAll<WeekDb>();
                    db.DeleteAll<DayPunchCardDb>();

                    db.DeleteAll<RepositoryDb>();
                }
            }catch(Exception e) { }

        }



        public static RepositoryDb GetRepositoryByName(string name)
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {

                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                RepositoryDb m = new RepositoryDb() { Id = -1, FullName = "", Description = "", Weeks = new List<WeekDb>(), DaysPunchCard = new List<DayPunchCardDb>() };
                try {
                    var t = (from p in db.Table<RepositoryDb>()
                             where p.FullName == name
                             select p).FirstOrDefault() ?? m;

                    if (t != null)
                    {
                        t.Weeks = Dal.GetWeekCommitsByRepository(t);
                        t.DaysPunchCard = Dal.GetDayPunchCardCommitsByRepository(t);
                    }



                    return t;
                }catch(Exception e)
                {
                    return m;
                }
            }
        }

        public static List<DayPunchCardDb> GetDayPunchCardCommitsByRepository(RepositoryDb repo)
        {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                List<DayPunchCardDb> t = new List<DayPunchCardDb>();
                if (repo.Id == -1) return t; //ako nema repozitorija s tim id, vrati odmah praznu listu commita

                var m = (from p in db.Table<DayPunchCardDb>()
                         where p.RepositoryDbId == repo.Id
                         select p).ToList() ?? t;
                return m;
            }
        }

        public static List<WeekDb> GetWeekCommitsByRepository(RepositoryDb repo)
        {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                List<WeekDb> t = new List<WeekDb>();
                //if (repo.Id == -1) return t; //ako nema repozitorija s tim id, vrati odmah praznu listu commita

                var m = (from p in db.Table<WeekDb>()
                         where p.RepositoryDbId == repo.Id
                         select p).ToList() ?? t;

                return m;
            }
        }

        public static List<DayDb> GetDayCommits()
        {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                List<DayDb> t = new List<DayDb>();

                var m = (from p in db.Table<DayDb>()

                         select p).ToList() ?? t;

                return m;
            }
        }




        public static void InsertDayPunchCardCommitsByRepository(RepositoryDb repo)
        {


            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                db.InsertAll(repo.DaysPunchCard);
            }

        }

        public static void InsertWeekCommitsByRepository(RepositoryDb repo)
        {


            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                db.InsertOrReplaceWithChildren(repo);
            }

        }
        public static void InsertDayCommits(List<DayDb> days)
        {


            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                db.InsertAll(days);
            }

        }

        public static void SaveRepository(RepositoryDb repository)
        {


            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                db.InsertOrReplaceWithChildren(repository, recursive: true);
            }
        }

        public static async Task InsertDayPunchCardCommitsByRepositoryAsync(RepositoryDb repo)
        {

           
            using (var connection = new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DbPath, false)))
            {
                connection.TraceListener = new DebugTraceListener();
                var asyncConnection = new SQLiteAsyncConnection(() => { return connection; });

                await asyncConnection.InsertAllAsync(repo.DaysPunchCard);

            }

        }

        public static async Task InsertWeekCommitsByRepositoryAsync(RepositoryDb repo)
        {

            
            using (var connection = new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DbPath, false)))
            {
                connection.TraceListener = new DebugTraceListener();
                var asyncConnection = new SQLiteAsyncConnection(() => { return connection; });

                await asyncConnection.InsertAllAsync(repo.Weeks);

            }

        }

        public static async Task InsertDayCommitsAsync(List<DayDb> days)
        {

          
            using (var connection = new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DbPath, false)))
            {
                connection.TraceListener = new DebugTraceListener();
                var asyncConnection = new SQLiteAsyncConnection(() => { return connection; });

                await asyncConnection.InsertAllAsync(days);

            }

        }

        public static async Task<List<DayDb>> GetDayCommitsAsync()
        {


            using (var connection = new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DbPath, false)))
            {
                connection.TraceListener = new DebugTraceListener();
                var asyncConnection = new SQLiteAsyncConnection(() => { return connection; });

                var commits = await asyncConnection.Table<DayDb>().ToListAsync();


                return commits != null ? commits : new List<DayDb>();
            }



        }

        public static async Task<List<DayPunchCardDb>> GetDayPunchCardCommitsByRepositoryAsync(RepositoryDb repo)
        {

            using (var connection = new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DbPath, false)))
            {
                connection.TraceListener = new DebugTraceListener();
                var asyncConnection = new SQLiteAsyncConnection(() => { return connection; });

                var commits = await asyncConnection.Table<DayPunchCardDb>().Where(x => x.RepositoryDbId == repo.Id).ToListAsync();


                return commits != null ? commits : new List<DayPunchCardDb>();
            }


        }

        public static async Task<List<WeekDb>> GetWeekCommitsByRepositoryAsync(RepositoryDb repo)
        {
            using (var connection = new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DbPath, false)))
            {
                connection.TraceListener = new DebugTraceListener();
                var asyncConnection = new SQLiteAsyncConnection(() => { return connection; });

                var commits = await asyncConnection.Table<WeekDb>().Where(x => x.RepositoryDbId == repo.Id).ToListAsync();


                return commits != null ? commits : new List<WeekDb>();
            }
        }






    }
}
