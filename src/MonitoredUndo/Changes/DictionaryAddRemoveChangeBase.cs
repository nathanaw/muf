using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace MonitoredUndo.Changes
{
    public abstract class DictionaryAddRemoveChangeBase : DictionaryChange
    {
        protected DictionaryAddRemoveChangeBase(object target, string propertyName, IDictionary collection, object key, object value)
            : base(target, propertyName, collection, new ChangeKey<object, string, object>(target, propertyName, key))
        {
            Key = key;
            Value = value;
        }

        public object Key { get; private set; }

        public object Value { get; private set; }

        public override void MergeWith(Change latestChange)
        {
            throw new NotImplementedException();
        }
    }
}
