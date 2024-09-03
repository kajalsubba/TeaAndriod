using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class StgHistoryModel
    {
        public int SrlNo { get; set; }
        public int CollectionId { get; set; }
        public string CollectionDate { get; set; }
        public string VehicleNo { get; set; }
        public string ClientName { get; set; }
        public long FirstWeight { get; set; }
        public long Deduction { get; set; }
        public long FinalWeight { get; set; }
        public string GradeName { get; set; }
        public string Status { get; set; }
        public string ViewBag { get; set; }
        public string Remarks { get; set; }
        public decimal GrossAmount { get; set; }
        public string CollectionType { get; set; }
        public string TransferFrom { get; set; }
        public string VehicleFrom { get; set; }
        public string CreatedBy { get; set; }
  
    }

    public class StgHistoryList
    {
        public List<StgHistoryModel> STGDetails { get; set; }
    }
    public class StgFilterRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TenantId { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
    }
}
