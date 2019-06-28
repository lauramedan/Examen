using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Examen.ViewModels
{
    public class UserGetModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        [EnumDataType(typeof(Models.UserRole))]
        public Models.UserRole UserRole { get; set; }
        public DateTime UserRoleStartDate { get; set; }

    }
}
