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
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace DeliveryServer1
{
    public partial class Form1 : Form
    {
        TcpListener server = null;
        
        public Form1()
        {
            InitializeComponent();
            FormClosing += new FormClosingEventHandler(WindowsFormClosing);
            InitStart();
        }
        private void WindowsFormClosing(object sender, FormClosingEventArgs s)
        {
            if (server != null)
                server = null;

            Application.Exit();
        }


        private void InitStart()
        {
            Thread socketworker = new Thread(new ThreadStart(socketThread));
            socketworker.IsBackground = true;
            socketworker.Start();
        }

        private void socketThread()
        {
            try
            {
                server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
                server.Start();

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    updateStatusInfo("Connected");
                    Thread clientworker = new Thread(new ParameterizedThreadStart(clientThread));
                    clientworker.IsBackground = true;
                    clientworker.Start(client);
                }
            }
            catch (SocketException se)
            {
                Debug.WriteLine("SocketException : {0}", se.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception : {0}", ex.Message);
            }
        }

        private void clientThread(object sender)
        {
            // 1. 데이타 받기
            TcpClient client = sender as TcpClient;
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[8092];
            DataPacket packet = new DataPacket();

            while (stream.Read(buffer, 0, buffer.Length) != 0)
            {
                packet = GetBindAck(buffer);
            }

            stream.Close();
            client.Close();

            // 2. 데이타 표시하기
            string Id = packet.Id;
            string Phone = packet.Phone;
            string Address = packet.Address;
            string Memo = packet.Memo;
            string Menu = packet.Menu;

            Invoke((MethodInvoker)delegate
            {
                ListViewItem i = new ListViewItem();
                i.Text = Id;
                i.SubItems.Add(Phone);
                i.SubItems.Add(Address);
                i.SubItems.Add(Menu);
                i.SubItems.Add(Memo);
                i.SubItems.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                listView1.Items.Add(i);

            });



            
            updateStatusInfo("주문 접수!");
        }

        private void updateStatusInfo(string content)
        {
            Action del = delegate ()
            {
                toolStripStatusLabel1.Text = content;
            };
            Invoke(del);
        }
        
        private void btnDataClear_Click(object sender, EventArgs e)
        {
            foreach(ListViewItem item in listView1.SelectedItems)
            {
                listView1.Items.Remove(item);
            }
            listView1.Update();
        }
        private DataPacket GetBindAck(byte[] btfuffer)
        {
            DataPacket packet = new DataPacket();

            MemoryStream ms = new MemoryStream(btfuffer, false);
            BinaryReader br = new BinaryReader(ms);

            packet.Id = ExtendedTrim(Encoding.UTF8.GetString(br.ReadBytes(20)));
            packet.Phone = ExtendedTrim(Encoding.UTF8.GetString(br.ReadBytes(20)));
            packet.Address = ExtendedTrim(Encoding.UTF8.GetString(br.ReadBytes(20)));
            packet.Menu = ExtendedTrim(Encoding.UTF8.GetString(br.ReadBytes(100)));
            packet.Memo = ExtendedTrim(Encoding.UTF8.GetString(br.ReadBytes(100)));

            br.Close();
            ms.Close();

            return packet;
        }

        
        private string ExtendedTrim(string source)
        {
            string dest = source;
            int index = dest.IndexOf('\0');
            if (index > -1)
            {
                dest = source.Substring(0, index + 1);
            }

            return dest.TrimEnd('\0').Trim();
        }

      }
}
