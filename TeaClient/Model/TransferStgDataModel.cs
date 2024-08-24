using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class TransferStgDataModel
    {
        public DateTime? CollectionDate { get; set; }
        public int? TripId { get; set; }
        public long? ClientId { get; set; }
        public int? FirstWeight { get; set; }
        public int? WetLeaf { get; set; }
        public int? LongLeaf { get; set; }
        public int? Deduction { get; set; }
        public int? FinalWeight { get; set; }
        public decimal? Rate { get; set; }
        public int? GradeId { get; set; }
        public string Remarks { get; set; }

        public string BagList { get; set; }
    }
    public class TransferStgList
    {
        public long? TenantId { get; set; }
        public long? TransferBy { get; set; }
        public long? TransferTo { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleTo { get; set; }
        public string Category { get; set; } = "Transfer";
        public List<TransferStgDataModel> StgTransferData { get; set; }
    }

    public class GetTransferDataModel
    {
        public string VehicleNo { get; set; }
        public int TripId { get; set; }
        public String CollectionDate { get; set; }
        public long TenantId { get; set; }
        public long CreatedBy { get; set; }
    }

    public class TransferDataList
    {
        public List<SaveLocalCollectionModel> TransferData{ get; set; }
    }
}
