using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.DTO.ResponseModel
{
    public class UserAddressResModel
    {
    }
    public class ListUserAddressResModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string Address { get; set; } = null!;
        
        public string Ward { get; set; } = null!;
       
        public string District { get; set; } = null!;

        public string Province { get; set; } = null!;

        public bool? IsDefault { get; set; }
    }
}
