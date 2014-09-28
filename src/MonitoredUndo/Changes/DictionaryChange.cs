using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitoredUndo.Changes
{
    public abstract class DictionaryChange : Change
    {
                #region Member Variables

        private readonly IDictionary _Collection;
        private readonly string _PropertyName;

        #endregion

        #region Constructors

        protected DictionaryChange(object target, string propertyName, IDictionary collection, object changeKey)
            : base(target, changeKey)
        {
            this._PropertyName = propertyName;
            this._Collection = collection;
        }

        #endregion

        #region Properties

        public IDictionary Collection
        {
            get { return _Collection; }
        }

        public string PropertyName
        {
            get { return _PropertyName; }
        }

        #endregion

    }
}
