using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Basique.Conversion
{
    public class StackConverter : IBasiqueConverter
    {
        public List<IBasiqueConverter> Stack = new();

        public bool TryConvert(object from, Type toType, [NotNullWhen(true)] out object to)
        {
            foreach (var conv in Stack.Reverse<IBasiqueConverter>())
            {
                if (conv.TryConvert(from, toType, out to))
                    return true;
            }
            to = default;
            return false;
        }
    }
}