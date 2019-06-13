using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 位操作
    /// </summary>
    public class ByteBitOperator
    {
        private int data = 0;

        /// <summary>
        /// value in byte
        /// </summary>
        public byte ByteValue
        {
            get { return (byte)data; }
        }

        /// <summary>
        /// 0为基数，第一个位
        /// </summary>
        public int Zero
        {
            get
            {
                return GetValue(0);

            }
            set
            {
                SetValue(0, value);
            }
        }

        /// <summary>
        /// 0为基数，第二个位
        /// </summary>
        public int One
        {
            get
            {
                return GetValue(1);
            }
            set
            {
                SetValue(1, value);                
            }
        }

        /// <summary>
        /// 0为基数，第三个位
        /// </summary>
        public int Two
        {
            get
            {
                return GetValue(2);
            }
            set
            {
                SetValue(2, value);
            }
        }

        /// <summary>
        /// 0为基数，第四个位
        /// </summary>
        public int Three
        {
            get
            {
                return GetValue(3);
            }
            set
            {
                SetValue(3, value);
            }
        }

        /// <summary>
        /// 0为基数，第五个位
        /// </summary>
        public int Four
        {
            get
            {
                return GetValue(4);
            }
            set
            {
                SetValue(4, value);
            }
        }

        /// <summary>
        /// 0为基数，第六个位
        /// </summary>
        public int Five
        {
            get
            {
                return GetValue(5);
            }
            set
            {
                SetValue(5, value);
            }
        }

        /// <summary>
        /// 0为基数，第七个位
        /// </summary>
        public int Six
        {
            get
            {
                return GetValue(6);
            }
            set
            {
                SetValue(6, value);
            }
        }

        /// <summary>
        /// 0为基数，第八个位
        /// </summary>
        public int Seven
        {
            get
            {
                return GetValue(7);
            }
            set
            {
                SetValue(7, value);
            }
        }

        private int GetValue(int pos)
        {
            return (data & (int)Math.Pow(2, pos)) >> pos;
        }

        private void SetValue(int pos, int value)
        {
            if (value != 1 && value != 0)
            {
                Console.WriteLine("Value should be 1 or 0.");
            }
            else
            {
                var tmpValue = (int)Math.Pow(2, pos);
                if (value == 1)
                {
                    data = data | tmpValue;
                }
                else
                {
                    if ((data >> pos & 1) == 1)
                    {
                        data -= tmpValue;
                    }
                }
            }
        }

        /// <summary>
        /// 生成一个可以进行位操作的实例
        /// </summary>
        /// <param name="data"></param>
        public ByteBitOperator(byte data)
        {
            this.data = data;
        }
    }
}
