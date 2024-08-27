using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class StgSaveModel
    {
      
        public DateTime CollectionDate { get; set; }
        public int TripId { get; set; }
        public long ClientId { get; set; }
        public int FirstWeight { get; set; }
        public int WetLeaf { get; set; }
        public int LongLeaf { get; set; }
        public int Deduction { get; set; }
        public int FinalWeight { get; set; }
        public decimal Rate { get; set; }
        public int GradeId { get; set; }
        public string Remarks { get; set; }
        public string BagList { get; set; }
        public string CollectionType { get; set; }
        public long VehicleFrom { get; set; }
        public long TransferFrom { get; set; }
      
    }
    public class STGModelList
    {
        public long TenantId { get; set; }
        public long CreatedBy { get; set; }
        public string VehicleNo { get; set; }
        public string ServerComment { get; set; }
        public string Source { get; set; } = "Mobile";
        public string Category { get; set; } = "STG";
        public List<StgSaveModel> StgListData { get; set; }
    }
}
