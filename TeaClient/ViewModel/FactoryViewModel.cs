using Android.Content.Res;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using TeaClient.Model;
using TeaClient.SessionHelper;

namespace TeaClient.ViewModel
{
    public class FactoryViewModel
    {
        public IList<FactoryModel> Factorylists { get; set; }
       
        
        public FactoryViewModel()
        {
            Factorylists = new ObservableCollection<FactoryModel>();
         
            GetFactory();
       
        }

        public async void GetFactory()
        {

            string url = "http://72.167.37.70:82/Master/GetFactory";

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
