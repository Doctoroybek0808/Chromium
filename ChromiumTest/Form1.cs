using CefSharp.WinForms;
using System;
using System.Windows.Forms;
using CefSharp;
using System.Threading.Tasks;
using System.Dynamic;
using System.Threading;
using System.IO;

namespace ChromiumTest
{



    public partial class Form1 : Form
    {
        public string signed;
        private FsDLL fsDLL = new FsDLL();
        private static WebServer webservers;
        public static ChromiumWebBrowser browser;
        Thread thrd;
        public Form1()
        {
            InitializeComponent();
            try
            {
                if (File.Exists(Environment.CurrentDirectory + "\\debug.log"))
                {
                    File.Delete(Environment.CurrentDirectory + "\\debug.log");
                }
                CefSharpSettings.LegacyJavascriptBindingEnabled = true;
                Cef.Initialize(new CefSettings());
                browser = new ChromiumWebBrowser("file:///" + Environment.CurrentDirectory + "//factura-send.html");
                 
                browser.JavascriptObjectRepository.NameConverter = null;

                browser.JavascriptObjectRepository.Register("FsDLL", fsDLL, isAsync: false, options: new BindingOptions());
                fsDLL.DataReady += WebServer.FsDLL_DataReady;
                browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;

                browser.RenderProcessMessageHandler = new RenderProcessMessageHandler();

                panel1.Controls.Add(browser);
                thrd = new Thread(GetCommand2);
                thrd.Start();

                contextMenuStrip = new ContextMenuStrip();
                contextMenuStrip.Items.Add("Restart");
                contextMenuStrip.Items.Add("Exit");
                contextMenuStrip.ItemClicked += this.menuStrip1_ItemClicked;
                contextMenuStrip.AutoClose = true;

                bool MousePointerOnTaskBar = Screen.GetWorkingArea(this).Contains(Cursor.Position);
                if (this.WindowState == FormWindowState.Minimized && MousePointerOnTaskBar)
                {
                    notifyIcon1.Icon = this.Icon;
                    this.ShowInTaskbar = false;
                    notifyIcon1.Visible = true;
                }
            }
            catch (Exception ex)
            {
                thrd.Abort();
                MessageBox.Show("Form1 " + ex.Message);
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {

                var item_text = e.ClickedItem.Text;

                string respond = string.Empty;
                this.ShowInTaskbar = false;
        
                if (e.ClickedItem.Text == "Restart")
                {
                    MessageBox.Show("Restart");
                }
                else if (e.ClickedItem.Text == "Exit")
                {
                    thrd.Abort();
                    Application.Exit();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("menuStrip1_ItemClicked " + ex.Message);
            }
        }
        public async void GetCommand2(object arg)
        {
            try
            {
                string[] prefixes = new string[] { "http://*:8808/" };
                webservers = new WebServer(prefixes);
                //WriteToFile("webservers " + prefixes[0]);
                webservers.run(prefixes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetCommand2 " + ex.Message);
            }
        }

        private void FsDLL_DataReady(object sender, string e)
        {
            MessageBox.Show("Data: " + e);
            Console.WriteLine(e);
        }

        private void Browser_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
            //browser.ShowDevTools();
        }

        public class RenderProcessMessageHandler : IRenderProcessMessageHandler
        {
            public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
            {
                const string script = "document.addEventListener('OnContextReleased', function(){ alert('OnContextReleased'); });";

                frame.ExecuteJavaScriptAsync(script);
            }

            public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
            {
                throw new NotImplementedException();
            }

            public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
            {
                throw new NotImplementedException();
            }

            // Wait for the underlying JavaScript Context to be created. This is only called for the main frame.
            // If the page has no JavaScript, no context will be created.
            void IRenderProcessMessageHandler.OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
            {
              //  const string script = "document.addEventListener('DOMContentLoaded', function(){ alert('DomLoaded'); });";

               // frame.ExecuteJavaScriptAsync(script);
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip.Show(MousePosition);
            }
        }
    }
}
