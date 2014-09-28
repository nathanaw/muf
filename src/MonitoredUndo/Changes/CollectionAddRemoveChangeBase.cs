using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    public abstract class CollectionAddRemoveChangeBase : CollectionChange
    {

        #region Member Variables

        protected object _RedoElement;
        protected int _RedoIndex;

        #endregion


        #region Constructors

        public CollectionAddRemoveChangeBase(object target, string propertyName, IList collection, int index, object element)
            : base(target, propertyName, collection,
                   new ChangeKey<object, string, object>(target, propertyName, element))
        {
            this.Element = element;
            this.Index = index;

            this._RedoElement = element;
            this._RedoIndex = index;
        }

        #endregion

        #region Public Properties

        public object Element { get; private set; }

        public int Index { get; private set; }

        #endregion

        #region Public Methods

        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as CollectionAddRemoveChangeBase;

            if (null != other)
            {
                this._RedoElement = other._RedoElement;
                this._RedoIndex = other._RedoIndex;
            }
        }

        #endregion

        #region Internal

        protected string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    "{4}(Property={0}, Target={{{1}}}, Value={2}, Key={{{3}}})",
                    PropertyName, Target, Index, Element, GetType().Name);
            }
        }

        #endregion
    }


}
