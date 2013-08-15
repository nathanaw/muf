using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PropertyChange : Change
    {

        #region Member Variables

        private readonly string _PropertyName;
        // both should be weak
        private readonly object _OldValue;
        private object _NewValue;

        #endregion

        #region Constructors

        public PropertyChange(object instance, string propertyName, object oldValue, object newValue)
            : base(instance, new ChangeKey<object, string>(instance, propertyName))
        {
            this._PropertyName = propertyName;
            this._OldValue = oldValue;
            this._NewValue = newValue;
        }

        #endregion


        #region Public Properties

        public string PropertyName
        {
            get { return _PropertyName; }
        }

        public object OldValue
        {
            get { return _OldValue; }
        }

        public object NewValue
        {
            get { return _NewValue; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// When consolidating events, we want to keep the original "Undo"
        /// but use the most recent Redo. This will pull the Redo from the 
        /// specified Change and apply it to this instance.
        /// </summary>
        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as PropertyChange;

            if (null != other)
                this._NewValue = other._NewValue;
        }

        #endregion

        #region Internal

        protected override void PerformUndo()
        {
            Target.GetType().GetProperty(PropertyName).SetValue(Target, OldValue, null);
        }

        protected override void PerformRedo()
        {
            Target.GetType().GetProperty(PropertyName).SetValue(Target, NewValue, null);
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    "PropertyChange(Property={0}, Target={{{1}}}, NewValue={{{2}}}, OldValue={{{3}}})",
                    PropertyName, Target, NewValue, OldValue);
            }
        }

        #endregion

    }


}
