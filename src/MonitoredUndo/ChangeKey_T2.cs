using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace MonitoredUndo
{

    /// <summary>
    /// Used to uniquely identify a change that has a 2-part "key".
    /// </summary>
    public class ChangeKey<T1, T2>
    {
        private T1 m_One;
        private T2 m_Two;

        public T1 Item1 { get { return m_One; } }
        public T2 Item2 { get { return m_Two; } }

        public ChangeKey(T1 item1, T2 item2)
        {
            m_One = item1;
            m_Two = item2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            ChangeKey<T1, T2> tuple = obj as ChangeKey<T1, T2>;
            if (tuple == null)
            {
                return false;
            }
            if (object.Equals(this.m_One, tuple.m_One))
            {
                return object.Equals(this.m_Two, tuple.m_Two);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return CombineHashCodes(m_One.GetHashCode(), m_Two.GetHashCode());
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Tuple of '{0}', '{1}'", m_One, m_Two);
        }


        internal static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }

    }


}
