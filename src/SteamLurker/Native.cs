using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lurker.Steam
{
    internal class Native
    {
        [DllImport("Kernel32.dll")]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
    }
}
