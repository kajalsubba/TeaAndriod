using Android.Webkit;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PasswordChangePage : ContentPage
	{
        AppSettings _appSetting = AppConfigService.GetConfig();

        ClientLoginData LoginData = new ClientLoginData();
        public PasswordChangePage()
		{
			InitializeComponent();
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
        }

		private async void OnSubmitClicked(object sender, EventArgs e)
		{
            await changePassword();

        }

        private void ConfirmPasswordEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPassword.Text == txtConfirmPassword.Text)
            {
                passwordMatchLabel.Text = "Passwords match";
                passwordMatchLabel.TextColor = Color.Green;
            }
            else
            {
                passwordMatchLabel.Text = "Passwords do not match";
                passwordMatchLabel.TextColor = Color.Red;
            }
        }

       async Task changePassword()
        {
            string url = _appSetting.ApiUrl+"Admin/ChangePassword";

            if (txtPassword.Text!=txtConfirmPassword.Text)
            {
                await DisplayAlert("Error", "Password is not match..!", "OK");
                return;
            }

            // Create an object to be sent as JSON in the request body
            ChangePasswordModel dataToSend = new ChangePasswordModel
            {
                UserName = LoginData.ClientLoginDetails[0].ContactNo,
                Password = txtConfirmPassword.Text,
                LoginType ="Client",
                TenantId = Convert.ToInt32(LoginData.ClientLoginDetails[0].TenantId),
                CreatedBy = Convert.ToInt32(LoginData.ClientLoginDetails[0].UserId)
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var valueData = JsonConvert.DeserializeObject<SaveReturn>(results);

                    if (valueData.Id != 0)
                    {
  
                        await DisplayAlert("Success", valueData.Message, "OK");

                        await Navigation.PushAsync(new LoginPage());
                    }
                    else
                    {
                        await DisplayAlert("Info", "Opps, something went wrong..!", "OK");
                    }

                }

            }
        }
    }
    }