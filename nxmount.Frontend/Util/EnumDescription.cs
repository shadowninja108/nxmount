using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Frontend.Util
{
    public record EnumDescription
    {
        public object Value { get; set; }

        public string Description { get; set; }

        public string Help { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}
