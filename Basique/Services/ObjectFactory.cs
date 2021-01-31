using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Basique.Solve;

namespace Basique.Services
{
    public static class ObjectFactory
    {
        public static object Create(Type type, PathTree<object> values)
        {
            object instance;
            if (values.WalkValues().All(x => x.Key.CanFollowType(type)))
            {
                var ctor = type.GetConstructors().FirstOrDefault(x => x.IsPublic && x.GetParameters().Length == 0);
                if (ctor != null)
                {
                    instance = ctor.Invoke(Array.Empty<object>());
                    foreach (var (path, obj) in values.WalkValues())
                    {
                        path.Set(instance, obj);
                    }
                    return instance;
                }
            }

            instance = TryCreateAnonymous(type, values);
            if (instance != null) return instance;

            throw new NotImplementedException();
        }

        private static object TryCreateAnonymous(Type type, PathTree<object> values)
        {
            var firstFoundConstructor = type.GetConstructors()[0];
            var properties = type.GetProperties();
            var arguments = firstFoundConstructor.GetParameters();
            var origArgs = new ParameterInfo[arguments.Length];
            Array.Copy(arguments, origArgs, arguments.Length);

            if (properties.Length != arguments.Length) return null;

            Array.Sort(properties, Comparer<PropertyInfo>.Create((a, b) =>
            {
                var compare = StringComparer.InvariantCulture.Compare(a.PropertyType.AssemblyQualifiedName, b.PropertyType.AssemblyQualifiedName);
                if (compare == 0)
                    compare = StringComparer.InvariantCulture.Compare(a.Name, b.Name);
                return compare;
            }));
            Array.Sort(arguments, Comparer<ParameterInfo>.Create((a, b) =>
            {
                var compare = StringComparer.InvariantCulture.Compare(a.ParameterType.AssemblyQualifiedName, b.ParameterType.AssemblyQualifiedName);
                if (compare == 0)
                    compare = StringComparer.InvariantCulture.Compare(a.Name, b.Name);
                return compare;
            }));

            for (int i = 0; i < properties.Length && i < arguments.Length; i++)
            {
                if (arguments[i].ParameterType.AssemblyQualifiedName != properties[i].PropertyType.AssemblyQualifiedName) return null;
                if (arguments[i].Name != properties[i].Name) return null;
            }

            object[] neededArgs = new object[arguments.Length];

            foreach (var (member, obj) in values)
            {
                if (member is not PropertyInfo prop) return null;

                int indexSorted = Array.IndexOf(properties, prop);
                int indexNeeded = Array.IndexOf(origArgs, arguments[indexSorted]);
                if (obj.IsTree)
                {
                    neededArgs[indexNeeded] = Create(prop.PropertyType, obj.Tree);
                }
                else
                    neededArgs[indexNeeded] = obj.Value;
            }
            return firstFoundConstructor.Invoke(neededArgs);
        }
    }
}