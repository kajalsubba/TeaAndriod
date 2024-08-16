using Android.Content.Res;
using Android.Views.Accessibility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
  
        private async void BtnClient_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserModule.UserLoginPage());
        }
    }
}
