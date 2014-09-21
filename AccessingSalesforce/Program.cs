using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using svcRef = AccessingSalesforce.ServiceReference1;
using webRef = AccessingSalesforce.WebReference;

namespace AccessingSalesforce
{
    class Program
    {
        public const String subject = "Test Case";
        public const String description = "testing";
        public const String accountId = "001d000001UsLAX";
        public const String priority = "Medium";
        public const String status = "New";
        public const String username = "user@salesforce.com";
        public const String password = "passwordAndToken";
        public const String consumerKey = "";
        public const String consumerSecret = "";


        static void Main(string[] args)
        {
            NewCaseViaSvcRef();  //1st SvcRef call
            NewCaseViaWebRef();  //1st Webref call
            NewCaseViaSvcRef();  //2nd SvcRef call
            NewCaseViaWebRef();  //2nd Webref call
            
            // pause for user input
            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();
        }

        static void NewCaseViaWebRef()
        {
            webRef.SforceService sfBinding = null;
            webRef.LoginResult lr = null;
            sfBinding = new webRef.SforceService();

            //Time how long it takes 
            Stopwatch sp = new Stopwatch();
            sp.Start();

            //login to Salesforce
            try
            {
                lr = sfBinding.login(username, password);
            }
            catch (Exception exc)
            {
                sfBinding = null;
                Console.WriteLine("The following error message was received while trying to connect: " + exc.Message);
                    
                return;
            }
            //Establish the session
            sfBinding.Url = lr.serverUrl;
            sfBinding.SessionHeaderValue = new webRef.SessionHeader();
            sfBinding.SessionHeaderValue.sessionId = lr.sessionId;

            //Create a new case
            webRef.Case sfCase = new webRef.Case();
            sfCase.Subject = subject;
            sfCase.Description = description;
            sfCase.AccountId = accountId;
            sfCase.Priority = priority;
            sfCase.Status = status;
            try
            {
                webRef.SaveResult[] res = sfBinding.create(new webRef.sObject[] { sfCase });
                //Get the results
                if (res[0].success)
                {
                    Console.WriteLine("The following case was saved sucessfully: "
                        + res[0].id.ToString());
                }
                else
                {
                    Console.WriteLine("The following error was received: "
                        + res[0].errors[0].message);
                    return;
                }
            }
            catch (Exception exc)
            {
                sfBinding = null;
                Console.WriteLine("The following error message was received while trying to create a case: "
                    + exc.Message);
                return;
            }


            //Stop timer and get elapsed time
            sp.Stop();
            TimeSpan ts = sp.Elapsed;

            //Display the Results of timer
            Console.WriteLine("WebRef Runtime: "
                + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes,
                    ts.Seconds, ts.Milliseconds / 10));

        }

         static void NewCaseViaSvcRef()
         {
             EndpointAddress epAddr = null;
             svcRef.LoginResult lr = null;
             svcRef.SessionHeader header = null;

             //Time how long it takes 
             Stopwatch sp = new Stopwatch();
             sp.Start();

             using (svcRef.SoapClient client = new svcRef.SoapClient("Soap"))
             {
                 try
                 {

                     lr = client.login(
                         null,                  //LoginScopeHeader
                         username,              //username      
                         password);      //password - Token is included if authenticating from outside Trusted IP Range

                     //save URL in the epaddress and the session id in the header
                     epAddr = new EndpointAddress(lr.serverUrl);
                     header = new svcRef.SessionHeader();
                     header.sessionId = lr.sessionId;

                 }
                 catch (Exception exc)
                 {
                     Console.WriteLine("The following error message was received while trying to connect: " + exc.Message);
                 }
             }

             

             using (svcRef.SoapClient queryClient = new svcRef.SoapClient("Soap", epAddr))
             {
                 //Create a new case
                 svcRef.Case sfCase = new svcRef.Case();
                 sfCase.Subject = subject;
                 sfCase.Description = description;
                 sfCase.AccountId = accountId;
                 sfCase.Priority = priority;
                 sfCase.Status = status;

                 try
                 {

                     svcRef.SaveResult[] results;
                     svcRef.LimitInfo[] info;
                     queryClient.create(
                             header,
                             null,    //assignmentruleheader
                             null,    //mruheader
                             null,    //allowfieldtruncationheader
                             null,    //disablefieldtrackingheader
                             null,    //streamingenabledheader
                             null,    //allornoneheader
                             null,    //debuggingheader
                             null,    //packageversionheader
                             null,    //emailheader
                             new svcRef.sObject[] { sfCase },
                             out info,
                             out results
                          );

                     if (results[0].success)
                     {
                         Console.WriteLine("The following case was saved sucessfully: "
                         + results[0].id.ToString());
                     }
                     else
                     {
                         Console.WriteLine("The following error was received: "
                         + results[0].errors[0].message);
                     }

                 }
                 catch (Exception exc)
                 {
                     Console.WriteLine("The following error message was received while trying to create a case: " + exc.Message);
                 }
             }
             

             //Stop timer and get elapsed time
             sp.Stop();
             TimeSpan ts = sp.Elapsed;

             //Display the Results of timer
             Console.WriteLine("SvcRef Runtime: "
                 + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                     ts.Hours, ts.Minutes,
                     ts.Seconds, ts.Milliseconds / 10));

         }
    }
}
