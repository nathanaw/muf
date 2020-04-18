using System;
using System.Collections.Generic;

namespace MonitoredUndo
{

    /// <summary>
    /// Compares to weak references to determine if the point to the same instance.
    /// Returns false if either are no longer alive.
    /// </summary>
    public class WeakReferenceComparer : IEqualityComparer<WeakReference>
    {

        /// <summary>
        /// Compare two WeakReferences for reference equality
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Right</param>
        /// <returns>True if both alive, and point to same reference via object.ReferenceEquals().</returns>
        public bool Equals(WeakReference x, WeakReference y)
        {
            object left = x?.Target;
            object right = y?.Target;

            if (left is object && right is object && x.IsAlive && y.IsAlive)
            {
                return object.ReferenceEquals(left, right);
            }

            return false;
        }

        /// <summary>
        /// Returns the hashcode of the reference, if alive. Otherwise zero.
        /// </summary>
        /// <param name="obj">The weak reference.</param>
        /// <returns>The hashcode of the reference, if alive. Otherwise zero.</returns>
        public int GetHashCode(WeakReference obj)
        {
            if (obj is null) return 0;

            object target = obj.Target;
            if (target is object && obj.IsAlive) // aka not null
            {
                return target.GetHashCode();
            }

            return 0;
        }
    }

}
