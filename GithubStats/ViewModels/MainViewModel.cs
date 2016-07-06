using GithubStats.DataAccessLayer;
using GithubStats.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace GithubStats.ViewModels
{
    public class Utility
    {

        private static MainViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        public static bool IsInternetAvailable()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }


    }



    public class PivotSection
    {
        public string Title { get; set; } //ex. Today total
        public string Total { get; set; } // ex. 294
        public string Date { get; set; } //ex. March 10
        public string ProgressValue { get; set; }
        public ObservableCollection<ChartTemplate> Result { get; set; }

        public PivotSection()
        {
            Result = new ObservableCollection<ChartTemplate>();
        }
    }

    public class ChartTemplate
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }


    public class MainViewModel
    {


        public ObservableCollection<RepositoryDb> Repository { get; set; }

        private static string repository = null;
        public ObservableCollection<PivotSection> PivotItemsDays { get; set; }
        public ObservableCollection<PivotSection> PivotItemsWeeks { get; set; }
        public ObservableCollection<PivotSection> PivotItemsMonths { get; set; }
        public SolidColorBrush CurrentColorBrush { get; set; }

        public bool CanClose { get; set; }

        public MainViewModel()
        {


        }


        public async void InitializeData()
        {
            PivotItemsDays = new ObservableCollection<PivotSection>();
            PivotItemsWeeks = new ObservableCollection<PivotSection>();
            PivotItemsMonths = new ObservableCollection<PivotSection>();
            Repository = new ObservableCollection<RepositoryDb>();
            CurrentColorBrush = Application.Current.Resources["blueColor"] as SolidColorBrush;
            if (repository == null) return;

            repository = repository.ToUpper();
            CanClose = false;
            Refresh();
            await GetNewData();
          
        }


        public void setRepository(string repo)
        {
            repository = repo;
            InitializeData(); //inicijaliziram podatke ovdje jer u suprotnom singleton dobavi podatke za prethodni repozitorij tokom logOut/logIn pokusaja
        }



        private void LoadPivot(List<WeekDb> weeks, List<DayPunchCardDb> days)
        {
            PivotItemsDays.Clear();
            PivotItemsWeeks.Clear();
            PivotItemsMonths.Clear();
            List<string> daysOfWeek = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            DateTime lastWeekDate = weeks.Count == 0 ? DateTime.Now : DateTime.Parse(weeks[51].Date);

            #region logika za dane
            for (int i = 0; i < 7; i++)
            {
                List<DayPunchCardDb> pcd = days.Where(x => x.DayOfWeek == daysOfWeek[i]).ToList();
                PivotSection pivotItemDays = new PivotSection
                {
                    Date = daysOfWeek[i],
                    Title = "Today Total",
                    Total = days.Count == 0 ? "0" : pcd.Sum(x=>x.CommitCount).ToString(),
                    ProgressValue = pcd.Count == 0 ? "0" : ((decimal)pcd.Sum(x=>x.CommitCount) / (decimal)days.Sum(x=>x.CommitCount) * 100).ToString()

                };
                pivotItemDays.Result.Clear();
                var space = "";
                for (int j = 0; j < 24; j++)
                {
                    var name = "";
                    space += " ";
                    if (j == 0) name = "0 hrs"; else if (j == 23) name = "24 hrs"; else name = space;

                    var amount = pcd.Count == 0 ? 0 : pcd.Where(x => x.HourOfTheDay == j).FirstOrDefault().CommitCount;
                    pivotItemDays.Result.Add(new ChartTemplate { Name = name, Amount = amount });
                }
                PivotItemsDays.Add(pivotItemDays);
            }
            #endregion

            #region logika za sedmice
            foreach (WeekDb week in weeks)
            {
                DateTime date = DateTime.ParseExact(week.Date, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                PivotSection pivotItemWeeks = new PivotSection
                {
                    Date = date.ToString("MMMM") + " " + date.Day,
                    Title = "Week Total",
                    Total = week.CommitCount.ToString(),
                    ProgressValue = weeks.Count == 0 ? "0" : ((decimal)week.CommitCount / (decimal)weeks.Sum(x => x.CommitCount) * 100).ToString()
                };

                // Za svaki dan u trenutnoj sedmici u petlji pronadji broj commita
                for (int i = 0; i < 7; i++)
                {

                    pivotItemWeeks.Result.Add(new ChartTemplate { Name = daysOfWeek[i] == "Sunday" || daysOfWeek[i]=="Thursday" ? daysOfWeek[i][0] + " " : daysOfWeek[i][0].ToString(), Amount = week.Days[i].CommitCount }); // space dodan za saturday i thursday, jer u suprotnom spoji iste labele
                }

                PivotItemsWeeks.Add(pivotItemWeeks);
            }
            #endregion

            #region logika za mjesece
            var months = weeks.Select(x => x.Month).Distinct().ToList();
            var filteredDays = GetDaysFromWeeks(weeks);

            foreach (int month in months)
            {
                DateTime date = DateTime.ParseExact(weeks.Where(x => x.Month == month).FirstOrDefault().Date, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                // List<DayDb> filteredMonthsList = new List<DayDb>();
                //foreach (WeekDb w in weeks.Where(x => x.Month == month)) { filteredMonthsList.AddRange(w.Days); }
                PivotSection pivotItemMonths = new PivotSection
                {
                    Date = date.ToString("MMMM") + " " + date.Year,
                    Title = "Month Total",
                    Total = weeks.Where(x => x.Month == month).Sum(y => y.CommitCount).ToString(),
                    ProgressValue = weeks.Count == 0 ? "0" : ((decimal)weeks.Where(x => x.Month == month).Sum(x => x.CommitCount) / (decimal)weeks.Sum(x => x.CommitCount) * 100).ToString()
                };

               
                int numberOfDays = DateTime.DaysInMonth(date.Year, date.Month);
                var space = "";
                for (int i = 1; i <= numberOfDays; i++)
                {
                    var name = "";
                    space += " ";
                    if (i == 1 || i == numberOfDays) name = date.ToString("MMMM") + " " + i; else name = space;
                    var amount = filteredDays.Exists(a => a.Date.Month == month && a.Date.Day == i) ? filteredDays.Where(x => x.Date.Month == month && x.Date.Day == i).FirstOrDefault().CommitCount : 0;
                    pivotItemMonths.Result.Add(new ChartTemplate { Name = name, Amount = amount }); // space dodan za sunday, jer u suprotnom spoji saturday i sunday kao labelu
                }

                PivotItemsMonths.Add(pivotItemMonths);

            }
            #endregion
        }


        private List<DayDb> GetDaysFromWeeks(List<WeekDb> weeks)
        {
            List<DayDb> days = new List<DayDb>();
            foreach (WeekDb week in weeks)
            {
                DateTime weekDate = DateTime.ParseExact(week.Date, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                foreach (DayDb day in week.Days)
                {
                    day.Date = weekDate.AddDays(day.DayNumber);
                    days.Add(day);
                }
            }



            return days;
        }




        public async void Refresh()
        {
            CanClose = false;
            RepositoryDb r = Dal.GetRepositoryByName(repository);
            if (r != null)
            {
                Repository.Clear();
                Repository.Add(r);
                List<WeekDb> weeks = await Dal.GetWeekCommitsByRepositoryAsync(r);
                List<DayDb> allDays =await Dal.GetDayCommitsAsync();
                List<DayPunchCardDb> punchCardDays = await Dal.GetDayPunchCardCommitsByRepositoryAsync(r);
                if (weeks != null && allDays != null && punchCardDays != null)
                {
                    weeks.ToList().ForEach(w => w.Days = allDays.Where(d => d.WeekDbId == w.Id).ToList());
                    LoadPivot(weeks, punchCardDays);

                }
            }
            CanClose = true;
        }

        public async Task GetNewData()
        {
            CanClose = false;
            Task t1 = GithubFacade.PopulateGithubRepositoryAsync(repository);
            await t1;
            Task t2 = GithubFacade.PopulateGithubCommitsAsync(repository);
            await t2;

            Refresh();
            CanClose = true;
        }



    }
}
