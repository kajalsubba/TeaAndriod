using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using TeaClient.Model;
using Xamarin.Forms;

namespace TeaClient.ViewModel
{
    public class TenantViewModel
    {
        public IList<TenantList> TenantList { get; set; }

        public TenantViewModel()
        {
            TenantList = new ObservableCollection<TenantList>();
            GetTenant();
        }

        public async void GetTenant()
        {

            using (HttpClient client = new HttpClient())
            {
                string url = "http://72.167.37.70:94/Admin/GetTenant";

                HttpResponseMessage response = await client.GetAsync(url);
                var results = await response.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<TenantModel>(results);

                foreach (var tenant in data.TenantDetails)
                {
                    TenantList.Add(new TenantList { TenantId = tenant.TenantId, TenantName = tenant.TenantName });
                }

            }

           
        }
    }
}
