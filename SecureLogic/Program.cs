using System;
using System.Linq;
//using System.Drawing;
//using System.Threading;
//using System.Threading.Tasks;
using System.Reflection;

namespace SecureMonitor
{
    public static class Variables
	{
		public static string errorLog = AppDomain.CurrentDomain.BaseDirectory + "SendSMS.log";
		public static string ExePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
		public static bool DeleteLogFlag = false;
		public const string API_ID = "SmartPrinter";
		public const string API_CODE = "1o9LWIlLDtbab2jPZmzAop5Fm";
	}

	static class Program
	{
		[STAThread]
		static void Main()
		{
			AppendLog.LogFile(1, "<Main Program Started");

			// Copy Temp File to checker folder
			string targetPath = Variables.ExePath + "SignChecker\\";
			string DestFile = CopyFile(targetPath);
			string[] MyArgsArr = { DestFile , "spsmonitor" , "Ss1234!" , "110", "670", "50", "50", "0", "1", "I Approve", "", Variables.ExePath, "digitally-signed4.jpg",
					"1", "30000", "5", "http://seclog1.smartprinter.co.il:443/", "http://seclog2.smartprinter.co.il:443/"};

			string resault = SecureSign.SignDoc(MyArgsArr);
			string[] sm = SMSMessagebuilder(resault);
						
			if (int.Parse(sm[0]) > 2000)
            {
				AppendLog.LogFile(3, "Fail to sign document");
				Console.WriteLine("Fail to sign document");
				// Error found in signature //
				Sms MyMessage = new Sms();
				string [] PhoneNumbers = (Properties.Settings.Default.PhoneNumbers ?? "052-3721307,052-5403334,054-9330933").Split(',');
				string MyNum;
				foreach (string Num in PhoneNumbers)
				{
					MyNum = Sms.FixPhoneNumber(Num);
					if (MyMessage.SendSMS(MyNum, sm[1]))
					{
						AppendLog.LogFile(1, "Sent Error SMS message");
						Console.WriteLine("Sent Error SMS message");
					}
					else
					{
						AppendLog.LogFile(1, "Problem with sending SMS message");
						Console.WriteLine("Problem with sending SMS message");

					}
				}
				

			}

		}
// ----------------------------------------------------------------------------------------------------------------
		private static string CopyFile(string targetPath)
        {
			string fileName = "Sample.pdf";
			string sourcePath = Variables.ExePath;
			
			string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
			string destFile = System.IO.Path.Combine(targetPath, fileName);

			System.IO.Directory.CreateDirectory(targetPath);

			// overwrite the destination file if it already exists.
			System.IO.File.Copy(sourceFile, destFile, true);
			AppendLog.LogFile(1, "File:" + destFile + ", copied to checker destination");
			return destFile;
		}
		private static string [] SMSMessagebuilder(string resault)
        {
			string[] sm = resault.Split(',');
						
			sm[1] = "התגלתה תקלה בשרתי החתימה של Secure Logic\n  תיאור התקלה:\n"  + sm[1].Trim();

			return sm;
        }
	}
		
}
