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
	public partial class LocalGradeData : ContentPage
	{

        public SQLiteConnection conn;
        public LocalGradeSaveModel _gradeModel;

        public LocalGradeData ()
		{
			InitializeComponent ();
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<LocalGradeSaveModel>();
            DisplayDetails();
        }

        public void DisplayDetails()
        {

            var details = (from x in conn.Table<LocalGradeSaveModel>() select x).ToList();
            myListView.ItemsSource = details;
        }
    }
}