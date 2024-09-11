using Android.Webkit;
using Microsoft.Net.Http.Headers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserDashboardPage : ContentPage
    {
        public IList<DashboardModel> MySource { get; set; }
        UserModel LoginData = new UserModel();
        public SQLiteConnection conn;
        public UserDashboardPage()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            MySource = new ObservableCollection<DashboardModel>()
            {
                new DashboardModel(){Title ="Leaf Collection" ,BgImageSource="addStg.png"},
                new DashboardModel(){Title ="Collection History" ,BgImageSource="leaf.png"},
                new DashboardModel(){Title ="Clean Memory" ,BgImageSource="cleanMemory.png"},
                new DashboardModel(){Title ="Add Vehicle" ,BgImageSource="addVehicle.png"},
                new DashboardModel(){Title ="Save Printer" ,BgImageSource="print.png"},
                new DashboardModel(){Title ="Recover" ,BgImageSource="recover.png"},

                new DashboardModel(){Title ="Change Password" ,BgImageSource="password.png"},
                new DashboardModel(){Title ="Logout" ,BgImageSource="logout.png"},
            };
            conn = DependencyService.Get<ISqlLite>().GetConnection();

            //conn.CreateTable<SaveLocalCollectionModel>();
            BindingContext = this;
            HeaderName.Text = "Welcome " + LoginData.LoginDetails[0].UserFirstName;
        }

        protected override bool OnBackButtonPressed()
        {
            //  DisplayConfirmation();
            return true; // Do not continue processing the back button
        }

        private async void OnImageTapped(object sender, EventArgs e)

        {
            if (sender is Image image)
            {

                string sourcePath = image.Source.ToString();

                switch (sourcePath)
                {
                    case "File: addStg.png":
                        await Navigation.PushAsync(new TripAssignPage());
                        break;
                    case "File: leaf.png":
                        await Navigation.PushAsync(new UserHistory.StgHistory());
                        break;

                    case "File: cleanMemory.png":
                        //  await Navigation.PushAsync(new SmartHistoryPage());
                        bool _submit = await DisplayAlert("Clean", "Make sure your work is completed.Then only Clean!", "Yes", "No");
                        if (_submit)
                        {
                            conn.Execute("DROP TABLE IF EXISTS SaveLocalCollectionModel");
                            conn.CreateTable<SaveLocalCollectionModel>();
                            await DisplayAlert("Info", "Cleaning has done successfully.", "Ok");

                        }
                        break;
                    case "File: addVehicle.png":
                        await Navigation.PushAsync(new UserModule.AddVehiclePage());
                        break;
                    case "File: print.png":
                        await Navigation.PushAsync(new DeviceSettings.DevicesSettingsPage());
                        break;
                    case "File: password.png":
                        await DisplayAlert("Info", "This Page is under construction. ", "Ok");

                        break;
                    case "File: recover.png":
                        await Navigation.PushAsync(new UserModule.RecoverCollectionPage());

                        break;
                    case "File: logout.png":
                        SessionManager.ClearSession();
                        await Navigation.PushAsync(new MainPage());
                        break;
                    default:
                        return;
                }
            }
        }
    }
}