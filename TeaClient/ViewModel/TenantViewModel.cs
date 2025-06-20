using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using TeaClient.Model;
using TeaClient.Services;
using Xamarin.Forms;

namespace TeaClient.ViewModel
{
    public class TenantViewModel
    {
      
        public IList<TenantList> TenantList { get; set; }
        AppSettings _appSetting = AppConfigService.GetConfig();



        public TenantViewModel()
        {
            TenantList = new ObservableCollection<TenantList>();
            GetTenant();
        }

        public async void GetTenant()
        {
            try
            {

                using (HttpClient client = new HttpClient())
                {
                    string url = _appSetting.ApiUrl + "Admin/GetTenant";

                    HttpResponseMessage response = await client.GetAsync(url);
                    var results = await response.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<TenantModel>(results);

                    foreach (var tenant in data.TenantDetails)
                    {
                        TenantList.Add(new TenantList { TenantId = tenant.TenantId, TenantName = tenant.TenantName });
                    }

                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            finally { 
            
            }

           
        }
    }
}
