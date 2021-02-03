using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace Basique.Services
{
    public static class GenericUtils
    {
        static readonly ConcurrentDictionary<Type, Func<int, Array>> arrayFactories = new();
        static readonly ConcurrentDictionary<Type, Func<int, IList>> listFactories = new();

        public static IList MakeGenericList(ICollection from, Type type)
        {
            IList newList = listFactories.GetOrAdd(type, _ =>
            {
                var intParam = Expression.Parameter(typeof(int));
                var listType = typeof(List<>).MakeGenericType(type);
                var ctor = listType.GetConstructor(new[] { typeof(int) });
                return Expression.Lambda<Func<int, IList>>(
                    Expression.New(ctor, new[] { intParam }), false, new[] { intParam }
                ).Compile();
            })(from.Count);

            foreach (var item in from)
            {
                newList.Add(item);
            }

            return newList;
        }
        public static Array MakeGenericArray(ICollection from, Type type)
        {
            Array newArray = arrayFactories.GetOrAdd(type, _ =>
            {
                var intParam = Expression.Parameter(typeof(int));
                return Expression.Lambda<Func<int, Array>>(
                    Expression.NewArrayBounds(type, new[] { intParam }), false, new[] { intParam }
                ).Compile();
            })(from.Count);

            int i = 0;
            foreach (var item in from)
            {
                newArray.SetValue(item, i++);
            }

            return newArray;
        }
    }
}