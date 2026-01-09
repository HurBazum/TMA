using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMA.UI.Infrastructure
{
    public class FilterGroup
    {
        public string Header { get; set; } = null!;
        public List<FilterOption> Options { get; set; } = [];
    }
}
