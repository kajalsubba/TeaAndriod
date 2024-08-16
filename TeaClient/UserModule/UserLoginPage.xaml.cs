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
    public partial class UserLoginPage : ContentPage
    {
        AppSettings _appSetting = AppConfigService.GetConfig();

        public UserLoginPage()
        {
            InitializeComponent();
        }

        //private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    VehicleListView.IsVisible = true;
        //    VehicleListView.BeginRefresh();

        //    try
        //    {
        //        var dataEmpty = data.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));

        //        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        //            VehicleListView.IsVisible = false;
        //        else if (dataEmpty.Max().Length == 0)
        //            VehicleListView.IsVisible = false;
        //        else
        //            VehicleListView.ItemsSource = data.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));
        //    }
        //    catch (Exception ex)
        //    {
        //        VehicleListView.IsVisible = false;

        //    }
        //    VehicleListView.EndRefresh();

        //}

        //private void ListView_OnItemTapped(Object sender, ItemTappedEventArgs e)
        //{
        //    //EmployeeListView.IsVisible = false;  

        //    String listsd = e.Item as string;
        //    VehicleNo.Text = listsd;
        //    VehicleListView.IsVisible = false;

        //    ((ListView)sender).SelectedItem = null;
        //}
        private async void Button_Clicked(object sender, EventArgs e)
        {

            try
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
                string url = _appSetting.ApiUrl + "Admin/Login";

          
                // Create an object to be sent as JSON in the request body
                var dataToSend = new
                {
                    UserName = txtUserName.Text,
                    Password = txtPassword.Text
               
                };

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(dataToSend);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    var results = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var data = JsonConvert.DeserializeObject<UserModel>(results);
                        if (data.LoginDetails.Count > 0)
                        {
                            SessionManager.SetSessionValue("UserDetails", data);

                            //if (data.LoginDetails[0].FirstTimeLogin == true)
                            //{
                               // await Navigation.PushAsync(new PasswordChangePage());
                          //  }
                            //else
                            //{
                                await Navigation.PushAsync(new TripAssignPage());
                           // }
                        }
                        else
                        {
                            await DisplayAlert("Error", "Invalid Login, try again", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message.ToString(), "OK");
            }
          
        }
        }
    }