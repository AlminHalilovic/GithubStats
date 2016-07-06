using GithubStats.DataAccessLayer;
using GithubStats.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace GithubStats
{
    public class GithubFacade
    {

        public static async Task PopulateGithubCommitsAsync(string repo)
        {
            try
            {
                var days = await GetDayCommitsDataWrapperAsync(repo);
                var weeks = await GetWeekCommitsDataWrapperAsync(repo);



                RepositoryDb r = Dal.GetRepositoryByName(repo);
                if (r != null && days != null && weeks != null)
                {

                    r.Weeks = weeks.Activity.Select(x => new WeekDb { RepositoryDbId = r.Id, Date = x.WeekTimestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss"), DayOfWeek = x.WeekTimestamp.DayOfWeek.ToString(), Month = x.WeekTimestamp.Month, CommitCount = x.Total }).ToList();
                    r.DaysPunchCard = days.PunchPoints.Select(x => new DayPunchCardDb { RepositoryDbId = r.Id, HourOfTheDay = x.HourOfTheDay, DayOfWeek = x.DayOfWeek.ToString(), CommitCount = x.CommitCount }).ToList();

                    //Dal.DropDatabase();

                    //Dal.InsertDayPunchCardCommitsByRepository(r);

                    //Dal.InsertWeekCommitsByRepository(r);
                    //Dal.SaveRepository(r);

                    await Dal.InsertDayPunchCardCommitsByRepositoryAsync(r);

                    await Dal.InsertWeekCommitsByRepositoryAsync(r);



                    var result = Dal.GetWeekCommitsByRepository(r);

                    result.ToList().ForEach(c => c.Days = GetDays(weeks.Activity.Where(x => x.WeekTimestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss") == c.Date).FirstOrDefault().Days, c.Id));

                    List<DayDb> daysToInsert = new List<DayDb>();
                    foreach (WeekDb w in result) { daysToInsert.AddRange(w.Days); }
                    //Dal.InsertDayCommits(daysToInsert);

                    await Dal.InsertDayCommitsAsync(daysToInsert);

                }


            }
            catch (Exception)
            {
                return;
            }
        }

        private static List<DayDb> GetDays(IReadOnlyList<int> days, int weekId)
        {
            List<DayDb> result = new List<DayDb>();
            for (int i = 0; i < 7; i++)
            {
                result.Add(new DayDb { DayNumber = i, CommitCount = days[i], WeekDbId = weekId });
            }
            return result;
        }


        private static async Task<CommitActivity> GetWeekCommitsDataWrapperAsync(string repo)
        {
            CommitActivity activity = null;
            try
            {

                Windows.Storage.ApplicationDataContainer localSettings =
                      Windows.Storage.ApplicationData.Current.LocalSettings;
                Windows.Storage.ApplicationDataCompositeValue composite =
               (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values["githubSettings"];
                if (composite == null) return null;
                GitHubClient github = new GitHubClient(new ProductHeaderValue("GithubStats"));
                github.Credentials = new Credentials((string)composite["githubUsername"], (string)composite["githubPassword"]);


                string[] words = (composite["githubRepository"] as string).Split('/');
                RepositoryDb repository = Dal.GetRepositoryByName(repo);

                activity = (CommitActivity)await github.Repository.Statistics.GetCommitActivity(words[0], words[1]);



                return activity;
            }
            catch (Exception) { }
            return activity;
        }

        private static async Task<PunchCard> GetDayCommitsDataWrapperAsync(string repo)
        {
            PunchCard punchCard = null;
            try
            {

                Windows.Storage.ApplicationDataContainer localSettings =
                      Windows.Storage.ApplicationData.Current.LocalSettings;
                Windows.Storage.ApplicationDataCompositeValue composite =
               (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values["githubSettings"];
                if (composite == null) return null;
                GitHubClient github = new GitHubClient(new ProductHeaderValue("GithubStats"));
                github.Credentials = new Credentials((string)composite["githubUsername"], (string)composite["githubPassword"]);
                CommitRequest c = new CommitRequest();
                c.Since = new DateTimeOffset(DateTime.Now.StartOfWeek(DayOfWeek.Sunday));

                string[] words = (composite["githubRepository"] as string).Split('/');
                RepositoryDb repository = Dal.GetRepositoryByName(repo);

                punchCard = (PunchCard)await github.Repository.Statistics.GetPunchCard(words[0], words[1]);




                return punchCard;
            }
            catch (Exception) { }
            return punchCard;
        }

        public static async Task PopulateGithubRepositoryAsync(string repo)
        {
            try
            {
                var repository = await GetRepositoryDataWrapperAsync(repo);

                RepositoryDb r = new RepositoryDb { FullName = repository.full_name.ToUpper(), Description = repository.description };

                Dal.SaveRepository(r);

            }
            catch (Exception)
            {
                return;
            }
        }


        private static async Task<RepositoryDataWrapper> GetRepositoryDataWrapperAsync(string repo)
        {
            try
            {
                string url = String.Format("https://api.github.com/repos/{0}",
                    repo);

                var jsonMessage = await CallGithubAsync(url);

                // Response -> string / json -> deserialize
                var serializer = new DataContractJsonSerializer(typeof(RepositoryDataWrapper));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

                var result = (RepositoryDataWrapper)serializer.ReadObject(ms);
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }



        private async static Task<string> CallGithubAsync(string url)
        {

            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent",
                                 "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2;  WOW64; Trident / 6.0)");
            var response = await http.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }


    }
}
