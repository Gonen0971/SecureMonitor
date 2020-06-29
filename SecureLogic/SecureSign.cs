using com.slg.prosigner.api;
using com.slg.prosigner.api.utils;
//using System.Threading;
//using System.Threading.Tasks;
using iTextSharp.text.pdf;
using Secure.Logic.Utils;
using System;
using System.IO;

namespace SecureMonitor
{
	class SecureSign
	{
		public static string SignDoc(string[] myargs)
		{
			string szPdfPath = myargs[0];
			int resaultCode = 0;
			string resaultStr = "";
			//PdfDocument doc = PdfReader.Open(szPdfPath);
			if (!File.Exists(szPdfPath))
            {
				AppendLog.LogFile(2, "SignDoc -- File: " + szPdfPath + " Not found!");
				Console.WriteLine("SignDoc -- File: " + szPdfPath + " Not found!");
			}
			else
			{ 
				PdfReader reader = new PdfReader(szPdfPath);
			
				double nPageWidth = 595;
				double nPageHeight = 842;

				string szUser = myargs[1];
				string szPassword = myargs[2];

				double nLeft = Convert.ToDouble(myargs[3]);
				double nTop = Convert.ToDouble(myargs[4]);
				double nWidth = Convert.ToDouble(myargs[5]);
				double nHeight = Convert.ToDouble(myargs[6]);

				bool bRelative = myargs[7] == "1" ? true : false;
				//bool bLastPage = myargs[8]=="1" ? true : false;
				int nPage = Convert.ToInt32(myargs[8]);

				string sPage = "";

				switch (nPage)
				{
					case 1:
						sPage = "first";
						break;
					case 0:
						sPage = "last";
						break;
					case -1:
						sPage = "all";
						break;
					default:
						sPage = "first";
						break;
				}

				if (nPage == 0)
				{
					int pages = reader.NumberOfPages;
					iTextSharp.text.Rectangle rect = reader.GetPageSize(pages - 1);
					nPageWidth = rect.Width;
					nPageHeight = rect.Height;
					//nPageWidth = doc.Pages[doc.Pages.Count - 1].Width;
					//nPageHeight = doc.Pages[doc.Pages.Count - 1].Height;
				}
				else
				{
					iTextSharp.text.Rectangle rect = reader.GetPageSize(1);
					//nPageWidth = doc.Pages[0].Width;
					//nPageHeight = doc.Pages[0].Height;
					nPageWidth = rect.Width;
					nPageHeight = rect.Height;
				}

				reader.Close();

				if (bRelative)
				{
					double k = nPageHeight / 841;

					nLeft *= k;
					nTop *= k;
					nWidth *= k;
					nHeight *= k;
				}

				string szReason = myargs[9];
				string szLocation = myargs[10];
				string szBy = myargs[11];
				string szImagePath = myargs[12];
				bool bImageOnly = myargs[13] == "1" ? true : false;
				Int32 nTimeout = Convert.ToInt32(myargs[14]);
				Int32 nTries = Convert.ToInt32(myargs[15]);

				//This is the entry point of 
				string szPrimURL = myargs[16];
				string szSecURL = myargs[17];
				string token = null;

				//PSSignerAPI api = new PSSignerAPI(WEB_SERVER_URL);
				PSSignerAPI api = new PSSignerAPI(szPrimURL, szSecURL);//,nTimeout);

				try
				{


					// Optional - Call to check status 
					APIStatusResult sr = api.Status();
					if (sr.IsSuccess())
					{
						Console.WriteLine("Service is UP , status: " + sr.status + ", Version: " + sr.version);
					}
					else
					{
						//HandleErrorResult(sr.);
						Console.WriteLine(sr.innerErrorMessage);
						resaultCode = 2002;
						resaultStr = sr.innerErrorMessage;
						return (resaultCode.ToString() + "," + resaultStr);
					}


					//Define the signature format, if null is send then the default settings will be read from the server account
					SigFormat sigFormat = new SigFormat();

					sigFormat.Contact = szBy;// "Contact";
					sigFormat.Reason = szReason;// "Reason";
												//sigFormat.sigpage = bLastPage ? "last":sigFormat.sigpage = "first"; // values "first" or "last"(default) or "all"
					sigFormat.sigpage = sPage;
					sigFormat.Location = szLocation;// "IL";
					byte[] data = File.ReadAllBytes(szImagePath);// @"c:\temp\signature.jpg");
					sigFormat.ImageBase64 = Convert.ToBase64String(data);
					sigFormat.ImageOnly = bImageOnly;//Convert.ToInt32(myargs[13]);

					//This is sample settings for Lower Left  
					sigFormat.xCordinate = 20;
					sigFormat.yCordinate = 150;
					sigFormat.imageHeight = 75;
					sigFormat.imageWidth = 75;


					string temp = null;
					byte[] buffer = File.ReadAllBytes(szPdfPath);

					/*Before calling the signing code application must call the login method. 
					 * After successfull login, the result contains a session key that needs 
					 * to be pass in every call to the API
					 */

					//Call to API login, this is needs to be done only once!
					//APIResult res = api.Login(APPLICATION_ID, APPLICATION_KEY);

					//APIExtendedResult 
					APILoginResult res = api.Login(Variables.API_ID, Variables.API_CODE);

					//always check result! 
					if (!res.IsSuccess())
					{
						//this.HandleErrorResult(loginResult);
						//return;
						resaultStr = "API Login Failure ";
						Console.WriteLine(resaultStr);
						resaultCode = 2000;
						return (resaultCode.ToString() + "," + resaultStr); ;
					}
					// Get the token to be used for further calls to the API 
					token = res.token;

					//Open signing session - this is recommended in case of multiple signatures 
					APIOpenSessionResult OpenSessionRes = api.OpenSession(token, szUser, //The user ID to sign the file
																		szPassword // The pin for the user. 
						);

					//always check result! 
					if (!OpenSessionRes.IsSuccess())
					{
						resaultStr = "OpenSession Failure ";
						Console.WriteLine(resaultStr);
						resaultCode = 2003;
						return (resaultCode.ToString() + "," + resaultStr); 
					}

					byte[] pdfFileContent = null;
					pdfFileContent = File.ReadAllBytes(szPdfPath);
					bool bSigned = false;
					for (int i = 0; i < nTries; i++)
					{

						//Call the sign funtion 
						APISignResult apiRes = null;
						apiRes = api.Sign(token, new FileInfo(szPdfPath).Name, pdfFileContent, szUser, "0", szPassword, OPERATION_MODE.LOCAL_SIGN, sigFormat);

						//always check result! 
						if (apiRes.IsSuccess())
						{
							File.WriteAllBytes(szPdfPath, apiRes.buffer);
							//Console.WriteLine("Sign OK");
							Console.WriteLine("File signed and saved to: " + szPdfPath);
							bSigned = true;
							break;
						}
						else
						{
							System.Threading.Thread.Sleep(nTimeout);
							//Console.WriteLine("Sign Failure ");
							//this.HandleErrorResult(apiRes);
							//Console.WriteLine("Unknown Error: Unable to sign file");
							//return 2001;
						}

					}

					if (!bSigned)
					{
						resaultCode = 2001;
						resaultStr = "Unknown Error: Unable to sign file";
						Console.WriteLine(resaultStr);
						return (resaultCode.ToString() + "," + resaultStr);
					}

					resaultCode = 0;
					resaultStr = "success signing document";
					return (resaultCode.ToString() + "," + resaultStr);
				}
				catch (APIException ex)
				{
					Console.WriteLine(Message(ex));
					resaultCode = (Int32)ex.HResult;// SpecificCode;//-2;
					resaultStr = ex.ToString();
					return (resaultCode.ToString() + "," + resaultStr);

				}

				catch (Exception ex)
				{
					resaultCode = 2006;
					resaultStr = "Unknown Error: " + ex.Message;
					Console.WriteLine(resaultStr);
					return (resaultCode.ToString() + "," + ex.HResult);
				}

				finally
				{
					//Close session - no signatures for this session will be allowd 
					if (resaultCode != 2002)
						api.CloseSession(token, szUser);
					else
					{
						resaultCode = 2002;
						resaultStr = "Timeout Error Serer is unavailable- could not sign document";
						Console.WriteLine(resaultStr);
						resaultStr = resaultCode.ToString() + "," + resaultStr;
					}
					
					//Close session -invalidate the token and clean cach 
					APILogoutResult logoutRes = api.Logout(token);
					if (!logoutRes.IsSuccess())
					{
						Console.WriteLine("Logout Failure ");
						//this.HandleErrorResult(logoutRes);
						//return 2005;
					}
					
				}
				// -------------------------------------------------------------------------
			}
			resaultCode = 2008;
			return resaultCode.ToString();	

		}
		static string Message(APIException ex)
		{
			switch (ex.HResult)//SpecificCode)
			{
				case (int)ErrorCodes.SPECIFIC_CODES.API_UNABLE_TO_CONNECT_TO_SERVER:
					return "Unable to connect to server";

				case (int)ErrorCodes.SPECIFIC_CODES.API_AUTHENTICATION_INVALID_CRYPTOUSER_CREDENTIALS:
					return "Wrong User ID";

				case (int)ErrorCodes.SPECIFIC_CODES.INPUT_VALIDATION_USER_PIN_LENGTH_TOO_SHORT:
				case (int)ErrorCodes.SPECIFIC_CODES.INPUT_VALIDATION_NO_COMPLEXITY_NO_SPECIAL_CHAR:
				case (int)ErrorCodes.SPECIFIC_CODES.USER_AUTHENTICATION_WRONG_PIN:
					return "Wrong Password";
				case (int)ErrorCodes.SPECIFIC_CODES.GENERAL_ERROR:
					//if (ex.ErrorExtendedInformation != "")
					//	return ex.ErrorExtendedInformation;
					//else
					return "General Error";
				default:
					return ex.ToString();
			}


		}
		
	}
}
