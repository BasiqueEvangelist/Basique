using System;
using Basique.Solve;

namespace Basique
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoConstantFoldingAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MethodWriterAttribute : Attribute
    {
        public IMethodWriter MethodWriter { get; set; }

        public MethodWriterAttribute(Type methodWriterType)
        {
            if (!typeof(IMethodWriter).IsAssignableFrom(methodWriterType))
                throw new InvalidOperationException("Method writer must implement IMethodWriter!");

            MethodWriter = (IMethodWriter)Activator.CreateInstance(methodWriterType);
        }
    }
}