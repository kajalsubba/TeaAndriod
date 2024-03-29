using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TeaClient.Model
{
    public class FactoryModel
    {

        public long FactoryId { get; set; }
        public string FactoryName { get; set; }

    }

    public class FactoryList
    {
        public IList<FactoryModel> FactoryDetails { get; set; }
       

    }

    public class FactoryAccountList
    {
        public IList<FactoryAccountModel> AccountDetails { get; set; }


    }

    public class FactoryAccountModel
    {

        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public long FactoryId { get; set; }

    }
}
