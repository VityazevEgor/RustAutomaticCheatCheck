using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Models
{
    public class RegistryModel
    {
        public string FilePath { get; set; }
        public int RunCount { get; set; }
        public bool stillExsist { get; set; }
    }
}
