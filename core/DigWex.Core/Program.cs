using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace DigWex
{
  class Program
  {
    #region fields
    private static Version _assemblyVersion;
    private static string _version;
    #endregion

    #region properties
    public static string Directory { get; private set; }

    public static Version AssemblyVersion => _assemblyVersion;

    public static string Version => _version;
    #endregion

    [STAThread]
    static void Main(string[] args)
    {
      CultureInfo culture = new CultureInfo("en-us");
      CultureInfo.DefaultThreadCurrentCulture = culture;
      CultureInfo.DefaultThreadCurrentUICulture = culture;
      Thread.CurrentThread.CurrentCulture = culture;

      var currentDomain = AppDomain.CurrentDomain;
      var assembly = Assembly.GetExecutingAssembly();

      Directory = currentDomain.BaseDirectory.Trim(new char[] { '\\', '/' });

      Console.WriteLine("---------------------------");

      _assemblyVersion = assembly.GetName().Version;
      _version = _assemblyVersion.ToString();

      //currentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
      currentDomain.UnhandledException += UnhandledException;
      TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

      //NativeMethods.SetDllDirectory(_libDirectory);
      //NativeMethods.SetErrorMode(ErrorModes.SEM_NOGPFAULTERRORBOX);
      //NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_DISPLAY_REQUIRED);

      Log.Instance.Init();
      Log.Info("Start v" + _version);


      int port = 8000;
      if (args.Length > 0) {
        int.TryParse(args[0], out port);
      }

      App.Run(port);
      Thread.Sleep(-1);
    }


    [SecurityCritical]
    [HandleProcessCorruptedStateExceptions]
    private static void UnhandledException(object sender,
   UnhandledExceptionEventArgs e)
    {
      var ex = (Exception)e.ExceptionObject;

      Type type = ex.GetType();

      int code = -1;
      if (type == typeof(FileNotFoundException) || type == typeof(DllNotFoundException) || type == typeof(FileLoadException))
        code = -2;
      Log.Fatal(ex);
      ExitApp(code);
    }

    //private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
    //{
    //  string assemblyPath = Path.Combine(_libDirectory, new AssemblyName(e.Name).Name + ".dll");

    //  if (!File.Exists(assemblyPath)) return null;
    //  return Assembly.LoadFrom(assemblyPath);
    //}

    private static void TaskScheduler_UnobservedTaskException(object sender,
    UnobservedTaskExceptionEventArgs e)
    {
      Log.Fatal(e.Exception);
      ExitApp(-1);
    }

    public static void ExitApp(int code)
    {
      Log.Info("Exit code:" + code);
      Environment.Exit(code);
    }
  }
}

