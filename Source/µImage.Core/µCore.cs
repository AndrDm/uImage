using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

