using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Peer_to_Peer_Chat
{
    public partial class Form1 : Form
    {
        //-- Declare the socket, endpoints and memory buffer --//
        Socket mySocket;
        EndPoint epLocal, epRemote;
        byte[] buffer;


        //-- Small Function that gets the local internet Protocol address --//
        private string getLocalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "127.0.0.1";
        }

        //-- Message Call Back Function, updates the chat log --//
        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                byte[] receivedData = new byte[1500];
                receivedData = (byte[])aResult.AsyncState;
                

                ASCIIEncoding aEncoding = new ASCIIEncoding();
                string receivedMessage = aEncoding.GetString(receivedData);

                chatBox.Text += "Friend: " + receivedMessage;
                chatBox.Text += "\n";

                buffer = new byte[1500];
                mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "An error occurred while transmitting data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //-- Disable checking for cross thread calls --//
            RichTextBox.CheckForIllegalCrossThreadCalls = false;
            TextBox.CheckForIllegalCrossThreadCalls = false;

            //-- Configure the socket --//
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            label1.Text = "Current IP: " + getLocalIP();

            
        }

        //-- Close Program Button --//
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //-- Handle Form Keydown Events Here --//
        }

        private void typeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                byte[] sendingMessage = aEncoding.GetBytes(typeBox.Text);
                
                mySocket.Send(sendingMessage);

                chatBox.Text += "You: " + typeBox.Text + "\n";
                typeBox.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //-- Obtain User Network Information --//

            string netInfo = "";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c ipconfig";
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.Start();

            StreamReader outputWriter = p.StandardOutput;
            netInfo += outputWriter.ReadToEnd();

            MessageBox.Show(netInfo);
            // ----------------------------------------//
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void connButton_Click(object sender, EventArgs e)
        {
            epLocal = new IPEndPoint(IPAddress.Parse(getLocalIP()), (int)numericUpDown1.Value);
            mySocket.Bind(epLocal);

            epRemote = new IPEndPoint(IPAddress.Parse(ipBox.Text), (int)numericUpDown2.Value);
            mySocket.Connect(epRemote);

            //-- Listen to the port for new data --//
            buffer = new byte[1500];
            mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

            connButton.Enabled = false;
        }
    }
}
