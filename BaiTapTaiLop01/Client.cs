using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaiTapTaiLop01
{
    public partial class Client : Form
    {
        private IPEndPoint IP = null;
        private Socket socket = null;
        public Client()
        {
            InitializeComponent();
            txtMessage.ReadOnly = true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            txtMessage.ReadOnly = false;
            txtIP.ReadOnly = true;
            txtUsrname.ReadOnly = true;
            errorProvider1.Clear();

            if (!IPEndPoint.TryParse(txtIP.Text.Trim(), out IP))
            {
                errorProvider1.SetError(txtIP, "IP không đúng định dạng");
                return;
            }

            //UDP
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            socket.Connect(IP);

            string temp = txtUsrname.Text.Trim();
            
            Send(Serialize("!connect"));
            if (!temp.Equals(""))
                Send(Serialize("!username " + temp));

            Thread receive = new Thread(Receive);
            receive.IsBackground = true;
            receive.Start();
        }

        private void btnDisconnect_click(object sender, EventArgs e)
        {
            Send(Serialize("!disconnect"));
            socket.Close();
            txtReceived.AppendText("Disconnected from " + IP.ToString() + "\r\n");
            txtIP.ReadOnly = false;
            txtMessage.ReadOnly = true;
            txtUsrname.ReadOnly = false;
        }

        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 10000];
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint senderRemote = (EndPoint)sender;
                    int rc = socket.ReceiveFrom(data, 1024 * 10000, SocketFlags.None, ref senderRemote);
                    if (rc != 0)
                        txtReceived.AppendText("Server: " + (string)Deserialize(data) + "\r\n");
                }
            }
            catch { socket.Close(); }
        }

        private void Send(byte[] message = null)
        {
            if (message == null)
                message = Serialize(txtMessage.Text);
            int rc = socket.SendTo(message, 0, message.Length, SocketFlags.None, IP);
            if (rc == 0)
            {
                txtReceived.AppendText("Can't send the Message!\r\n");
                return;
            }
            txtReceived.AppendText("Me: " + txtMessage.Text.Trim() + "\r\n");
            txtMessage.Clear();
        }

        public static byte[] Serialize(object o)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, o);
            return stream.ToArray();
        }

        public static object Deserialize(byte[] o)
        {
            MemoryStream stream = new MemoryStream(o);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Send();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtMessage.Clear();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                Send();
        }

        private void txtIP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)  
                btnConnect.PerformClick();
        }

        private void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                txtMessage.Clear();
        }
    }
}
