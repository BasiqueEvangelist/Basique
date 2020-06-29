using System;
using System.Linq;
using NLog;
using Perfusion;

namespace Basique
{
    public class LogInfo : ObjectInfo
    {
        [Inject] LogFactory factory;
        public override ObjectInfo Clone() => new LogInfo();

        public override object GetInstance(IContainer c, Type requester = null)
        {
            return factory.GetLogger(requester != null ? getTypeName(requester) : "<type unspecified>");
        }
        private string getTypeName(Type t)
        {
            return (t.DeclaringType == null ? t.Namespace + "." + t.Name : getTypeName(t.DeclaringType) + "." + t.Name)
            + (t.GenericTypeArguments.Length > 0 ? "<" + string.Join(",", t.GenericTypeArguments.Select(getTypeName)) + ">" : "");
        }
    }
}