using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class SaveLocalCollectionModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime CollectionDate { get; set; }
        public int TripId { get; set; }
        public string VehicleNo { get; set; }

        public long ClientId { get; set; }

        public string ClientName{ get; set; }
        public int FirstWeight { get; set; }
        public int WetLeaf { get; set; }
        public int LongLeaf { get; set; }
        public int Deduction { get; set; }
        public int FinalWeight { get; set; }
        public decimal Rate { get; set; }

        public decimal GrossAmount { get; set; }
        public int GradeId { get; set; }

        public string GradeName { get; set; }
        public string Remarks { get; set; }
        public long TenantId { get; set; }
        public string Status { get; set; }

        public string BagList { get; set; }

        public long CreatedBy { get; set; }

        public bool IsEdit { get; set; } = false;
        public string CollectionType { get; set; } = "Self";
        public long TransferFrom { get; set; }
        public long VehicleFrom { get; set; }

        public bool DataSendToServer { get; set; }=false;

    }



}
