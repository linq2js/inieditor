using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniEditor
{
    public static class ObjectUtils
    {
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dic)
        {
            return dic.ToDictionary(x => x.Key, x => x.Value, dic.Comparer);
        }
    }
}
