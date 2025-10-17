using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
namespace Services
{
    
    public class Coroutine2
    {
        private readonly AsyncContextThread contextThread;
        private readonly TaskFactory factory;
        public static Coroutine2 Instance { get; private set; } = new Coroutine2(new AsyncContextThread());
        public static Coroutine2 Instance2 { get; private set; } = new Coroutine2(new AsyncContextThread());
        //[SetsRequiredMembers]
        public Coroutine2(AsyncContextThread ct)
        {
            contextThread = ct;

            if (contextThread == null)
                contextThread = new AsyncContextThread();
             
            
            factory = contextThread.Factory;

        }
        public long ActiveCount { get; private set; }

        public Task Run(Action action)
        {
            var task = factory.Run(action);
            ActiveCount++;
            task.ContinueWith((t) =>
            {
                ActiveCount--;
                if (t.IsFaulted)
                    Logging.Error("Coroutine.Run", t.Exception + "");
            });
            return task;
        }

        public Task Run(Func<Task> action)
        {
            var task = factory.Run(action);
            ActiveCount++;
            task.ContinueWith((t) =>
            {
                ActiveCount--;
                if (t.IsFaulted)
                    Logging.Error("Coroutine.Run", t.Exception + "");
            });
            return task;
        }

        public Task<T> Run<T>(Func<Task<T>> action)
        {
            var task = factory.Run(action);
            ActiveCount++;
            task.ContinueWith((t) =>
            {
                ActiveCount--;
                if (t.IsFaulted)
                    Logging.Error("Coroutine.Run", t.Exception + "");
            });
            return task;
        }

        public Task<T> Run<T>(Func<T> action)
        {
            var task = factory.Run(action);
            ActiveCount++;
            task.ContinueWith((t) =>
            {
                ActiveCount--;
                if (t.IsFaulted)
                    Logging.Error("Coroutine.Run", t.Exception + "");
            });
            return task;
        }
    }


}
