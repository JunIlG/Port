using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    public enum PacketType
    {
        Handshake = 1,
        Message = 2
    }

    public class Packet : IDisposable
    {
        private List<byte> buffer = new List<byte>();
        private byte[] readableBuffer;
        private int readPos = 0;
        private bool disposed = false;

        public Packet()
        {
        }

        public Packet(PacketType type)
        {
            Write((int)type);
        }

        public Packet(byte[] data)
        {
            SetBytes(data);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Utitlity Methods
        public void SetBytes(byte[] data)
        {
            Write(data);
            readableBuffer = buffer.ToArray();
        }

        public void InsertInt(int data)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(data));
        }

        public int Length()
        {
            return buffer.Count;
        }

        public int UnreadLength()
        {
            return Length() - readPos;
        }

        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public void Reset(bool shouldReset = true)
        {
            if (shouldReset)
            {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            }
            else
            {
                // Unread last read int
                readPos -= 4;
            }
        }
        #endregion

        #region Write Methods
        public void Write(byte[] data)
        {
            buffer.AddRange(data);
        }

        public void Write(int data)
        {
            buffer.AddRange(BitConverter.GetBytes(data));
        }

        public void Write(string data)
        {
            Write(data.Length);
            buffer.AddRange(Encoding.UTF8.GetBytes(data));
        }

        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }
        #endregion

        #region Read Methods
        public byte[] ReadBytes(int length, bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                byte[] data = buffer.GetRange(readPos, length).ToArray();

                if (moveReadPos)
                {
                    readPos += length;
                }

                return data;
            }
            else
            {
                throw new Exception("Attempted to read byte[] but failed");
            }
        }

        public int ReadInt(bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                int data = BitConverter.ToInt32(readableBuffer, readPos);

                if (moveReadPos)
                {
                    readPos += 4;
                }

                return data;
            }
            else
            {
                throw new Exception("Attempted to read int but failed");
            }
        }

        public string ReadString(bool moveReadPos = true)
        {
            try 
            {
                int length = ReadInt(moveReadPos);
                int pos = moveReadPos ? readPos : readPos + 4;
                string data = Encoding.UTF8.GetString(readableBuffer, pos, length);

                if (moveReadPos)
                {
                    readPos += length;
                }

                return data;
            }
            catch
            {
                throw new Exception("Attempted to read string but failed");
            }

        }
        #endregion
    }
}