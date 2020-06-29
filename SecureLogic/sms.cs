using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace SecureMonitor
{
    class Sms
    {
        //// Variables Declaration ///
        const string accountSid = "AC318e1a8bf4bb3987c8ed57e27f9f5bc3";
        const string DefaultAuthToken = "1a78a9835ce5fad26c00b76a7752dc9f";
        const string defaultMessagingServiceSid = "MGa08440691d29048510435be7b32b5416";
        string authToken = Properties.Settings.Default.authToken ?? DefaultAuthToken;


        /////////////////////////////////////////////////////
        // Send SMS powered by Twilio:
        //Arg1: recipient Phone Number (05X-1234567) <Recipient>
        //Arg2: Message of the SMS <Message>
        //Access Token is defined in CFG file
        //Arg: '/?' Shows all arguments
        //Written by Gonen Harel
        ////////////////////////////////////////////////////
        public bool SendSMS(string PhoneNumber, string MyMessage)
        {
            // Delete Error Log file
            if (File.Exists(Variables.errorLog))
                File.Delete(Variables.errorLog);

            AppendLog.LogFile(1, "<Main - >SendSMS.exe Started");

            if (PhoneNumber.Length > 9 && MyMessage.Length > 3)
            {
                try
                {
                    PhoneNumber = FixPhoneNumber(PhoneNumber);
                    TwilioClient.Init(accountSid, authToken);
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2

                    var message = MessageResource.Create(body: MyMessage,
                                                                    to: new Twilio.Types.PhoneNumber(PhoneNumber),
                                                                    messagingServiceSid: defaultMessagingServiceSid
                                                                    );
                    AppendLog.LogFile(1, "Message.Sid :" + message.Sid);
                    Console.WriteLine(message.Sid);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                    return false;
                }
            }
            else
            {
                AppendLog.LogFile(2, "Phone Number or Message is Empty - No SMS sent");
                Console.WriteLine("Warning: Phone Number or Message is Empty - No SMS Sent");
                return false;
            }
        }


//--------------------------------------------------------------------------//


        //public static string ReadSetting(string key, string DefaultValue)
        //{
        //    try
        //    {
        //        //var appSettings = ConfigurationManager.AppSettings;
        //        //string result = appSettings[key] ?? DefaultValue;
        //        string result = Properties.Settings.Default.PhoneNumbers ?? DefaultValue;
        //        Console.WriteLine(result);
        //        return result;
        //    }
        //    catch (ConfigurationErrorsException)
        //    {
        //        Console.WriteLine("Error reading app settings");
        //        //Default Token
        //        return DefaultValue;
        //    }
        //}

        public static string FixPhoneNumber(string Phone)
        {
            Phone = Phone.Replace("-", "");
            if (Phone.StartsWith("0"))
                Phone = "+972" + Phone.Substring(1);
            return Phone;
        }

    }
}
