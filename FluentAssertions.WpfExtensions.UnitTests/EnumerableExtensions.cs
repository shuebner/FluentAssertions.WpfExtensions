using System;
using System.Collections;

namespace FluentAssertions.WpfExtensions.Internal
{
    static class EnumerableExtensions
    {
        public static bool SequenceEqual(this IEnumerable one, IEnumerable other)
        {
            if (one is null)
            {
                throw new ArgumentNullException(nameof(one));
            }

            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var oneEnumerator = one.GetEnumerator();
            var otherEnumerator = other.GetEnumerator();

            while (oneEnumerator.MoveNext())
            {
                if (otherEnumerator.MoveNext())
                {
                    if (!object.Equals(oneEnumerator.Current, otherEnumerator.Current))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (otherEnumerator.MoveNext())
            {
                return false;
            }

            return true;
        }
    }
}
