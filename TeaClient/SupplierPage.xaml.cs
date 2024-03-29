using Android.App;
using Java.Util;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SessionHelper;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace TeaClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SupplierPage : ContentPage
    {
        public IList<FactoryAccountModel> FactoryAccountlists { get; set; }

        public IList<FactoryAccountModel> Accountlists { get; set; }
        public SupplierPage()
        {
            InitializeComponent();
            Accountlists = new ObservableCollection<FactoryAccountModel>();
            GetFactoryAccount();
            // var dd=  SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
        }
        private void OnSubmitClicked(object sender, EventArgs e)
        {

        }
        private void OnFactotyIndexChanged(object sender, EventArgs e)
        {
           // var selectedTenant = (TenantList)Tenant.SelectedItem;
            var selectedAccount =FactoryName.SelectedItem as FactoryModel;
            IList<FactoryAccountModel> filteredList = Accountlists.Where(p => p.FactoryId == selectedAccount.FactoryId).ToList();

            AccountName.ItemsSource = (System.Collections.IList)filteredList;
            AccountName.ItemDisplayBinding = new Binding("AccountName");

        }
        public async void GetFactoryAccount()
        {

            string url = "http://72.167.37.70:82/Master/GetFactoryAccount";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                IsClientView = true,
                TenantId = 1
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<FactoryAccountList>(results);
                    foreach (var factory in data.AccountDetails)
                    {

                        Accountlists.Add(new FactoryAccountModel { FactoryId = factory.FactoryId, AccountId = factory.AccountId, AccountName = factory.AccountName });
                    }

                }
                //AccountName.ItemsSource = (System.Collections.IList)Accountlists;
                //AccountName.ItemDisplayBinding = new Binding("AccountName");

            }


        }
        private async void OnCaptureClicked(object sender, EventArgs e)
        {
           await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsTakePhotoSupported && !CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "Format is not supported", "ok");
                return;
            }
            else
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Images",
                    Name = DateTime.Now.ToString() + ".jpg"
                }) ; 
                if (file==null)
                {
                    return;
                  

                }
                await DisplayAlert("File Path ", file.Path, "ok");
                ChallanImage.Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    return stream;
                });

            }
        }

        

    }
}