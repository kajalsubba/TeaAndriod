using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TeaClient.EventHelper;
using TeaClient.Model;

namespace TeaClient.Services
{
    public static class AppConfigService
    {

        public static AppSettings GetConfig()
        {
            try {

                var assembly = typeof(AppSettings).GetTypeInfo().Assembly;
                //  using (Stream stream = assembly.GetManifestResourceStream("TeaClient.Configuration.appsettings.release.json"))
                using (Stream stream = assembly.GetManifestResourceStream("TeaClient.Configuration.appsettings.debug.json"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        AppSettings config = JsonConvert.DeserializeObject<AppSettings>(json);
                        return config;
                    }
                }

            }
            catch ( Exception ex)
            {
                return null;
            }
         }
        

    }
}
