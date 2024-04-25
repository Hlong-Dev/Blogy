// File: UserRoleUpdateModel.cs
using System.Collections.Generic;

namespace DoAnCoSo2.Models
{
    public class UserRoleUpdateModel
    {
        public string UserId { get; set; }
        public string NewRole { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
