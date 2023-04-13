namespace Soapbox.Core.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class IListExtensions
    {
        public static int FindIndex<T>(this IList<T> source, Predicate<T> match)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (match(source[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
