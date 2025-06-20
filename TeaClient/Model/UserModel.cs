using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.Model
{
    public class UserModel
    {
        public List<UserLoginModel> LoginDetails { get; set; }
    }

    public class UserLoginModel
    {
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public int UserRoleId { get; set; }
        public string RoleDescription { get; set; }
        public string RoleName { get; set; }
        public int TenantId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string UserEmail { get; set; }
        public string ContactNo { get; set; }
        public string LoginType { get; set; }
    }
}
