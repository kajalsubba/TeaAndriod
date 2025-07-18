﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.CommunityToolkit.Converters;

namespace TeaClient.Model
{
    public class VehicleLockModel
    {
     
           
        public long VehicleId { get; set; }
        public int TripId { get; set; }

        public string VehicleNo { get; set; }

        public string TripName { get; set; }    
        public DateTime LockDate { get; set; }
        public long TenantId { get; set; }
        public long TransferTo { get; set; }
        public long TransferFrom { get; set; }
        public long VehicleFrom { get; set; }

        public string LoginUserName { get; set; }
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
