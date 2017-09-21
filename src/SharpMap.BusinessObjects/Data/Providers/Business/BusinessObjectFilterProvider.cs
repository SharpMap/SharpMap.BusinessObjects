using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMap.Data.Providers.Business
{
    public abstract class BusinessObjectFilterProvider
    {
        public delegate bool FilterMethod(object bo);
        public FilterMethod FilterDelegate { get; set; }
    }
}
