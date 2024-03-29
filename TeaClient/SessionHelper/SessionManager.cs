using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TeaClient.SessionHelper
{
    public static class SessionManager
    {
        public static void SetSessionValue(string key, object value)
        {
            Application.Current.Properties[key] = value;
        }

        public static T GetSessionValue<T>(string key)
        {
            if (Application.Current.Properties.ContainsKey(key))
            {
                return (T)Application.Current.Properties[key];
            }

            return default(T);
        }

        public static void ClearSession()
        {
            Application.Current.Properties.Clear();
        }
    }
}
