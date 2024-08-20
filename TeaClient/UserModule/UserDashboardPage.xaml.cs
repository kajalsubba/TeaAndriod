using Android.Webkit;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SessionHelper;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserDashboardPage : ContentPage
    {
        public IList<DashboardModel> MySource { get; set; }

        public UserDashboardPage()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            MySource = new ObservableCollection<DashboardModel>()
            {
                new DashboardModel(){Title ="STG Entry" ,BgImageSource="addStg.png"},
                new DashboardModel(){Title ="Leaf History" ,BgImageSource="leaf.png"},
                new DashboardModel(){Title ="Payment History" ,BgImageSource="smart.png"},
                //new DashboardModel(){Title ="Bill History" ,BgImageSource="bill.png"},
                new DashboardModel(){Title ="Change Password" ,BgImageSource="password.png"},
                new DashboardModel(){Title ="Logout" ,BgImageSource="logout.png"},
            };

            BindingContext = this;
            HeaderName.Text = "Welcome";
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
                        // await Navigation.PushAsync(new SupplierHistory());
                        await DisplayAlert("Info", "This Page is under construction. ", "Ok");
                        break;

                    case "File: smart.png":
                        //  await Navigation.PushAsync(new SmartHistoryPage());
                        await DisplayAlert("Info", "This Page is under construction. ", "Ok");

                        break;
                    //case "File: bill.png":
                    //    await Navigation.PushAsync(new BillHistoryPage());
                    //    break;
                    case "File: password.png":
                        //    await Navigation.PushAsync(new PasswordChangePage());
                        await DisplayAlert("Info", "This Page is under construction. ", "Ok");

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