using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitoredUndo.Changes
{
    public class DictionaryRemoveChange : DictionaryAddRemoveChangeBase
    {
        public DictionaryRemoveChange(object target, string propertyName, IDictionary collection, object key, object value) 
            : base(target, propertyName, collection, key, value)
        {
        }

        protected override void PerformUndo()
        {
            Collection.Add(Key, Value);
        }

        protected override void PerformRedo()
        {
            Collection.Remove(Key);
        }
    }
}
