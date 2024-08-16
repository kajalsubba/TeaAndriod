using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class DailyCollectionView : ContentPage
	{
        public SQLiteConnection conn;
        public SaveLocalCollectionModel _CollectionModel;

        public IList<SaveLocalCollectionModel> dataItems { get; set; }
        public DailyCollectionView ()
		{
			InitializeComponent ();
            dataItems = new ObservableCollection<SaveLocalCollectionModel>();
            BindingContext = this;
            conn = DependencyService.Get<ISqlLite>().GetConnection();
            conn.CreateTable<SaveLocalCollectionModel>();
            CollectionData();
        }

        public void CollectionData()
        {

            var CollectionDetails = (from x in conn.Table<SaveLocalCollectionModel>() select x).ToList();

            foreach (var _collect in CollectionDetails)
            {

                dataItems.Add(new SaveLocalCollectionModel
                {
                    ClientName = _collect.ClientName,
                    GradeName=_collect.GradeName,
                    FirstWeight = _collect.FinalWeight,
                    Deduction = _collect.Deduction,
                    FinalWeight = _collect.FinalWeight,
                    Rate = _collect.Rate,
                    GrossAmount = _collect.GrossAmount,
                    Remarks = _collect.Remarks,
                   
                });
            }
        }
    }
}