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
using System.Drawing;
using System.Runtime.InteropServices;

namespace WinViewer
{
    public partial class WinViewer : Form
    {
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static Bitmap GetWindowCapture(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            RECT windowRect = new RECT();
            GetWindowRect(hWnd, ref windowRect);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, width, height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Bitmap.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);//ɾ���ù��Ķ���
            DeleteDC(hmemdc);//ɾ���ù��Ķ���
            return bmp;
        }

        //[DllImport("user32")]
        //[return: MarshalAs(UnmanagedType.Bool)]

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(
            IntPtr hwndParent,
            uint hwndChildAfter,
            string lpszClass,
            string lpszWindow
            );

        private IntPtr FindWindowEx(IntPtr hwnd, string lpszWindow, bool bChild)
        {
            IntPtr iResult = IntPtr.Zero;
            // �����ڸ������ϲ��ҿؼ�
            iResult = FindWindowEx(hwnd, 0, null, lpszWindow);
            // ����ҵ�ֱ�ӷ��ؿؼ����
            if (iResult != IntPtr.Zero) return iResult;

            // ����趨�˲����Ӵ����в���
            if (!bChild) return iResult;

            // ö���Ӵ��壬���ҿؼ����
            int i = EnumChildWindows(
            hwnd,
            (h, l) =>
            {
                IntPtr f1 = FindWindowEx(h, 0, null, lpszWindow);
                if (f1 == IntPtr.Zero)
                    return true;
                else
                {
                    iResult = f1;
                    return false;
                }
            },
            0);
            // ���ز��ҽ��
            return iResult;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd
         );

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(
         IntPtr hwnd,                // Window to copy,Handle to the window that will be copied.
         IntPtr hdcBlt,              // HDC to print into,Handle to the device context.
         UInt32 nFlags               // Optional flags,Specifies the drawing options. It can be one of the following values.
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
         string lpszDriver,         // driver name������
         string lpszDevice,         // device name�豸��
         string lpszOutput,         // not used; should be NULL
         IntPtr lpInitData   // optional printer data
         );
        [DllImport("gdi32.dll")]
        public static extern int BitBlt(
         IntPtr hdcDest, // handle to destination DCĿ���豸�ľ��
         int nXDest,   // x-coord of destination upper-left cornerĿ���������Ͻǵ�X����
         int nYDest,   // y-coord of destination upper-left cornerĿ���������Ͻǵ�Y����
         int nWidth,   // width of destination rectangleĿ�����ľ��ο��
         int nHeight, // height of destination rectangleĿ�����ľ��γ���
         IntPtr hdcSrc,   // handle to source DCԴ�豸�ľ��
         int nXSrc,    // x-coordinate of source upper-left cornerԴ��������Ͻǵ�X����
         int nYSrc,    // y-coordinate of source upper-left cornerԴ��������Ͻǵ�Y����
         UInt32 dwRop   // raster operation code��դ�Ĳ���ֵ
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(
         IntPtr hdc // handle to DC
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(
         IntPtr hdc,         // handle to DC
         int nWidth,      // width of bitmap, in pixels
         int nHeight      // height of bitmap, in pixels
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(
         IntPtr hdc,           // handle to DC
         IntPtr hgdiobj    // handle to object
         );

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(
         IntPtr hdc           // handle to DC
         );


        public WinViewer()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            //RdpViewerWindow rdpViewerWindow = new RdpViewerWindow
            //{
            //    MdiParent = this
            //};
            //rdpViewerWindow.Show();
            

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
            

            Timer tick = new Timer();//�������ʹ��System.Windows.Froms.Timer ��������ͬһϵ�����߳�
            tick.Interval = 3000;
            tick.Tick += delegate
            {
                udpcRecv = new UdpClient(localIpep);
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
                udpcRecv.Close();
   
            };
            
            tick.Start();
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            pRdpViewer.Disconnect();
        }

        private void CaptureWndButton_Click(object sender, EventArgs e)
        {
            IntPtr HandleResult = FindWindowEx(pRdpViewer.Handle,@"Output Painter Window",true);

            Bitmap sourceBitmap = GetWindowCapture(HandleResult);

            sourceBitmap.Save(@"form2.bmp");
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