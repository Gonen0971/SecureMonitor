using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Win32;
using System.Configuration;

namespace SecureMonitor
{
    public class Utils
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);


       


        public bool TestEmailRegex(string emailAddress)
        {

            string patternStrict = @"^(([^<>()[\]\\.,;:\s@""]+"
                + @"(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@"
                + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                + @"[a-zA-Z]{2,}))$";

            Regex reStrict = new Regex(patternStrict);

            bool isStrictMatch = reStrict.IsMatch(emailAddress);
            return isStrictMatch;
        }

        public class IniFile
        {
            [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
            private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

            // Declare the unmanaged functions.
            [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
            private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);



            //The path of the file we are operating on.
            private static string m_path;
            public const int MaxSectionSize = 32767; // 32 KB
            public static string sDefault = "";
            public static StringBuilder buffer = new StringBuilder(MaxSectionSize);

            public IniFile(string path)
            {
                //Convert to the full path.  Because of backward compatibility, 
                // the win32 functions tend to assume the path should be the 
                // root Windows directory if it is not specified.  By calling 
                // GetFullPath, we make sure we are always passing the full path
                // the win32 functions.
               // m_path = System.IO.Path.GetFullPath(path);
            }

            public void WriteValue(string sectionName, string keyName, string value)
            {
                // Write a new value.
                WritePrivateProfileString(sectionName, keyName, value, m_path);
            }
            public static string ReadValue(string sectionName, string keyName,string  path)
            {
                m_path = System.IO.Path.GetFullPath(path);
                if (GetPrivateProfileString(sectionName, keyName, sDefault, buffer, MaxSectionSize, m_path) != 0)
                {
                    return Utils.IniFile.buffer.ToString();
                }
                else
                {
                    return "";
                }
            }
        }



        public static string ReadRegKey(string RegKey)
        {
            if (RegKey != "Winsert")
            {
                string keyname = "Software\\SPS\\" + RegKey + "\\secure\\";
                if (Utils.Is64Bit())
                    keyname = "Software\\WOW6432Node\\SPS\\" + RegKey + "\\secure\\";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyname))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("code00", "");
                        if (o != null)
                            return Convert.ToString(o);
                    }
                }
            }
            else
            {
                string keyname = "Software\\SPS\\" + RegKey;
                if (Utils.Is64Bit())
                    keyname = "Software\\WOW6432Node\\SPS\\" + RegKey;

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyname))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("License", "");
                        if (o != null)
                            return Convert.ToString(o);
                    }
                }
            }
            return ("");

        }





        public static bool Is64Bit()
        {
            if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Is32BitProcessOn64BitProcessor()
        {
            bool retVal;

            IsWow64Process(System.Diagnostics.Process.GetCurrentProcess().Handle, out retVal);

            return retVal;
        }

        public string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                Console.WriteLine(result);
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
                return null;
            }
        }


        public static bool registryValueExists(string hive_HKLM_or_HKCU, string registryRoot, string valueName)
        {
            RegistryKey root;
            switch (hive_HKLM_or_HKCU.ToUpper())
            {
                case "HKLM":
                    root = Registry.LocalMachine.OpenSubKey(registryRoot, false);
                    break;
                case "HKCU":
                    root = Registry.CurrentUser.OpenSubKey(registryRoot, false);
                    break;
                default:
                    throw new System.InvalidOperationException("parameter registryRoot must be either \"HKLM\" or \"HKCU\"");
            }
            if (root != null)
            {
                var x = root.GetValue(valueName);
                if (x != null)
                    return true;
                else return false;
            }
            // return root.GetValue(valueName) != null;
            else
                return false;
        }

        public static bool registryKeyExists(string hive_HKLM_or_HKCU, string registryRoot)
        {
            RegistryKey root;
            switch (hive_HKLM_or_HKCU.ToUpper())
            {
                case "HKLM":
                    root = Registry.LocalMachine.OpenSubKey(registryRoot); //, false);
                    if (root == null)
                        return false;
                    else
                        return true;

                case "HKCU":
                    root = Registry.CurrentUser.OpenSubKey(registryRoot); //, false);
                    if (root == null)
                        return false;
                    else
                        return true;
                default:
                    throw new System.InvalidOperationException("parameter registryRoot must be either \"HKLM\" or \"HKCU\"");
            }

        }


        public static string GetRegistrykey(string RegValueName)
        {
            string keyname = "Software\\SPS\\Smart Focal Point\\Settings\\";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyname))
            {
                if (key != null)
                {
                    Object o = key.GetValue(RegValueName, "");
                    if (o != null)
                        if (Convert.ToString(o) != "default")
                            return Convert.ToString(o);
                        else return ("");

                }
            }
            return ("");
        }
    }    
 }



