using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tabber
{
    internal static class ReflectionHelper
    {
        internal static IEnumerable<object> EachValues(object o)
        {
            Type type = o.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo propInfo in type.GetProperties(flags)
                                      .Where(w => w.CanRead && w.CanWrite &&
                                                  !w.GetIndexParameters().Any()))
            {
                yield return propInfo.GetValue(o, null);
            }
        }
        internal static IEnumerable<PropertyInfo> EachProps(object o)
        {
            Type type = o.GetType();
            return EachProps(type);
        }
        internal static IEnumerable<PropertyInfo> EachProps(Type type)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (PropertyInfo propInfo in type.GetProperties(flags)
                                      .Where(w => w.CanRead && w.CanWrite &&
                                                  !w.GetIndexParameters().Any()))
            {
                yield return propInfo;
            }
        }
    }
}
