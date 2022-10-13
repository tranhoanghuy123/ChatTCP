using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }
        TcpClient client;
        byte[] message;
        private void btn_Connect_Click(object sender, EventArgs e)
        {
            IPAddress ip;
            int port;
            if (string.IsNullOrEmpty(client_IP.Text) || string.IsNullOrEmpty(client_Port.Text))
            {
                MessageBox.Show("Vui Lòng Nhập IP Và Port Từ 1000 Trở Lên!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!int.TryParse(client_Port.Text, out port))
            {
                MessageBox.Show("Port Không Hợp Lệ!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //if (!IPAddress.TryParse(client_IP.Text, out ip))
            //{
            //    MessageBox.Show("Vui Lòng Nhập IP Hợp Lệ!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            if (string.IsNullOrEmpty(txt_Username.Text))
            {
                MessageBox.Show("Tên Người Dùng Bị Bỏ Trống Kìa :((", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            client_result.Text += "Waiting for Server... \r\n";
            client = new TcpClient();
            IPAddress[] host = Dns.GetHostAddresses(client_IP.Text);
            var result = client.BeginConnect(host[0], port, Connect, client); 
            client_result.Text += "Connected "+ client_IP.Text + " !\r\n";

            // Config form non-input data,only using textbox message and button send then start server
            client_IP.Enabled = false;
            client_Port.Enabled = false;
            btn_Connect.Enabled = false;
            txt_Username.Enabled = false;
            client_Message.Enabled = true;
            btn_Send.Enabled = true; 
            btn_Clear.Enabled = true;
            btn_Disconnect.Enabled = true;
        }
        void Connect(IAsyncResult iar)
        {
            TcpClient TCPClient;

            try
            {
                TCPClient = (TcpClient)iar.AsyncState;
                TCPClient.EndConnect(iar);
                message = new byte[512];
                // Read Message if connection is successful
                TCPClient.GetStream().BeginRead(message, 0, message.Length, readMessageFromServer, TCPClient); 

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void readMessageFromServer(IAsyncResult iar)
        {
            TcpClient TCPClient;
            int nCountBytesReceivedFromServer;
            string strReceived;

            try
            {
                TCPClient = (TcpClient)iar.AsyncState;
                nCountBytesReceivedFromServer = TCPClient.GetStream().EndRead(iar); // Get quantity bytes of Network stream

                if (nCountBytesReceivedFromServer == 0) 
                {

                    MessageBox.Show("Kết Nối Thất Bại.", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //Get Message From server and Display it 
                strReceived = Encoding.UTF8.GetString(message, 0, nCountBytesReceivedFromServer);
                msg(strReceived);
                message = new byte[512];
                TCPClient.GetStream().BeginRead(message, 0, message.Length, readMessageFromServer, TCPClient);

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void writeMessageToServer(IAsyncResult iar)
        {
            TcpClient TCPClient;
            try
            {
                TCPClient = (TcpClient)iar.AsyncState;
                TCPClient.GetStream().EndWrite(iar);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btn_Send_Click(object sender, EventArgs e)
        {
            //Events will do something if Button is clicked
            if (string.IsNullOrEmpty(client_Message.Text)) // check message is empty?
            {
                MessageBox.Show("Vui lòng nhập nội dung tin nhắn!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };
            try
            {
                if (client != null)
                {
                    if (client.Client.Connected)
                    {
                        byte[] message = Encoding.UTF8.GetBytes(txt_Username.Text+" :" + client_Message.Text);
                        //get stream and send message to server
                        client.GetStream().BeginWrite(message, 0, message.Length, writeMessageToServer, client);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void msg(string mesg)
        {
            // Method display Message into textbox_result
            client_result.Text += mesg + "\r\n";
        }
        public void Disconnect(IAsyncResult iar)
        {
            client.EndConnect(iar);
        }
        private void btn_Disconnect_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Ngắt Kết Nối Thành Công!","Thông Báo",MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            btn_Disconnect.Enabled = false;
            btn_Clear.Enabled = false;
            client_Message.Enabled = false;
            btn_Send.Enabled = false;
            client_result.Enabled = false;
            client.GetStream().Close();
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            client_result.Text = null;
        }

        private void client_result_TextChanged(object sender, EventArgs e)
        {

        }

        private void Client_Load(object sender, EventArgs e)
        {

        }
    }
}
