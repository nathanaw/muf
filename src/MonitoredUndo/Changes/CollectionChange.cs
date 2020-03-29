using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    // TODO collection changes need more tests

    public abstract class CollectionChange : Change
    {
        

        private readonly IList _Collection;
        private readonly string _PropertyName;

        

        

        protected CollectionChange(object target, string propertyName, IList collection, object changeKey)
            : base(target, changeKey)
        {
            this._PropertyName = propertyName;
            this._Collection = collection;
        }

        

        

        public IList Collection
        {
            get { return _Collection; }
        }

        public string PropertyName
        {
            get { return _PropertyName; }
        }

        
    }


}
