using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Chat.Shared
{
    public static class Time
    {
        public static T Measure<T>(Func<T> action, out TimeSpan span)
        {
            var startNew = Stopwatch.StartNew();
            var result = action();
            startNew.Stop();
            span = startNew.Elapsed;

            return result;
        }

        public static async Task<TimeSpan> Measure(Func<Task> action)
        {
            var startNew = Stopwatch.StartNew();
            await action();
            startNew.Stop();
            return startNew.Elapsed;
        }

        public static async Task<TimeSpan> Measure<T>(Func<Task<T>> action)
        {
            var startNew = Stopwatch.StartNew();
            await action();
            startNew.Stop();
            return startNew.Elapsed;
        }
    }
}