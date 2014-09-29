using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitoredUndo.Changes
{
    public class DictionaryReplaceChange : DictionaryChange
    {
        /// <summary>
        /// Change type for a Dictionary replace action.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="propertyName"></param>
        /// <param name="collection"></param>
        /// <param name="key">Dictionary key</param>
        /// <param name="oldValue">Old Dictionary value</param>
        /// <param name="newValue">New Dictionary value</param>
        public DictionaryReplaceChange(object target, string propertyName, IDictionary collection, object key, object oldValue, object newValue) 
            : base(target, propertyName, collection, new ChangeKey<object, string, object>(target, propertyName, new ChangeKey<object, object>(oldValue, newValue)))
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object Key { get; set; }

        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public override void MergeWith(Change latestChange)
        {
            throw new NotImplementedException();
        }

        protected override void PerformUndo()
        {
            Collection[Key] = OldValue;
        }

        protected override void PerformRedo()
        {
            Collection[Key] = NewValue;
        }
    }
}
