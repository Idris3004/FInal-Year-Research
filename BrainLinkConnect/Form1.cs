using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using BrainLinkSDK_Windows;
using System.IO;
using WMPLib;
using System;

namespace BrainLinkConnect
{
    public partial class Form1 : Form
    {
        private BrainLinkSDK brainLinkSDK;

        private float ave = 0;

        private List<int> raw = new List<int>();

        private List<float> hrvList = new List<float>();

        private List<double> lastHRV = new List<double>();

        private List<(long, string)> Devices = new List<(long, string)>();

        List<string[]> dataPoints = new List<string[]>();

        private WindowsMediaPlayer player = new WindowsMediaPlayer();

        private StreamWriter writer;

        private List<float[]> eegData = new List<float[]>();
        private bool Sad;
        private bool stressed;
        private bool Angry;

        public Form1()
        {
            InitializeComponent();

            brainLinkSDK = new BrainLinkSDK();
            brainLinkSDK.OnEEGDataEvent += new BrainLinkSDKEEGDataEvent(BrainLinkSDK_OnEEGDataEvent);
            brainLinkSDK.OnEEGExtendEvent += new BrainLinkSDKEEGExtendDataEvent(BrainLinkSDK_OnEEGExtendDataEvent);
            brainLinkSDK.OnGyroDataEvent += new BrainLinkSDKGyroDataEvent(BrainLinkSDK_OnGyroDataEvent);
            brainLinkSDK.OnHRVDataEvent += new BrainLinkSDKHRVDataEvent(BrainLinkSDK_OnHRVDataEvent);
            brainLinkSDK.OnRawDataEvent += new BrainLinkSDKRawDataEvent(BrainLinkSDK_OnRawDataEvent);
            brainLinkSDK.OnDeviceFound += new BrainLinkSDKOnDeviceFoundEvent(BrainLinkSDK_OnDeviceFoundEvent);
            writer = new StreamWriter("brainlink_data.csv");
        }

        private void BrainLinkSDK_OnDeviceFoundEvent(long Address, string Name)
        {
            Debug.WriteLine("Discover name " + Name);
            listBox1.Items.Add(Name + " : " + Address.ToString("X12"));
            Devices.Add((Address, Name));
        }

        private void BrainLinkSDK_OnRawDataEvent(int Raw)
        {
            raw.Add(Raw);
            if (raw.Count > 512)
            {
                raw.Remove(raw[0]);
            }
            chart1.Series[0].Points.DataBindY(raw);
        }

        private void BrainLinkSDK_OnHRVDataEvent(int[] HRV, int Blink)
        {
            for (int i = 0; i < HRV.Length; i++)
            {
                hrvBox.Text += HRV[i] + "ms ";
                hrvList.Add(HRV[i]);
            }
            if (hrvList.Count >= 60)
            {
                double hrv = StandardDiviation(hrvList.ToArray());
                lastHRV.Add(hrv);
                if (lastHRV.Count > 5)
                {
                    lastHRV.RemoveAt(0);
                }
                string hrvString = "";
                for (int i = 0; i < lastHRV.Count; i++)
                {
                    hrvString += "hrv" + i + ":" + lastHRV[i].ToString("F2");
                }
                hrvString += " avg:" + ave.ToString("F2") + " size:" + hrvList.Count;
                hrvList.Clear();
                hrvLabel.Text = hrvString;
                hrvBox.Text = "";
            }
        }

        private void BrainLinkSDK_OnGyroDataEvent(int X, int Y, int Z)
        {
            xvalue.Text = X.ToString();
            yvalue.Text = Y.ToString();
            zvalue.Text = Z.ToString();
        }

        private void BrainLinkSDK_OnEEGExtendDataEvent(BrainLinkExtendModel Model)
        {
            //Debug.WriteLine("Extend");
            ap.Text = Model.Ap.ToString();
            ele.Text = Model.Electric.ToString();
            version.Text = Model.Version.ToString();
            temp.Text = Model.Temperature.ToString();
            heart.Text = Model.HeartRate.ToString();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (listBox1 != null)
            {
                Debug.WriteLine("Click");
                brainLinkSDK.Start();
                listBox1.Items.Clear();
                Devices.Clear();
            }
        }
      

        private void BrainLinkSDK_OnEEGDataEvent(BrainLinkModel Model)
        {
            att.Text = Model.Attention.ToString();
            med.Text = Model.Meditation.ToString();
            delta.Text = Model.Delta.ToString();
            theta.Text = Model.Theta.ToString();
            lalpha.Text = Model.LowAlpha.ToString();
            halpha.Text = Model.HighAlpha.ToString();
            lbeta.Text = Model.LowBeta.ToString();
            hbeta.Text = Model.HighBeta.ToString();
            lgamma.Text = Model.LowGamma.ToString();
            hgamma.Text = Model.HighGamma.ToString();
            signal.Text = Model.Signal.ToString();
            // Write the EEG data to the Excel csv file
            writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
            Model.Attention, Model.Meditation, Model.Delta, Model.Theta,
            Model.LowAlpha, Model.HighAlpha, Model.LowBeta, Model.HighBeta,
            Model.LowGamma, Model.HighGamma, Model.Signal);
  
                // Check if user is in an emotive state as trained by the k-mean clustering technique
                if (Model.Attention <= 31.897471477027437 && Model.Meditation <= 69.52235584335492 && Model.Delta <= 841531.2007400552 && Model.Theta <= 108209.60692260253 
                && Model.LowAlpha <= 34893.7792167746 && Model.HighAlpha <= 18904.195575084796 && Model.LowBeta <= 18844.59636139377 && Model.HighBeta <= 15253.772587110698
                && Model.LowGamma <= 8296.758248535307 && Model.HighGamma <= 4868.545097132283)
                {
                    Sad = true;
                    listBox2.Items.Clear();
                    listBox2.Items.Add("sad");                    
                }
                 if (Model.Attention >= 44.31435806366747 && Model.Meditation >= 44.35977374375163 && Model.Delta >= 817576.1149697455 && Model.Theta >= 163192.4634635623
                && Model.LowAlpha >= 23646.419922388857 && Model.HighAlpha >= 11467.252926861347 && Model.LowBeta >= 10801.817679558018 && Model.HighBeta >= 8493.861154959228
                && Model.LowGamma >= 5631.106123388582 && Model.HighGamma >= 8811.604906603527)
                {
                    stressed = true;
                    listBox2.Items.Clear();
                    listBox2.Items.Add("stressed");
                }

                else if ((Model.Attention >= 42.62432216905901 && Model.Meditation >= 64.14469696969698 && Model.Delta >= 841550.7750797449 && Model.Theta >= 157947.9783891547
                && Model.LowAlpha >= 38681.413636363635 && Model.HighAlpha >= 21360.442663476875 && Model.LowBeta >= 17168.594298245614 && Model.HighBeta >= 13444.8692185008 
                && Model.LowGamma >= 8380.469218500799 && Model.HighGamma >= 7362.06889952153))
                {
                  Angry = true;
                  listBox2.Items.Clear();
                  listBox2.Items.Add("Angry");
                }

            }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            //Debug.WriteLine("Click");
            //brainLinkSDK.Start();

        }

        private double StandardDiviation(float[] x)
        {
            ave = x.Average();
            double dVar = 0;
            for (int i = 0; i < x.Length; i++)
            {
                dVar += (x[i] - ave) * (x[i] - ave);
            }
            return Math.Sqrt(dVar / x.Length);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            brainLinkSDK.SetApEnable(checkBox1.Checked);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            brainLinkSDK.SetGyroEnable(checkBox2.Checked);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            brainLinkSDK.Close();
            writer.Close();
            brainLinkSDK = null;
            Dispose();
            Application.Exit();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

        }

        private void Connect_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < Devices.Count)
            {
                (long, string) Device = Devices[listBox1.SelectedIndex];
                brainLinkSDK.connect(Device.Item1);
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            // Get the path of the CSV file
            string filePath = System.IO.Path.Combine(Application.StartupPath, "brainlink_data.csv");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                MessageBox.Show("The file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Open the file using the default program for CSV files
            Process.Start(filePath);
        }

        

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Sad == true)
            {
                // Set the path to the nasheed folder
                string nasheedPath = @"C:\Users\arapa\Desktop\Sad Nasheed";
                // Choose a random nasheed from the folder

                string[] nasheeds = Directory.GetFiles(nasheedPath);
                Random random = new Random();
                string nasheed = nasheeds[random.Next(nasheeds.Length)];
                // Start Windows Media Player app and play the selected nasheed
                Process.Start("wmplayer.exe", $"/play /close \"{nasheed}\"");
            }
            if (stressed == true)
            {
                // Set the path to the nasheed folder
                string nasheedPath = @"C:\Users\arapa\Desktop\Stressed Nasheed";
                // Choose a random nasheed from the folder

                string[] nasheeds = Directory.GetFiles(nasheedPath);
                Random random = new Random();
                string nasheed = nasheeds[random.Next(nasheeds.Length)];
                // Start Windows Media Player app and play the selected nasheed
                Process.Start("wmplayer.exe", $"/play /close \"{nasheed}\"");
            }
            if (Angry == true)
            {
                // Set the path to the nasheed folder
                string nasheedPath = @"C:\Users\arapa\Desktop\Angry Nasheed";
                // Choose a random nasheed from the folder

                string[] nasheeds = Directory.GetFiles(nasheedPath);
                Random random = new Random();
                string nasheed = nasheeds[random.Next(nasheeds.Length)];
                // Start Windows Media Player app and play the selected nasheed
                Process.Start("wmplayer.exe", $"/play /close \"{nasheed}\"");
            }
        }
    }

}