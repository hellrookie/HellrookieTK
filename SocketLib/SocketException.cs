namespace Hellrookie.ToolKit.SocketLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Socket 异常
    /// </summary>
    public class SocketException : Exception
    {
        /// <summary>
        /// construct
        /// </summary>
        /// <param name="msg"></param>
        public SocketException(string msg)
            : base(msg)
        {
            
        }
    }

    /// <summary>
    /// socket为空的异常
    /// </summary>
    public sealed class NullSocketException : SocketException
    {
        /// <summary>
        /// 构造NullSocketException
        /// </summary>
        /// <param name="msg"></param>
        public NullSocketException(string msg)
            : base(msg)
        {
            
        }
    }

    /// <summary>
    /// socket的address非法
    /// </summary>
    public sealed class InvalidSocketAddressException : SocketException
    {
        /// <summary>
        /// 非法的address
        /// </summary>
        public string InvalidAddress { get; private set; }
        /// <summary>
        /// 构造InvalidSocketAddressException
        /// </summary>
        /// <param name="msg">exception的message</param>
        public InvalidSocketAddressException(string msg)
            : base(msg)
        {
            InvalidAddress = string.Empty;
        }

        /// <summary>
        /// 构造InvalidSocketAddressException
        /// </summary>
        /// <param name="msg">exception的message</param>
        /// <param name="invalidAddress">非法的address</param>
        public InvalidSocketAddressException(string msg, string invalidAddress)
            : this(msg)
        {
            InvalidAddress = invalidAddress;
        }
    }
}
