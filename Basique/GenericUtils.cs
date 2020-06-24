using System.Collections.Generic;
using System.Collections;
using System;

namespace Basique
{
    public static class GenericUtils
    {
        static Type listType = typeof(List<>);
        public static IList MakeGenericList(IEnumerable from, Type type)
        {
            Type newListType = listType.MakeGenericType(type);
            IList newList = (IList)Activator.CreateInstance(newListType);
            foreach (var item in from)
            {
                newList.Add(item);
            }

            return newList;
        }
        public static Array MakeGenericArray(IEnumerable from, Type type)
        {
            Type newArrayType = type.MakeArrayType();
            Array newArray = (Array)Activator.CreateInstance(newArrayType);
            foreach (var item in from)
            {
                ((IList)newArray).Add(item);
            }

            return newArray;
        }
    }
}