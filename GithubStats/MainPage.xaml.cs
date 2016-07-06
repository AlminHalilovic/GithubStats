using GithubStats.DataAccessLayer;
using GithubStats.Models;
using GithubStats.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GithubStats
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {


        public MainPage()
        {
            this.InitializeComponent();


        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);
            var repository = (string)e.Parameter;
            dayRB.IsChecked = true;
            Utility.ViewModel.setRepository(repository);
            DataContext = Utility.ViewModel;

            PivotPlatform.ItemsSource = Utility.ViewModel.PivotItemsDays;



        }

        private void mainR_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton r = (RadioButton)sender;
            switch (r.Content.ToString())
            {
                case "Day":
                    PivotPlatform.ItemsSource = Utility.ViewModel.PivotItemsDays;
                    (Application.Current.Resources["currentThemeColor"] as SolidColorBrush).Color = (Application.Current.Resources["blueColor"] as SolidColorBrush).Color;

                    break;
                case "Week":
                    PivotPlatform.ItemsSource = Utility.ViewModel.PivotItemsWeeks;
                    (Application.Current.Resources["currentThemeColor"] as SolidColorBrush).Color = (Application.Current.Resources["greenColor"] as SolidColorBrush).Color;
                    break;
                case "Month":
                    PivotPlatform.ItemsSource = Utility.ViewModel.PivotItemsMonths;
                    (Application.Current.Resources["currentThemeColor"] as SolidColorBrush).Color = (Application.Current.Resources["yellowColor"] as SolidColorBrush).Color;

                    break;
            }

        }

        private  void logOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Utility.ViewModel.CanClose) return;
            Dal.DropDatabase();
            Frame.Navigate(typeof(LoginPage));

        }

        private async void PullToRefreshExtender_RefreshRequested(object sender, AmazingPullToRefresh.Controls.RefreshRequestedEventArgs e)
        {
            if (!Utility.IsInternetAvailable())
            {
                await new MessageDialog("No internet access").ShowAsync();
                return;
            }

            var deferral = e.GetDeferral();
            Dal.DropDatabase();
            await Utility.ViewModel.GetNewData();
           
            deferral.Complete();
        }
    }


}
