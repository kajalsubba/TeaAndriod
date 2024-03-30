using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class SupplierModel
    {
        public int CollectionId { get; set; }
        public DateTime CollectionDate { get; set; }
        public string VehicleNo { get; set; }
        public long ClientId { get; set; }
        public long AccountId { get; set; }
        public int FineLeaf { get; set; }
        public long ChallanWeight { get; set; }
        public int Rate { get; set; }
        public int GrossAmount { get; set; }
        public int TripId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public int TenantId { get; set; }
        public int CreatedBy { get; set; }
    }

    public class SaveReturn
    {
        public long Id { get; set; }
        public string Message { get; set;}
    }
}
