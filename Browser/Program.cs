using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Browser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //private static void LoadApp()
        //{
        //    //Perform dependency check to make sure all relevant resources are in our output directory.
        //    var settings = new CefSettings();

        //    Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

        //    var browser = new BrowserForm();
        //    Application.Run(browser);
        //}

        //// Will attempt to load missing assembly from either x86 or x64 subdir
        //private static Assembly Resolver(object sender, ResolveEventArgs args)
        //{
        //    if (args.Name.StartsWith("CefSharp"))
        //    {
        //        string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
        //        string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
        //            Environment.Is64BitProcess ? "x64" : "x86",
        //            assemblyName);

        //        return File.Exists(archSpecificPath)
        //            ? Assembly.LoadFile(archSpecificPath)
        //            : null;
        //    }

        //    return null;
        //}
    }
}
