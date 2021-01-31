using System;
using System.Threading.Tasks;

namespace Basique.Services
{
    public class DisposePredicate : IAsyncDisposable, IDisposable
    {
        public object Wrapping { get; }
        public bool WillDispose { get; }

        public DisposePredicate(object of, bool dispose)
        {
            Wrapping = of;
            WillDispose = dispose;
        }

        public void Dispose()
        {
            if (WillDispose)
                if (Wrapping is IDisposable disp)
                    disp.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            if (WillDispose)
                if (Wrapping is IAsyncDisposable disp)
                    return disp.DisposeAsync();
            return default;
        }
    }
}