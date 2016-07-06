using GithubStats.DataAccessLayer;
using Octokit;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GithubStats
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Windows.UI.Xaml.Controls.Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {

            Authenticate();
        }


        public async void Authenticate()
        {
            loginButton.IsEnabled = false;
            usernameTB.IsEnabled = false;
            passwordTB.IsEnabled = false;
            gitUrlTB.IsEnabled = false;

            try
            {
                if (usernameTB.Text == "" || passwordTB.Password == "" || gitUrlTB.Text == "")
                {
                    await new MessageDialog("Please enter all information").ShowAsync();
                    return;
                }

                string[] words = gitUrlTB.Text.Split('/');

                if (words.Count() != 2 || words[0] == "" || words[1] == "")
                {
                    await new MessageDialog("URL form is {user}/{repository}").ShowAsync();
                    return;
                }


                var github = new GitHubClient(new ProductHeaderValue("GithubStats"));
                var basicAuth = new Credentials(usernameTB.Text, passwordTB.Password); // NOTE: not real credentials
                github.Credentials = basicAuth;

                var repo = await github.Repository.Get(words[0], words[1]);

                Windows.Storage.ApplicationDataCompositeValue composite =
                     new Windows.Storage.ApplicationDataCompositeValue();
                Windows.Storage.ApplicationDataContainer localSettings =
                      Windows.Storage.ApplicationData.Current.LocalSettings;
                composite["githubRepository"] = repo.FullName.ToUpper();
                composite["githubUsername"] = usernameTB.Text;
                composite["githubPassword"] = passwordTB.Password;
                localSettings.Values["githubSettings"] = composite;

                Frame.Navigate(typeof(MainPage), repo.FullName.ToUpper());

            }
            catch (Octokit.NotFoundException)
            {
                await new MessageDialog("Invalid Repository URL!").ShowAsync();

            }
            catch (Exception ex) when (ex is System.ArgumentException || ex is Octokit.AuthorizationException)
            {
                await new MessageDialog("Invalid username or password!").ShowAsync();

            }
            catch (System.Net.Http.HttpRequestException)
            {
                await new MessageDialog("Check your internet connection!").ShowAsync();
            }
            finally
            {
                loginButton.IsEnabled = true;
                usernameTB.IsEnabled = true;
                passwordTB.IsEnabled = true;
                gitUrlTB.IsEnabled = true;
            }



        }

        private void usernameTB_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                Authenticate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Dal.CreateDatabase();
        }


    }
}
