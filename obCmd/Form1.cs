using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string warn = null;
        string crit = null;
        int exitcode = 0;

        public Form1(string[] args)
        {            
            
            InitializeComponent();
            //Console.WriteLine(args[0]);
            this.Hide();
            //timer1.Enabled = true;
            OSL.RxIsAvailable += new EventHandler(OSL_RxIsAvailable);
            OSL.LinkChannel = 10;
            OSL.LinkName = "oBcMD";
            if (args.Length == 0){
                Console.WriteLine("Warning: No object reference given");
                exitcode = 1;
                timer1.Enabled = true;
                return;
            }
            foreach (string arg in args)
            {
                if (arg.Contains("="))
                {
                    string[] dat = arg.Split('=');
                    if (dat[0].ToLower() == "warn")
                    {
                        warn = dat[1];
                    }
                    if (dat[0].ToLower() == "crit")
                    {
                        crit = dat[1];
                    }
                }
            }
            if (doRequest(args[0]) == false){
                Console.WriteLine("Critical: Can't connect to Obsys");
                exitcode = 2;
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            OSL.Dispose();
            Environment.Exit(exitcode);

        }

        private bool doRequest(string obref)
        {
            if (OSL.TxAvailable == true)
            {
                OSL.TxCommand = 0;
                OSL.TxReference = 1000;
                OSL.TxObject = obref;
                OSL.TxValue = "";
                OSL.TxValueLen = 0;
                OSL.TxFailCode = 0;
                OSL.TxAvailable = false;
                return true;
            }
            else
            {
                return false;
            }
        }


        private void OSL_RxIsAvailable(object sender, EventArgs e)
        {
            string value;            
            try
            {
                //Debug.WriteLine("rxisavailable");                
                if (OSL.RxAvailable == true)
                {
                    if (OSL.RxReference == 1000)
                    {
                        if (OSL.RxFailCode <= 127)
                        {
                            value = OSL.RxValue;
                            value = value.Replace("|", "/");
                            value = value + " | " + OSL.TxObject + "=" + value+";";
                            if (warn != null && OSL.RxValue.Contains(warn))
                            {
                                Console.WriteLine("Warning - " + value);
                                exitcode = 1;
                                timer1.Enabled = true;
                                return;
                            }
                            if (crit != null && OSL.RxValue.Contains(crit))
                            {
                                Console.WriteLine("Critical - " + value);
                                exitcode = 2;
                                timer1.Enabled = true;
                                return;
                            }
                            Console.WriteLine("OK - "+ value);
                            exitcode = 0;
                            timer1.Enabled = true;
                        }
                        else
                        {
                            Console.WriteLine("Critical - Bad RIC returned for " + OSL.RxValue);
                            exitcode = 2;
                            timer1.Enabled = true;
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {
                OSL.RxAvailable = false;                
            }
        }
    }
}
