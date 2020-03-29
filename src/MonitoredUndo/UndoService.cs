using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace MonitoredUndo
{

    public class UndoService
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

        

        

        private IDictionary<object, UndoRoot> _Roots;
        
        

        

        public UndoService()
        {
            _Roots = new Dictionary<object, UndoRoot>();
        }

        

        

        /// <summary>
        /// Get (or create) an UndoRoot for the specified object or document instance.
        /// </summary>
        /// <param name="root">The object that represents the root of the document or object hierarchy.</param>
        /// <returns>An UndoRoot instance for this object.</returns>
        public UndoRoot this[object root]
        {
            get
            {
                if (null == root)
                    return null;

                UndoRoot ret = null;

                if (_Roots.ContainsKey(root))
                    ret = _Roots[root];

                if (null == ret)
                {
                    ret = new UndoRoot(root);
                    _Roots.Add(root, ret);
                }

                return ret;
            }
        }

        

        
        
        /// <summary>
        /// Clear the cached UndoRoots.
        /// </summary>
        public void Clear()
        {
            this._Roots.Clear();
        }

        

    }

}
