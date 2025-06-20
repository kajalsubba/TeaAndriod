using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class ClientModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
    }
    public class ClientList
    {
        public List<ClientModel> ClientDetails { get; set; }
    }

    public class LocalClientSaveModel
    {
        //[PrimaryKey, AutoIncrement]
        //public long Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
