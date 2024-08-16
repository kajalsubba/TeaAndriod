using Java.Net;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using TeaClient.Model;
using TeaClient.Services;
using TeaClient.SessionHelper;
using TeaClient.SQLLite;
using Xamarin.Forms;

namespace TeaClient.ViewModel
{
   public class GradeViewModel
    {
        AppSettings _appSetting = AppConfigService.GetConfig();
        public IList<GradeModel> GradeList { get; set; }
        UserModel LoginData = new UserModel();
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;

        public GradeViewModel()
        {
            GradeList = new ObservableCollection<GradeModel>();
            LoginData = SessionManager.GetSessionValue<UserModel>("UserDetails");
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalGradeSaveModel>();
       
            GetGradeFromLocalDB();
         
        }
        public void GetGradeFromLocalDB()
        {
            var GradeDetails = (from x in conn.Table<LocalGradeSaveModel>() select x).ToList();
            foreach (var _grade in GradeDetails)
            {
                GradeList.Add(new GradeModel { GradeId = _grade.GradeId, GradeName = _grade.GradeName });

            }
        }
        public async void GetGrade()
        {
            try
            {
                string url = _appSetting.ApiUrl + "Master/GetGrade";

                var dataToSend = new
                {
                    IsClientView = true,
                    TenantId =Convert.ToInt32(LoginData.LoginDetails[0].TenantId)
                };

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(dataToSend);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    var results = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var data = JsonConvert.DeserializeObject<GradeList>(results);
                        foreach (var _grade in data.GradeDetails)
                        {

                            GradeList.Add(new GradeModel { GradeId = _grade.GradeId, GradeName = _grade.GradeName });
                        }

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
