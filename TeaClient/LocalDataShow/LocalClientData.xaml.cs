using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaClient.Model;
using TeaClient.SQLLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeaClient.LocalDataShow
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LocalClientData : ContentPage
	{
        public SQLiteConnection conn;
        public LocalClientSaveModel _clientModel;
        public LocalClientData ()
		{
			InitializeComponent ();
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalClientSaveModel>();
            DisplayDetails();
        }

        public void DisplayDetails()
        {

            var details = (from x in conn.Table<LocalClientSaveModel>() select x).ToList();
            myListView.ItemsSource = details;
        }

    }
}