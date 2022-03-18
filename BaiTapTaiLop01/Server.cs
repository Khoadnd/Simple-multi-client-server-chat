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
    public partial class Server : Form
    {
        private Dictionary<EndPoint, string> usernames = new Dictionary<EndPoint, string>();
        private IPEndPoint IP = null;
        private List<EndPoint> client = new List<EndPoint>();
        private Socket socket = null;
        public Server()
        {
            InitializeComponent();
            txtMessage.ReadOnly = true;
        }

        private void btnHost_click(object sender, EventArgs e)
        {
            txtMessage.ReadOnly = false;
            txtIP.ReadOnly = true;
            errorProvider1.Clear();

            if (!IPEndPoint.TryParse(txtIP.Text.Trim(), out this.IP))
            {
                errorProvider1.SetError(txtIP, "IP không đúng định dạng");
                return;
            }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            socket.Bind((EndPoint)IP);

            Thread receive = new Thread(Receive);
            receive.IsBackground = true;
            receive.Start();

            txtReceived.AppendText("Hosted on " + IP.ToString() + "\r\n");
        }

        private void Send()
        {
            byte[] message = Serialize(txtMessage.Text);
            client.ForEach(n => socket.SendTo(message, 0, message.Length, SocketFlags.None, n));

            txtReceived.AppendText("Me: " + txtMessage.Text.Trim() + "\r\n");
            txtMessage.Clear();
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
                    int rc = socket.ReceiveFrom(data, ref senderRemote);
                    if (rc == 0)
                        continue;
                    string buffer = (string)Deserialize(data);
                    if (buffer.Equals("!status"))
                    {
                        socket.SendTo(Serialize("Alive!"), SocketFlags.None, senderRemote);
                        continue;
                    }

                    if (buffer.Equals("!connect"))
                    {
                        txtReceived.AppendText(senderRemote.ToString() + " connected!\r\n");
                        socket.SendTo(Serialize("Welcome to chat!\r\n!username <username> to change your name!"), SocketFlags.None, senderRemote);
                        client.Add(senderRemote);
                        usernames.Add(senderRemote, senderRemote.ToString());
                        continue;
                    }
                    if (buffer.Equals("!disconnect"))
                    {
                        txtReceived.AppendText(usernames[senderRemote] + " disconnected!\r\n");
                        client.Remove(senderRemote);
                        usernames.Remove(senderRemote);
                        continue;
                    }
                    if (buffer.Split(' ')[0].Equals("!username"))
                    {
                        txtReceived.AppendText(usernames[senderRemote] + " has changed name to " + buffer.Split(' ')[1] + "\r\n");
                        client.ForEach(n => socket.SendTo(Serialize(usernames[senderRemote] + " has changed name to " + buffer.Split(' ')[1]), SocketFlags.None, n));
                        usernames[senderRemote] = buffer.Split(' ')[1];
                        socket.SendTo(Serialize("Your name is: " + usernames[senderRemote]), SocketFlags.None, senderRemote);
                        continue;
                    }


                    txtReceived.AppendText(usernames[senderRemote] + ": " + buffer + "\r\n");
                    client.ForEach(n =>
                    {
                        if (!senderRemote.Equals(n))
                            socket.SendTo(Serialize(usernames[senderRemote] + ": " + buffer), SocketFlags.None, n);
                    });
                }
            }
            catch { socket.Close(); }
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

        private void btnShutDown_Click(object sender, EventArgs e)
        {
            txtReceived.Text += "Shutting down server...\r\n";
            socket.Close();
            txtIP.ReadOnly = false;
            txtMessage.ReadOnly = true;
        }

        private void txtIP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                btnHost.PerformClick();
        }

        private void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                txtMessage.Clear();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)            
                Send();                         
        }
    }
}
