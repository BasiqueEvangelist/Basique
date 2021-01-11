using System;
using System.Diagnostics.CodeAnalysis;

namespace Basique.Conversion
{
    public class NullableWrapConverter : IBasiqueConverter
    {
        private readonly IBasiqueConverter wraps;

        public NullableWrapConverter(IBasiqueConverter wraps)
        {
            this.wraps = wraps;
        }

        public bool TryConvert(object from, Type toType, [NotNullWhen(true)] out object to)
        {
            if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nullableWraps = toType.GetGenericArguments()[0];
                if (from == null || from is DBNull)
                {
                    to = Activator.CreateInstance(toType);
                    return true;
                }
                else
                {
                    if (wraps.TryConvert(from, nullableWraps, out var wrap))
                    {
                        to = Activator.CreateInstance(toType, wrap);
                        return true;
                    }
                }
            }
            to = default;
            return false;
        }
    }
}