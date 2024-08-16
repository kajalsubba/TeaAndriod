using Java.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;

namespace TeaClient.ViewModel
{
    public class TripViewModel
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<TripModel> Triplists { get; set; }
        UserModel LoginData = new UserModel();

        public TripViewModel()
        {
            Triplists = new ObservableCollection<TripModel>();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            GetTrip();
        }
        public async void GetTrip()
        {
            try
            {
                string url = _appSetting.ApiUrl + "Master/GetTrip";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    var results = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TripModelList>(results);
                    foreach (var trip in data.TripDetails)
                    {

                        Triplists.Add(new TripModel { TripId = trip.TripId, TripName = trip.TripName });
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {

            }


        }
    }
}
