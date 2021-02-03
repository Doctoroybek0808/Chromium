using CefSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChromiumTest
{
    public class WebServer
    {
        public static string state = "start";
        public static HttpListenerContext context;
        public static string res;
        public string[] prefixes;
        public WebServer(string[] prefixes)
        {
            this.prefixes = prefixes;
        }
        static void Perform(HttpListenerContext ctx, string message)
        {
            try
            {

                HttpListenerResponse response = ctx.Response;
                string responseString = message;
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                //response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // You must close the output stream.
                output.Close();
            }
            catch (Exception ex)
            {
                WriteToFileError("Perform " + ex.Message);
            }
        }
        public static void FsDLL_DataReady(object sender, string e)
        {
            context.Response.StatusCode = 200;
            Perform(context, e);
        }

        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }
        public async void run(string[] prefixes)
        {
            await ListenAsync(prefixes);
        }
        public async Task ListenAsync(string[] prefixes)
        {
            try
            {
                using (var listener = new HttpListener())
                {
                    if (!HttpListener.IsSupported)
                        throw new NotSupportedException(
                        "Needs Windows XP SP2, Server 2003 or later.");

                    if (prefixes == null || prefixes.Length == 0)
                        throw new ArgumentException("prefixes");


                    foreach (string s in prefixes)
                        listener.Prefixes.Add(s);

                    listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                    listener.Start();
                    bool listening = listener.IsListening;
                    if (listening)
                    {
                        while (listening)
                        {
                            try
                            {
                                HttpListenerContext context = await listener.GetContextAsync();
                                WebServer.context = context;
                                //HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)context.User.Identity;
                                //WriteToFile(identity.Name.ToString());
                                //WriteToFile(identity.Password.ToString());

                                //if (identity.Name == username && identity.Password == MD5Hash(MD5Hash(MD5Hash(password))))
                                //{
                                //await HandleRequestAsync(context);                            
                                HttpListenerRequest request = context.Request;
                                foreach (string key in request.QueryString.Keys)
                                {
                                    var content = request.QueryString.GetValues(key)[0];
                                    byte[] buffer = new byte[1024];

                                    WriteToFile("Content: " + content.ToString());
                                    if (content == "pause")
                                    {
                                        //WriteToFile("Pause fSync");
                                        WriteToFile("Content: " + content);
                                        state = "pause";
                                        context.Response.StatusCode = 200;
                                        Perform(context, "{\"status\":200,\"message\":\"" + content.ToString() + "\"}");
                                    }
                                    else if (content == "start")
                                    {
                                        var frame = Form1.browser.GetMainFrame();

                                        string text;
                                        using (var reader = new StreamReader(request.InputStream,
                                                                             request.ContentEncoding))
                                        {
                                            text = reader.ReadToEnd();
                                        }
                                        text = text.Replace("\n", "").Replace("\r", "");
                                        //var json = JsonConvert.SerializeObject(text, new JsonSerializerSettings { Formatting = Formatting.None }); ;

                                        string script = @"window.sign((data, json)  => FsDLL.dofunc(data), "+text+");";
                                       // Console.WriteLine(text);
                                        Task<JavascriptResponse> t = Form1.browser.EvaluateScriptAsPromiseAsync(script);
                                        t.Wait();
                                        state = "start";

                                        //context.Response.StatusCode = 200;
                                        //Perform(context, "{\"status\":200,\"message\":\"" + res + "\"}");
                                    }
                                    else if (content == "cert")
                                    {
                                        var frame = Form1.browser.GetMainFrame();

                                        string text;
                                        using (var reader = new StreamReader(request.InputStream,
                                                                             request.ContentEncoding))
                                        {
                                            text = reader.ReadToEnd();
                                        }
                                        text = text.Replace("\n", "").Replace("\r", "");
                                        //var json = JsonConvert.SerializeObject(text, new JsonSerializerSettings { Formatting = Formatting.None }); ;

                                        string script = @"window.getCert((data)  => FsDLL.dofunc(data));";
                                       // Console.WriteLine(text);
                                        Task<JavascriptResponse> t = Form1.browser.EvaluateScriptAsPromiseAsync(script);
                                        t.Wait();
                                        state = "start";

                                        //context.Response.StatusCode = 200;
                                        //Perform(context, "{\"status\":200,\"message\":\"" + res + "\"}");
                                    }

                                    else if (content == "attach")
                                    {
                                        var frame = Form1.browser.GetMainFrame();

                                        string text;
                                        using (var reader = new StreamReader(request.InputStream,
                                                                             request.ContentEncoding))
                                        {
                                            text = reader.ReadToEnd();
                                        }
                                        text = text.Replace("\n", "").Replace("\r", "");
                                        //var json = JsonConvert.SerializeObject(text, new JsonSerializerSettings { Formatting = Formatting.None }); ;

                                        string script = @"window.signAttach((data, json)  => FsDLL.dofunc(data), " + text + ");";
                                       // Console.WriteLine(text);
                                        Task<JavascriptResponse> t = Form1.browser.EvaluateScriptAsPromiseAsync(script);
                                        t.Wait();
                                        state = "start";

                                        //context.Response.StatusCode = 200;
                                        //Perform(context, "{\"status\":200,\"message\":\"" + res + "\"}");
                                    }

                                    else if (content == "decode")
                                    {
                                        var frame = Form1.browser.GetMainFrame();

                                        string text;
                                        using (var reader = new StreamReader(request.InputStream,
                                                                             request.ContentEncoding))
                                        {
                                            text = reader.ReadToEnd();
                                        }
                                        text = text.Replace("\n", "").Replace("\r", "");
                                        //var json = JsonConvert.SerializeObject(text, new JsonSerializerSettings { Formatting = Formatting.None }); ;

                                        string script = @"window.signDecode((data, json)  => FsDLL.dofunc(data), " + text + ");";
                                        //Console.WriteLine(text);
                                        Task<JavascriptResponse> t = Form1.browser.EvaluateScriptAsPromiseAsync(script);
                                        t.Wait();
                                        state = "start";

                                        //context.Response.StatusCode = 200;
                                        //Perform(context, "{\"status\":200,\"message\":\"" + res + "\"}");
                                    }

                                    //else
                                    //{
                                    //    context.Response.StatusCode = 200;
                                    //    Perform(context, "{\"status\":200,\"message\":\"" + content.ToString() + "\"}");

                                    //}
                                }



                                //}
                                //else
                                //{
                                //    context.Response.StatusCode = 404;
                                //    Perform(context, "{\"status\":404,\"message\":\"Wrong password or login\"}");
                                //}

                            }
                            catch (HttpListenerException ex)
                            {
                                if (ex.ErrorCode == 995)
                                {
                                    WriteToFileError("HttpListenerException " + ex.Message);
                                }
                            }
                            catch (SystemException ex)
                            {
                                WriteToFileError("SystemException " + ex.Source + " " + ex.TargetSite + " " + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(new Exception().Message);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFileError("SystemException " + ex.Source + " " + ex.TargetSite + " " + ex.Message);
            }
        }
        private static void WriteToFile(string text)
        {
            string path = Environment.CurrentDirectory + "\\test.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(text + " " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                writer.Close();
            }
        }
        private static void WriteToFile2(string text)
        {
            string path = Environment.CurrentDirectory + "\\query.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(text);
                writer.Close();
            }
        }
        private static void WriteToFileError(string text)
        {
            string path = Environment.CurrentDirectory + "\\respond.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(text + " " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                writer.Close();
            }
        }
        public void Stop()
        {
        }
    }
}
