using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarnationVariableSectionPart
{
    interface IParameterMonitor
    {
        internal float LastEvaluatedTime { get; set; }
        internal List<object> OldValues { get; set; }
        internal bool ValueChangedInCurrentFrame { get; set; }
    }
}