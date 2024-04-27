using Android.Webkit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();


        public IList<ClientLoginData> TenantList { get; set; }

        public LoginPage()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
         //   GetUpdateApp();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            string userId = txtUserName.Text;
            string password = txtPassword.Text;
            if (string.IsNullOrWhiteSpace(userId))
            {
                await DisplayAlert("Validation", "Please Enter User Name", "OK");
                return;
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Validation", "Please Enter Password", "OK");
                return;
            }
            string url = _appSetting.ApiUrl + "Admin/ClientLogin";

            var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                UserId = txtUserName.Text,
                Password = txtPassword.Text,
                TenantId = selectedTenant.TenantId
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<ClientLoginData>(results);
                    if (data.ClientLoginDetails.Count >0)
                    {
                        SessionManager.SetSessionValue("loginDetails", data);

                        if (data.ClientLoginDetails[0].FirstTimeLogin == true)
                        {
                            await Navigation.PushAsync(new PasswordChangePage());
                        }
                        else
                        {
                            await Navigation.PushAsync(new DashboardPage());
                        }
                    }
                    else
                    {
                       await DisplayAlert("Error", "Invalid Login, try again", "OK");
                    }
                }
            }
               
        }


        public async void GetUpdateApp()
        {

            using (HttpClient client = new HttpClient())
            {
                string url = _appSetting.ApiUrl+ "Admin/GetApkUpdateNotification";

                HttpResponseMessage response = await client.GetAsync(url);
                var results = await response.Content.ReadAsStringAsync();
               // var data = JsonConvert.DeserializeObject<VesionModel>(results);
            }


        }
    }
}