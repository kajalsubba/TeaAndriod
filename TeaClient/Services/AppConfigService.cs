using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TeaClient.EventHelper;

namespace TeaClient.Services
{
    public class AppConfigService
    {
        private readonly string _configFilePath = "appsettings.json";

        public AppConfigModel GetConfig()
        {
            var assembly = typeof(AppConfigService).GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(_configFilePath))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    AppConfigModel config = JsonConvert.DeserializeObject<AppConfigModel>(json);
                    return config;
                }
            }
            //string json = File.ReadAllText(_configFilePath);
            //AppConfigModel config = JsonConvert.DeserializeObject<AppConfigModel>(json);
            //return config;
        }
    }
}
