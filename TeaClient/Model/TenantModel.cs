using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class TenantModel
    {

        public List<TenantList> TenantDetails { get; set; }
    }


    public class TenantList
    {
        public long TenantId { get; set; }
        public string TenantName { get; set; }

    }
  
    public class ClientLoginData
    {
        public List<ClientInfo> ClientLoginDetails { get; set; }

    }

    public class ClientInfo
    {
        public long? ClientId { get; set; }
        public string UserFirstName { get; set; }
        public string ClientLastName { get; set; }
        public long? TenantId { get; set; }
        public string CompanyName { get; set; }
        public string CategoryName { get; set; }
        public string RoleDescription { get; set; }
        public string LoginType { get; set; }
        public long? UserId { get; set; }
        public string ClientName { get; set; }
        public string ContactNo { get; set; }
    }
}
