using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WPFHelpers.Helpers
{
    public class DebugHelper
    {
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static void MethodInfo(string message, [CallerMemberName] string methodName = "default")
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine($"### {System.DateTime.Now.ToString("hh:mm:ss.fff")} {threadId} @ {methodName}: {message}");
        }

        public static void ExceptionInfo(Exception ex)
        {
            Debug.WriteLine("************* Exception Caught *************");
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
        }
    }
}
