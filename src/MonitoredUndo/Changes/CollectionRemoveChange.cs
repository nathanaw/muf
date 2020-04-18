using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionRemoveChange : CollectionAddRemoveChangeBase
    {
        

        public CollectionRemoveChange(object target, string propertyName, IList collection, int index, object element)
            : base(target, propertyName, collection, index, element) { }

        

        

        protected override void PerformUndo()
        {
            Collection.Insert(Index, Element);
        }

        protected override void PerformRedo()
        {
            Collection.Remove(_RedoElement);
        }

        
    }


}
