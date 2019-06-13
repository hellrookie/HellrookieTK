namespace Hellrookie.ToolKit.SocketLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// windows socket
    /// </summary>
    public class SocketInstance : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public delegate bool SocketDelegate();
        /// <summary>
        /// socket释放前执行操作
        /// </summary>
        public event SocketDelegate DoBeforDispose;
        /// <summary>
        /// socket释放后执行操作
        /// </summary>
        public event SocketDelegate DoAfterDispose;
        /// <summary>
        /// socket对象
        /// </summary>
        protected Socket socket;
        private bool disposed;

        /// <summary>
        /// socket终端address信息
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// socket本地address信息
        /// </summary>
        public IPEndPoint LocalEndPoint { get; set; }


        /// <summary>
        /// 默认使用IPv4_TCP
        /// </summary>
        public SocketInstance()
        {
            CreateInstance();
            disposed = false;
        }

        /// <summary>
        /// 使用套接字类型和协议类型实例化SocketInstance
        /// </summary>
        /// <param name="type">套接字类型</param>
        /// <param name="protocol">协议类型</param>
        public SocketInstance(SocketType type, ProtocolType protocol)
        {
            CreateInstance(type, protocol);
            disposed = false;
        }

        private void CreateInstance(SocketType type = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp)
        {
            socket = new Socket(AddressFamily.InterNetwork, type, protocol);
        }

        private void CreateInstance(Socket s)
        {
            socket = s;
        }

        /// <summary>
        /// 带有Pool的write
        /// </summary>
        public bool WriteConnected
        {
            get { return Poll(100, SelectMode.SelectWrite); }
        }

        /// <summary>
        /// 带有Pool的read
        /// </summary>
        public bool ReadConnected
        {
            get { return Poll(100, SelectMode.SelectRead); }
        }

        /// <summary>
        /// 绑定到IPAddress.Any
        /// </summary>
        /// <param name="port">绑定端口</param>
        public void Bind(int port)
        {
            Bind(IPAddress.Any, port);
        }

        /// <summary>
        /// 绑定到指定address
        /// </summary>
        /// <param name="address">绑定地址</param>
        /// <param name="port">绑定端口</param>
        public void Bind(string address, int port)
        {
            try
            {
                Bind(IPAddress.Parse(address), port);
            }
            catch(Exception ex)
            {
                throw new InvalidSocketAddressException(ex.ToString(), address);
            }
        }

        private void Bind(IPAddress address, int port)
        {
            var ep = GetEndPointInfo(address, port);
            try
            {
                if (socket != null)
                {
                    socket.Bind(ep);
                    LocalEndPoint = (socket.LocalEndPoint as IPEndPoint) ;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 监听端口
        /// </summary>
        /// <param name="backlog">接收连接数</param>
        public void Listen(int backlog)
        {
            try
            {
                socket.Listen(backlog);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns>建立的连接SocketInstance实例</returns>
        public SocketInstance Accept()
        {
            Socket s = null;
            try
            {
                s = socket.Accept();
            }
            catch (Exception)
            {
                throw;
            }

            SocketInstance instance = null;
            if (s == null)
            {
                throw new NullSocketException("No socket return from accept method.");
            }
            else
            {
                instance = new SocketInstance();
                instance.CreateInstance(s);
                instance.RemoteEndPoint = (IPEndPoint)s.RemoteEndPoint;
            }

            return instance;
        }

        /// <summary>
        /// 连接到套接字
        /// </summary>
        /// <param name="address">连接地址</param>
        /// <param name="port">连接端口</param>
        public void Connect(string address, int port)
        {
            var ep = GetEndPointInfo(IPAddress.Parse(address), port);
            try
            {
                socket.Connect(ep);
            }
            catch (Exception ex)
            {
                throw new SocketException(ex.Message);
            }
            RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
        }

        private IPEndPoint GetEndPointInfo(IPAddress address, int port)
        {
            if (address == null)
            {
                throw new InvalidSocketAddressException("Socket Address should not be null", address.ToString());
            }

            if (port >= 65535 || port < 0)
            {
                throw new InvalidSocketAddressException("Socket port is invalid", port.ToString());
            }

            IPEndPoint ep = null;
            try
            {
                ep = new IPEndPoint(address, port);
            }
            catch (Exception)
            {
                throw;
            }
            return ep;
        }

        /// <summary>
        /// Poll
        /// </summary>
        /// <param name="microSeconds">响应时间</param>
        /// <param name="mode">select 类型</param>
        /// <returns></returns>
        public bool Poll(int microSeconds, SelectMode mode)
        {
            if (socket == null)
            {
                throw new NullSocketException("Call CreateInstance before use socket.");
            }
            return socket.Poll(microSeconds, mode);
        }

        /// <summary>
        /// SocketInstance析构函数
        /// </summary>
        ~SocketInstance()
        {
            Dispose(false);
        }

        /// <summary>
        /// 从socket读数据
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>读到数据的长度</returns>
        public int Read(out string value)
        {
            var rtnValue = new StringBuilder();
            var recvData = new byte[1024];
            string convertData = string.Empty;
            int readCount = 0;
            do
            {
                readCount += socket.Receive(recvData, 1024, SocketFlags.None);
                if(readCount == 0)
                {
                    Dispose();
                    value = "";
                    return 0;
                }
                convertData = Encoding.UTF8.GetString(recvData, 0, readCount);
                rtnValue.AppendFormat("{0}", convertData);
            } while (convertData.IndexOf("<EOF>") < 0);
            value = rtnValue.ToString().Substring(0, rtnValue.Length - 5);
            return readCount;
        }

        /// <summary>
        /// 向socket写数据
        /// </summary>
        /// <param name="format">数据格式</param>
        /// <param name="args">格式参数</param>
        public void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        /// <summary>
        /// 向socket写数据
        /// </summary>
        /// <param name="data">写入数据</param>
        public void Write(string data)
        {
            var dataBuilder = new StringBuilder();
            dataBuilder.AppendFormat("{0}{1}", data, "<EOF>");
            var dataToSend = Encoding.UTF8.GetBytes(dataBuilder.ToString());
            try
            {
                socket.Send(dataToSend, dataToSend.Length, 0);
            }
            catch(Exception ex)
            {
                throw new SocketException(ex.ToString());
            }
        }

        /// <summary>
        /// 广播数据
        /// </summary>
        /// <param name="port">广播端口</param>
        /// <param name="format">广播数据格式</param>
        /// <param name="args">格式参数</param>
        public void WriteToBroadcast(int port, string format, params object[] args)
        {
            WriteToBroadcast(port, string.Format(format, args));
        }

        /// <summary>
        /// 广播数据
        /// </summary>
        /// <param name="port">广播端口</param>
        /// <param name="data">广播数据</param>
        public void WriteToBroadcast(int port, string data)
        {
            var dataBuilder = new StringBuilder();
            dataBuilder.AppendFormat("{0}{1}", data, "<EOF>");
            var dataToSend = Encoding.UTF8.GetBytes(dataBuilder.ToString());
            try
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                socket.SendTo(dataToSend, new IPEndPoint(IPAddress.Broadcast, port));
            }
            catch (Exception ex)
            {
                throw new SocketException(ex.ToString());
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (DoBeforDispose != null)
            {
                DoBeforDispose();
            }
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                // 清理托管资源
            }
            // 清理非托管资源
            try
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            catch
            {

            }
            finally
            {
                socket.Close();
                disposed = true;
            }
            if (DoAfterDispose != null)
            {
                DoAfterDispose();
            }
        }
    }
}
