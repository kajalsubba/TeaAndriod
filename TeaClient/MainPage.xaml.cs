using Android.Content.Res;
using Android.Views.Accessibility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.CommonPage;
using Xamarin.Forms;

namespace TeaClient
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnSupplier_Clicked(object sender, EventArgs e)
        {

             await Navigation.PushAsync(new LoginPage());

           // await Navigation.PushAsync(new DeviceSettings.DevicesSettingsPage());
        }
        protected override bool OnBackButtonPressed()
        {
            //  DisplayConfirmation();
            return true; // Do not continue processing the back button
        }

        private async void BtnClient_Clicked(object sender, EventArgs e)
        {
            //var loadingPage = new TransferPopUpPage();
            //await Navigation.PushModalAsync(loadingPage);
            await Navigation.PushAsync(new UserModule.UserLoginPage());
        }
    }
}
