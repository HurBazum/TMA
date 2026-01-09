using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMA.Application.Dtos.DtoAttributes
{
    public abstract class BaseAttribute : Attribute
    {
        public abstract object Parse(string value);
    }
}
