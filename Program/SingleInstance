using System;
using System.Threading;

namespace Flithor_ReusableCodes
{
    /// <summary>
    /// Singleton helpper class. Call <see cref="InstanceExists"/> check any running instance exists
    /// </summary>
    public static class SingleInstance
    {
        private static readonly string SINGLETON_ID = GetAssebmlyName();
        private static Mutex singletonMutex = new Mutex(true, SINGLETON_ID);
        /// <summary>
        /// Is any running instance exists
        /// </summary>
        public static bool InstanceExists => GetInstanceExists();

        private static bool GetInstanceExists()
        {
            if (singletonMutex.WaitOne(TimeSpan.Zero, true))
            {
                singletonMutex.ReleaseMutex();
                return false;
            }
            return true;
        }
        private static string GetAssebmlyName()
        {
            var assembly = typeof(SingleInstance).Assembly;
            return assembly.GetName().Name;
        }
    }
}
