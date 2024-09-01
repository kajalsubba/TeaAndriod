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

namespace TeaClient.UserModule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddVehiclePage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        UserModel LoginData = new UserModel();

        public AddVehiclePage()
        {
            InitializeComponent();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");

        }
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            try
            {
                string url = _appSetting.ApiUrl + "Master/SaveVehicle";

                string _vehicle = txtVehicleNo.Text;
                if (string.IsNullOrWhiteSpace(_vehicle))
                {
                    await DisplayAlert("Validation", "Please Enter Vehicle", "OK");
                    return;
                }
                var dataToSend = new
                {
                    VehicleNo = txtVehicleNo.Text,
                    VehicleDetails = txtDetails.Text,
                    TenantId = Convert.ToInt32(LoginData.LoginDetails[0].TenantId),
                    CreatedBy = Convert.ToInt32(LoginData.LoginDetails[0].UserId)
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
                        await DisplayAlert("Info", valueData.Message, "OK");
                        txtVehicleNo.Text = string.Empty;
                        txtDetails.Text = string.Empty;
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