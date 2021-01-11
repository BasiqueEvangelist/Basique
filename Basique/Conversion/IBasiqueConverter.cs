using System;
using System.Diagnostics.CodeAnalysis;

namespace Basique.Conversion
{
    public interface IBasiqueConverter
    {
        bool TryConvert(object from, Type toType, [NotNullWhen(true)] out object to);
    }
}