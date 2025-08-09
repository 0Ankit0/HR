using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Shared.Models
{
    public class TwoFactorLoginRequest
    {

        public string TwoFactorCode { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public bool RememberMachine { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
