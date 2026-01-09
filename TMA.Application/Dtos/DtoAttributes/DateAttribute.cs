using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMA.Application.Dtos.DtoAttributes
{
    internal class DateAttribute : BaseAttribute
    {
        public override object Parse(string value) => DateTime.Parse(value);
    }
}
