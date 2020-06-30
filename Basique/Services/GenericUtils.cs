using System.Collections.Generic;
using System.Collections;
using System;

namespace Basique.Services
{
    public static class GenericUtils
    {
        static Type listType = typeof(List<>);
        public static IList MakeGenericList(ICollection from, Type type)
        {
            Type newListType = listType.MakeGenericType(type);
            IList newList = (IList)Activator.CreateInstance(newListType);
            foreach (var item in from)
            {
                newList.Add(item);
            }

            return newList;
        }
        public static Array MakeGenericArray(ICollection from, Type type)
        {
            Type newArrayType = type.MakeArrayType();
            Array newArray = (Array)Activator.CreateInstance(newArrayType, new object[] { from.Count });
            int i = 0;
            foreach (var item in from)
            {
                newArray.SetValue(item, i++);
            }

            return newArray;
        }
    }
}