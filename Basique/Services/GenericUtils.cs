using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq.Expressions;

namespace Basique.Services
{
    public static class GenericUtils
    {
        static readonly Dictionary<Type, Func<int, Array>> arrayFactories = new();
        static readonly Dictionary<Type, Func<int, IList>> listFactories = new();

        public static IList MakeGenericList(ICollection from, Type type)
        {
            if (!listFactories.ContainsKey(type))
            {
                var intParam = Expression.Parameter(typeof(int));
                var listType = typeof(List<>).MakeGenericType(type);
                var ctor = listType.GetConstructor(new[] { typeof(int) });
                listFactories[type] = Expression.Lambda<Func<int, IList>>(
                    Expression.New(ctor, new[] { intParam }), false, new[] { intParam }
                ).Compile();
            }

            IList newList = listFactories[type](from.Count);
            foreach (var item in from)
            {
                newList.Add(item);
            }

            return newList;
        }
        public static Array MakeGenericArray(ICollection from, Type type)
        {
            if (!arrayFactories.ContainsKey(type))
            {
                var intParam = Expression.Parameter(typeof(int));
                arrayFactories[type] = Expression.Lambda<Func<int, Array>>(
                    Expression.NewArrayBounds(type, new[] { intParam }), false, new[] { intParam }
                ).Compile();
            }

            Array newArray = arrayFactories[type](from.Count);
            int i = 0;
            foreach (var item in from)
            {
                newArray.SetValue(item, i++);
            }

            return newArray;
        }
    }
}