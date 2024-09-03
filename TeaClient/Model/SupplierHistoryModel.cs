using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class SupplierHistoryModel
    {
        public string CollectionDate { get; set; }
        public string FactoryName { get; set; }
        public string VehicleNo { get; set; }
        public long FineLeaf { get; set; }
        public long ChallanWeight { get; set; }
        public decimal Rate { get; set; }
        public string RateStatus { get; set; }
        public decimal GrossAmount { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
    }
    public class SupplierList
    {
        public IList<SupplierHistoryModel> MobileData { get; set; }


    }
    public class SupplierSearchModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long TenantId { get; set; }
        public long ClientId { get; set; }
    
    }
}
