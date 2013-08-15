using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Reflection;

namespace MonitoredUndo
{
    public static class DefaultChangeFactory
    {
        public static ChangeFactory Current
        {
            get { return _Current; }
            set { _Current = value; }
        }
        private static ChangeFactory _Current = new ChangeFactory();

        [Obsolete("Use instance method")]
        public static bool ThrowExceptionOnCollectionResets
        {
            get { return Current.ThrowExceptionOnCollectionResets; }
            set { Current.ThrowExceptionOnCollectionResets = value; }
        }

        [Obsolete("Use instance method")]
        public static Change GetChange(object instance, string propertyName, object oldValue, object newValue)
        {
            return Current.GetChange(instance, propertyName, oldValue, newValue);
        }

        [Obsolete("Use instance method")]
        public static void OnChanging(object instance, string propertyName, object oldValue, object newValue)
        {
            Current.OnChanging(instance, propertyName, oldValue, newValue);
        }

        [Obsolete("Use instance method")]
        public static void OnChanging(object instance, string propertyName, object oldValue, object newValue, string descriptionOfChange)
        {
            Current.OnChanging(instance, propertyName, oldValue, newValue, descriptionOfChange);
        }

        [Obsolete("Use instance method")]
        public static IList<Change> GetCollectionChange(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
        {
            return Current.GetCollectionChange(instance, propertyName, collection, e);
        }


        [Obsolete("Use instance method")]
        public static void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
        {
            Current.OnCollectionChanged(instance, propertyName, collection, e);
        }

        [Obsolete("Use instance method")]
        public static void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e, string descriptionOfChange)
        {
            Current.OnCollectionChanged(instance, propertyName, collection, e, descriptionOfChange);
        }
    }

}
