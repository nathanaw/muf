using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace MonitoredUndo
{
    /// <inheritdoc cref="IUndoService"/>
    public class UndoService : IUndoService
    {

        private static UndoService _Current;
        private static IDictionary<Type, WeakReference> _CurrentRootInstances;

        /// <summary>
        /// Get (or create) the singleton instance of the UndoService.
        /// </summary>
        public static UndoService Current
        {
            get
            {
                if (null == _Current)
                    _Current = new UndoService();

                return _Current;
            }
        }

        /// <summary>
        /// Stores the "Current Instance" of a given object or document so that the rest of the model can access it.
        /// </summary>
        /// <typeparam name="T">The type of the root instance to store.</typeparam>
        /// <param name="instance">The document or object instance that is the "currently active" instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static object GetCurrentDocumentInstance<T>() where T : class
        {
            if (null == _CurrentRootInstances)
                return null;

            var type = typeof(T);
            if (_CurrentRootInstances.ContainsKey(type))
            {
                var wr = _CurrentRootInstances[type];

                if (null == wr || !wr.IsAlive)
                {
                    _CurrentRootInstances.Remove(type);
                    return null;
                }

                return wr.Target;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Stores the "Current Instance" of a given object or document so that the rest of the model can access it.
        /// </summary>
        /// <typeparam name="T">The type of the root instance to store.</typeparam>
        /// <param name="instance">The document or object instance that is the "currently active" instance.</param>
        public static void SetCurrentDocumentInstance<T>(T instance) where T : class
        {
            var type = typeof(T);

            if (null == _CurrentRootInstances)
            {
                if (null != instance)   // The instance can be null if it is being cleared.
                {
                    _CurrentRootInstances = new Dictionary<Type, WeakReference>();
                    _CurrentRootInstances.Add(type, new WeakReference(instance));
                }
            }
            else
            {
                var existing = GetCurrentDocumentInstance<T>();

                if (null == existing && null != instance)
                    _CurrentRootInstances.Add(type, new WeakReference(instance));
                else if (null != instance)
                    _CurrentRootInstances[type] = new WeakReference(instance);
                else
                    _CurrentRootInstances.Remove(type);
            }
        }



        // Use a weak reference for the key, to prevent memory leaks.
        private IDictionary<WeakReference, UndoRoot> _Roots;


        public UndoService()
        {
            // Use a custom comparer to compare the WeakReference keys.
            _Roots = new Dictionary<WeakReference, UndoRoot>(new WeakReferenceComparer());
        }




        /// <inheritdoc cref="IUndoService.this[object]"/>
        public UndoRoot this[object root]
        {
            get
            {
                if (null == root)
                    return null;

                UndoRoot ret = null;
                WeakReference wRef = new WeakReference(root);

                if (_Roots.ContainsKey(wRef))
                    ret = _Roots[wRef];

                if (null == ret)
                {
                    ret = new UndoRoot(root);
                    _Roots.Add(wRef, ret);
                }

                return ret;
            }
        }

        /// <inheritdoc cref="IUndoService.Clear"/>
        public void Clear()
        {
            this._Roots.Clear();
        }

    }

}
