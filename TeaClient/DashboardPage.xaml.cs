using Android.Webkit;
using Plugin.Media.Abstractions;
using Plugin.Media;
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

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardPage : ContentPage
    {
       readonly ClientLoginData LoginData = new ClientLoginData();
        public IList<DashboardModel> MySource { get; set; }
        public DashboardPage()
        {

            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
            if (LoginData != null)
            {
                MySource = new ObservableCollection<DashboardModel>() {

                new DashboardModel(){Title ="Supplier Entry" ,BgImageSource="Supplier.png"},
                new DashboardModel(){Title ="Leaf History" ,BgImageSource="leaf.png"},
                new DashboardModel(){Title ="Smart History" ,BgImageSource="smart.png"},
                new DashboardModel(){Title ="Bill History" ,BgImageSource="bill.png"},
                //new DashboardModel(){Title ="Save Printer" ,BgImageSource="print.png"},
                new DashboardModel(){Title ="Change Password" ,BgImageSource="password.png"},
                new DashboardModel(){Title ="Logout" ,BgImageSource="logout.png"},
            };

                BindingContext = this;
                HeaderName.Text = "Welcome, " + LoginData.ClientLoginDetails[0].ClientName;
            }
            else
            {
                Navigation.PushAsync(new LoginPage());
            }
        }

        protected override bool OnBackButtonPressed()
        {
            DisplayConfirmation();
            return true; // Do not continue processing the back button
        }
        async void DisplayConfirmation()
        {
            bool logout = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (logout)
            {
                // Perform logout action
                await Navigation.PushAsync(new LoginPage()); // Navigate to dashboard page
                                                             // Perform logout logic here
            }
        }
        private async void OnImageTapped(object sender, EventArgs e)

        {
            try
            {
                if (sender is Image image)
                {

                    string sourcePath = image.Source.ToString();

                    switch (sourcePath)
                    {
                        case "File: Supplier.png":
                            await Navigation.PushAsync(new SupplierPage());
                            break;
                        case "File: leaf.png":
                            await Navigation.PushAsync(new SupplierHistory());
                            break;

                        case "File: smart.png":
                            await Navigation.PushAsync(new SmartHistoryPage());
                            break;
                        case "File: bill.png":
                            await Navigation.PushAsync(new BillHistoryPage());
                            break;

                        case "File: password.png":
                            await Navigation.PushAsync(new PasswordChangePage());
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
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

            }

        }


    }
}