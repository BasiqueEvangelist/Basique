using System;
using System.Diagnostics.CodeAnalysis;

namespace Basique.Conversion
{
    public class ConvertConverter : IBasiqueConverter
    {
        public bool TryConvert(object from, Type toType, [NotNullWhen(true)] out object to)
        {
            try
            {
                to = Convert.ChangeType(from, toType);
                return true;
            }
            catch (InvalidCastException)
            {
                to = default;
                return false;
            }
        }
    }
}