using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionAddChange : CollectionAddRemoveChangeBase
    {
        
        


        
        public CollectionAddChange(object target, string propertyName, IList collection, int index, object element)
            : base(target, propertyName, collection, index, element) { }

        

        
        protected override void PerformUndo()
        {
            Collection.Remove(Element);
        }

        protected override void PerformRedo()
        {
            Collection.Insert(_RedoIndex, _RedoElement);
        }

        
    }


}
