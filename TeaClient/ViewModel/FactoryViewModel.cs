using Android.Content.Res;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using Xamarin.Forms;

namespace TeaClient.ViewModel
{
    public class FactoryViewModel
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<FactoryModel> Factorylists { get; set; }
        ClientLoginData LoginData = new ClientLoginData();
        public FactoryViewModel()
        {
        
            Factorylists = new ObservableCollection<FactoryModel>();
            LoginData = SessionManager.GetSessionValue<ClientLoginData>("loginDetails");
            GetFactory();
          
        }
     
    public async void GetFactory()
        {

            string url = _appSetting.ApiUrl+"Master/GetFactory";

            //var selectedTenant = (TenantList)Tenant.SelectedItem;

            // Create an object to be sent as JSON in the request body
            var dataToSend = new
            {
                IsClientView = true,
                TenantId =Convert.ToInt32(LoginData.ClientLoginDetails[0].TenantId)
            };

            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var results = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<FactoryList>(results);
                    foreach (var factory in data.FactoryDetails)
                    {

                        Factorylists.Add(new FactoryModel { FactoryId = factory.FactoryId, FactoryName = factory.FactoryName });
                    }

                }
            }


        }

       
    }
}
