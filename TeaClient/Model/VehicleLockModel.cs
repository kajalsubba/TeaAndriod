using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class VehicleLockModel
    {
     
           
            public long VehicleId { get; set; }
            public int TripId { get; set; }
            public DateTime LockDate { get; set; }
            public long TenantId { get; set; }
            public long CreatedBy { get; set; }
        
    }

    public class SaveVehicleLockModel
    {
        public string VehicleNo { get; set; }
        public int TripId { get; set; }
        public string LockDate { get; set; }
        public long TenantId { get; set; }
        public long CreatedBy { get; set; }

    }

    public class VehicleLockList
    {
        public List<VehicleLockModel> LockDetails { get; set; }
    }
}
