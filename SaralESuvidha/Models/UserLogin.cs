using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Mobile number is required")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string LoginPassword { get; set; }

    }
}
