using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.ViewModel
{
   public class ChangePasswordModel
    {
        public string UserName { get; set; }

        public string LoginType { get; set; }
        public string Password { get; set; }
        public long TenantId { get; set; }

        public long CreatedBy { get; set; }
    }
}
