/* All content in this sample is �AS IS?with with no warranties, and confer no rights. 
 * Any code on this blog is subject to the terms specified at http://www.microsoft.com/info/cpyright.mspx. 
 */

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AxRDPCOMAPILib;
using System.Net.Sockets;
using System.Net;

namespace WinViewer
{
    public partial class WinViewer : Form
    {
        public WinViewer()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            string ConnectionString = null;
            UdpClient udpcRecv;
            IPAddress ipAddr = null;
            IPHostEntry ipAddrEntry = Dns.GetHostEntry(Dns.GetHostName());//��õ�ǰHOST��

            //�ҵ�ipv4��Ч��hostip
            foreach (IPAddress _IPAddress in ipAddrEntry.AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    ipAddr = _IPAddress;
                }
            }

            string ip = ipAddr.ToString();

            IPEndPoint localIpep = new IPEndPoint(ipAddr, 7788); // ����IP��ָ���Ķ˿ں�
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0); // ���͵���IP��ַ�Ͷ˿ں�
            udpcRecv = new UdpClient(localIpep);

            Timer tick = new Timer();//�������ʹ��System.Windows.Froms.Timer ��������ͬһϵ�����߳�
            tick.Interval = 3000;
            tick.Tick += delegate
            {
                
                byte[] bytRecv = udpcRecv.Receive(ref remoteIpep);
                ConnectionString = Encoding.Default.GetString(bytRecv);
                //ConnectionString = ReadFromFile();
                if (ConnectionString != null)
                {
                    try
                    {
                        string shareIp = remoteIpep.Address.ToString();
                        m_ConnectIp = shareIp;//�ýӿ������Ժ���ʾ�����еĿͻ���IP
                        LogTextBox.Text += "���ӵ��ն˻�: " + m_ConnectIp + Environment.NewLine;
                        pRdpViewer.SmartSizing = true;
                        pRdpViewer.Connect(ConnectionString, m_ConnectIp, "");
                        tick.Stop();
                    }
                    catch (Exception ex)
                    {
                        LogTextBox.Text += "���Ӵ���. ������Ϣ: " + ex.ToString() + Environment.NewLine;
                    }
                }
            };
            
            tick.Start();
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            pRdpViewer.Disconnect();
        }

        private string ReadFromFile()
        {
            string ReadText = null;
            string FileName = null;
            string[] args = Environment.GetCommandLineArgs();
            
            if (args.Length == 2)
            {
                if (!args[1].EndsWith("inv.xml"))
                {
                    FileName = args[1] + @"\" + "inv.xml";
                }
                else
                {
                    FileName = args[1];
                }
            }
            else
            {
                FileName = "inv.xml";
            }
            
            LogTextBox.Text += ("���ļ���ȡ�����ִ� " +
                FileName + Environment.NewLine);
            try
            {
                using (StreamReader sr = File.OpenText(FileName))
                {
                    ReadText = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                LogTextBox.Text += ("��ȡ�����ִ��ļ�����. ������Ϣ: " + ex.ToString() + Environment.NewLine);
            }
            return ReadText;
        }

        private void OnConnectionEstablished(object sender, EventArgs e)
        {
            LogTextBox.Text += "���ӽ���" + Environment.NewLine;
        }

        private void OnError(object sender, _IRDPSessionEvents_OnErrorEvent e)
        {
            int ErrorCode = (int)e.errorInfo;
            LogTextBox.Text += ("Error 0x" + ErrorCode.ToString("X") + Environment.NewLine);
        }

        private void OnConnectionTerminated(object sender, _IRDPSessionEvents_OnConnectionTerminatedEvent e)
        {
            LogTextBox.Text += "������ֹ. ԭ��: " + e.discReason + Environment.NewLine;
        }

        private void ControlButton_Click(object sender, EventArgs e)
        {
            pRdpViewer.RequestControl(RDPCOMAPILib.CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE);
        }

        private void OnConnectionFailed(object sender, EventArgs e)
        {
            LogTextBox.Text += "����ʧ��." + Environment.NewLine;
        }

        private string m_ConnectIp = null;
        private bool m_bIsConnected = false;
    }
}