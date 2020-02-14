using System;
using System.Windows;
using System.Runtime.InteropServices;

namespace µ.Core 
{

    public sealed class µCore
    {

        /// <summary>
        /// Creates a new core.
        /// </summary>
        public µCore()
        {
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);

        public static void µOutputDebugString(string message)
        {
            OutputDebugString(message);
        }
     }
}

