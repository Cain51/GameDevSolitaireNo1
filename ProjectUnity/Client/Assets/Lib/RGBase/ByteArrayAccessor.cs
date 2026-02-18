using System.Collections;
using System.Text;


namespace RG.Basic
{
    public class ByteArrayAccessor
    {
        Seq<byte> _val = null;

        int arrayIndex = 0;

        int bitIndex = 0;

        public Seq<byte> Val { get { return _val; } set { _val = value; SetReadPositionToHead() ; } }

        public byte this[int index] { get { return _val[index]; } set { _val[index] = value; } }

        public int GetBit(int index) { return (_val[index / 8] >> (index % 8)) & 1; }

        public ByteArrayAccessor SetBit0(int index)
        {
            _val[index / 8] &= (byte)(~(1 << (index % 8)));
            return this;
        }

        public ByteArrayAccessor SetBit1(int index)
        {
            _val[index / 8] |= (byte)((1 << (index % 8)));
            return this;
        }

        public ByteArrayAccessor SetReadPositionToHead()
        {
            arrayIndex = 0;
            bitIndex = 0;
            return this;
        }

        public int ReadNext(int length)
        {
            int v = 0;
            int i = 0;
            while(length - i >= 8 - bitIndex)
            {
                v |= (_val[arrayIndex] >> bitIndex) << i;
                i += 8 - bitIndex;
                bitIndex = 0;
                arrayIndex++;
            }
            if(length > i)
            {
                v |= (_val[arrayIndex] & ((1 << (length - i)) - 1)) << i;
                bitIndex = length - i;
            }
            return v;
        }

        public int ReadNextInt() { return ReadNext(32); }

        public int ReadNextShort() { return ReadNext(16); }

        public int ReadNextByte() { return ReadNext(8); }

		public string ReadString()
		{
			// 读取字符串的长度
			int length = ReadNextInt();

			// 读取对应长度的字节数组
			byte[] bytes = new byte[length];
			for (int i = 0; i < length; i++)
			{
				bytes[i] = (byte)ReadNextByte();
			}

			// 将字节数组转换为字符串
			string str = Encoding.UTF8.GetString(bytes);

			return str;
		}

		public ByteArrayAccessor Write(int v,int length)
        {
            int i = 0;
            while (length - i >= 8 - bitIndex)
            {
                if (arrayIndex < _val.Count)
                {
                    if (bitIndex > 0) _val[arrayIndex] = (byte)((_val[arrayIndex] & (1 << bitIndex) - 1) | ((v >> i) << bitIndex));
                    else _val[arrayIndex] = (byte)(v >> i);
                }
                else
                {
                    _val.Add((byte)(v >> i));
                }
                i += (8 - bitIndex);
                bitIndex = 0;
                arrayIndex++;
            }
            if(i < length)
            {
                if (arrayIndex < _val.Count)
                {
                    if (bitIndex > 0) _val[arrayIndex] = (byte)((_val[arrayIndex] & (1 << bitIndex) - 1) | ((v >> i) << bitIndex));
                    else _val[arrayIndex] = (byte)(v >> i);
                }
                else
                {
                    _val.Add((byte)(v >> i));
                }
                bitIndex += length - i;
            }
            return this;
        }

        public ByteArrayAccessor WriteInt(int v)
        {
            return Write(v, 32);
        }

        public ByteArrayAccessor WriteShort(int v)
        {
            return Write(v, 16);
        }

        public ByteArrayAccessor WriteByte(int v)
        {
            return Write(v, 8);
        }

		public ByteArrayAccessor WriteString(string str)
		{
			// 将字符串转换为 UTF-8 编码的字节数组
			byte[] bytes = Encoding.UTF8.GetBytes(str);

			// 写入字符串的长度
			WriteInt(bytes.Length);

			// 逐字节写入字符串内容
			foreach (byte b in bytes)
			{
				WriteByte(b);
			}

			return this;
		}

		public bool AtTheEnd() {
            //UnityEngine.Debug.Log("ai:" + arrayIndex + " " + bitIndex + " " + _val.Count + " "+ (arrayIndex * 8 + bitIndex >= _val.Count * 8));
            return arrayIndex * 8 + bitIndex >= _val.Count * 8;
        }
    }
}
