using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RGBase {

    public class StreamBrokenException : Exception {
        public StreamBrokenException() : base("The stream is broken") {

        }
    }

    public class StreamFullException : Exception {
        public StreamFullException(string msg) : base(msg) {

        }
    }

    // 存储为双端队列
    // 手动调用，移动到下一Part
    // Count字节会在队列首部
    // 超出最大队列范围会报错
    public class PartStream {
        public static DateTime startTime;
        private static object statisMutex = new object();
        public static volatile int ReceiveCount;
        public static volatile int ReceiveByte;
        public static volatile int MaxChunk;
        private int _nonZHead;
        private int _nonZTail;
        private int _nonZCurCount;

        private string _resetSet = "";
        public string GetState(bool containReset = true) {
            if (containReset)
                return string.Format("Head:{0},Tail:{1},ChunkCount:{2}\n{3}", _head, _tail, _firstCount,_resetSet);
            else
                return string.Format("Head:{0},Tail:{1},ChunkCount:{2}", _head, _tail, _firstCount);
        }

        public static void IncReceiveCount() {
            lock(statisMutex) {
                if (ReceiveCount == 0) {
                    startTime = DateTime.Now;
                }
                ReceiveCount++;
            }
        }

        public static void AddReceiveByte(int count) {
            lock(statisMutex) {
                ReceiveByte += count;
            }
        }

        public static void SetMaxChunk(int count) {
            lock(statisMutex) {
                if (count > MaxChunk) {
                    MaxChunk = count;
                }
            }
        }

        private Streamer _streamer;
        public Streamer GetStreamer() {
            lock(mutex) {
                if (_streamer == null) {
                    if (HasStream()) {
                        return new Streamer(this);
                    }
                }
                return null;
            }
           
        }
        public void ReleaseStreamer(Streamer stream) {
            lock(mutex) {
                if (_streamer == stream)
                    _streamer = null;
            }
        }

        public readonly int STREAM_CAPCITY;
        public PartStream(int capcity) {
            STREAM_CAPCITY = capcity;
            _buf = new byte[STREAM_CAPCITY];
        }
        private byte[] _buf;
        private object mutex = new object();
        
        #region 双端队列帮助函数
        // 计算任意两个头尾的距离
        private int CalcCount(int head, int tail) {
            if (head == tail) return 0;
            if (head < tail) {
                return tail - head;
            }
            return (STREAM_CAPCITY - head) + _tail;
        }

        // 循环增加
        private int CycleInc(int pos, int offset = 1) {
            var toTail = STREAM_CAPCITY - pos - 1;
            if (toTail < offset) {
                return offset - toTail;
            } else {
                return pos + offset;
            }
        }

        // 循环减少
        private int CycleMinus(int pos, int offset = 1) {
            var toHead = pos;
            if (toHead < offset) {
                return STREAM_CAPCITY - offset + toHead;
            } else {
                return pos - offset;
            }

        }
        #endregion

        #region 双端队列操作
        private int _head = 0;          // 首个元素
        private int _tail = 0;          // 最后一个元素的后一个



        // 队列总元素数
        public int TotalCount() {
            lock(mutex) {
                return CalcCount(_head, _tail);
            }
        }

        // 清空队列
        private void Clear() {
            lock(mutex) {
                _head = _tail = 0;
            }
        }
        // 队列是否满了
        public bool IsFull() {
            return TotalCount() >= STREAM_CAPCITY - 1;
        }

        // 队列是否是空的
        private bool IsEmpty() {
            return _head == _tail;
        }

        // 队列中放一个元素
        private bool Put(byte b) {
            if (IsFull()) {
                return false;
            } else {
                _buf[_tail] = b;
                _tail = CycleInc(_tail);
                return true;
            }
        }

        private bool Put(byte[] bytes) {
            if (CalcCount(_head, CycleInc(_tail, bytes.Length)) >= STREAM_CAPCITY - 1) {
                return false;
            }
            for (int i = 0; i < bytes.Length; i++) {
                Put(bytes[i]);
            }
            return true;
        }

        // 查看但不弹出消息
        private byte Peek() {
            if (IsEmpty()) {
                throw new Exception("The buf is Empty!");
            }
            return _buf[_head];
        }



        // 弹出最先插入的元素
        private byte Pop() {
            var b = Peek();
            _head = CycleInc(_head);
            return b;
        }
        #endregion

        #region 添加数据的操作

        private int _curHead;
        private int _curCount;
        private int _lastCompleteHead = 0;
        private int _lastCompleteCount;
        private int _lastCompleteTail = 0;

        public void TryFinishChunk() {
            lock(mutex) {
                // 上一个消息没有读完
                if (!IsChunkFull()) {
                    // 将指针重置到上一个完成的位置
                    _tail = _lastCompleteTail;
                } else {
                    _lastCompleteHead = _curHead;
                    _lastCompleteCount = _curCount;
                    _lastCompleteTail = _tail;
                }
            }
            
        }

        // 标记一个Chunk开始写入
        public bool StartChunk(byte[] countBuf, int count) {
            lock(mutex) {
                TryFinishChunk();
                _curHead = _tail;
                _curCount = count;
                if (_firstCount <= 0) {
                    _firstCount = count;
                }
                // 推入count
                if (!Put(countBuf)) {
                    return false;
                }
                IncReceiveCount();
                AddReceiveByte(count);
                SetMaxChunk(count);
                return true;
            }
            
        }
        private volatile bool _streamBroken;
        // 往Chunk写入数据
        public bool PutChunk(byte[] data) {
            lock(mutex) {
                if (_streamBroken) {
                    return false;
                }
                if (IsChunkFull()) {
                    _streamBroken = true;
                    throw new StreamFullException(string.Format("The chunk is full. But you still put data into it._curHead:{0} _curCount:{1} _tail:{2}",_curHead,_curCount, _tail));
                }
                return Put(data);
            }
        }

        // 当前Chunk是否写满
        public bool IsChunkFull() {
            lock(mutex) {
                if (CalcCount(_curHead, _tail) >= _curCount+4) {
                    return true;
                }
                return false;
            }
        }

        // 这个操作会损坏Stream
        public void ClearAllChunk() {
            lock(mutex) {
                if (_streamer != null) {
                    _streamer.Break();
                    _streamer = null;
                }
                _curHead = 0;
                _curCount = 0;
                _lastCompleteCount = 0;
                _lastCompleteHead = 0;
                _lastCompleteTail = 0;
                _firstCount = 0;
                Clear();
                
            }
        }
        #endregion

        #region 提取stream
        private int _firstCount = 0;
        public bool HasStream() {
            lock(mutex) {
                if (_firstCount <= 0) return false;
                if (_lastCompleteCount <= 0) return false;
                var dis = CalcCount(_head, _lastCompleteHead);
                if (dis > 0) {
                    _resetSet = GetState(false)+"--"+DateTime.Now;
                    return true;
                } else {
                    // 最后一个完成的块就是头部
                    if (_lastCompleteCount > 0) {
                        _resetSet = GetState(false)+"--"+DateTime.Now;
                        return true;
                    }
                }
                return false;
            }
        }

        private int PeekCount() {
            int pos = _head;
            int count = 0;
            for (int i = 0; i < 4; i++) {
                count += _buf[pos] << (24 - 8 * i);
                pos = CycleInc(pos);
            }
            return count;
        }

        // 这个操作会损坏Stream
        public void SkipChunk() {
            lock(mutex) {
                if (_streamer != null) {
                    _streamer.Break();
                    _streamer = null;
                }
                if (!HasStream()) {
                    throw new Exception("You are trying skip stream. But their are no available stream.");
                }
                var dis = CalcCount(_head, _lastCompleteHead);
                _head = CycleInc(_head, _firstCount + 4);
                if (dis > 0) {
                    _firstCount = PeekCount();
                } else {
                    _lastCompleteHead = _head;
                    _lastCompleteCount = 0;
                    _lastCompleteTail = _tail;
                    if (TotalCount() < 4) {
                        _firstCount = 0;
                    }
                }
            }
        }
        #endregion

        #region Stream相关的帮助函数
        private int PosToIndex(int pos) {
            return CycleInc(_head + 4, pos);
        }

        private int IndexToPos(int index) {
            return CalcCount(index, _head + 4);
        }

        private bool GetByPos(long pos, out byte b) {
            if(pos >= _firstCount) {
                b = 0;
                return false;
            } else {
                b = _buf[PosToIndex((int)pos)];
                return true;
            }
        }
        #endregion

        public class Streamer : Stream {
            private PartStream _outer;
            public Streamer(PartStream outer) {
                _outer = outer;
            }

            public override bool CanRead {
                get {
                    lock (_outer.mutex) {
                        if (_broken) {
                            throw new StreamBrokenException();
                        }
                        return Length > 0;
                    }
                }
            }
            private long _streamPos = 0;
            public override bool CanSeek { get { throw new NotImplementedException(); } }

            public override bool CanWrite { get { throw new NotImplementedException(); } }

            public override long Length {
                get {
                    lock (_outer.mutex) {
                        if (_broken) {
                            throw new StreamBrokenException();
                        }
                        return _outer._firstCount - (int)_streamPos;
                    }
                }
            }

            public override long Position {
                get {
                    lock (_outer.mutex) {
                        if (_broken) {
                            throw new StreamBrokenException();
                        }
                        return _streamPos;
                    }

                }
                set {
                    lock (_outer.mutex) {

                        if (_broken) {
                            throw new StreamBrokenException();
                        }
                        _streamPos = value;
                    }
                }
            }

            public override void Flush() {
                throw new NotImplementedException();
            }

            public byte[] GetAll() {
                lock(_outer.mutex) {
                    if (_broken) {
                        throw new StreamBrokenException();
                    }
                    byte[] buf = new byte[_outer._firstCount];
                    for(int i =0; i< _outer._firstCount; i++) {
                        buf[i] = _outer._buf[_outer._head + i+4];
                    }
                    return buf;
                }
            }

            public override int Read(byte[] buffer, int offset, int count) {
                lock (_outer.mutex) {
                    if (_broken) {
                        throw new StreamBrokenException();
                    }
                    for (int i = offset; i < offset + count; i++) {
                        byte b;
                        if (_outer.GetByPos(_streamPos, out b)) {
                            buffer[i] = b;
                            _streamPos++;
                        } else {
                            return i - offset;
                        }
                    }
                    return count;
                }
            }

            private bool _broken;

            public void Break() {
                _broken = true;
            }

            public override long Seek(long offset, SeekOrigin origin) {
                lock (_outer.mutex) {
                    if (_broken) {
                        throw new StreamBrokenException();
                    }
                    long pos = 0;
                    switch (origin) {
                        case SeekOrigin.Begin:
                            pos = offset;
                            break;
                        case SeekOrigin.Current:
                            pos += offset;
                            break;
                        case SeekOrigin.End:
                            pos = _outer.IndexToPos(_outer._head + _outer._firstCount) + offset;
                            break;
                    }
                    _streamPos = pos;
                    return pos;
                }
            }

            public override void SetLength(long value) {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count) {
                throw new NotImplementedException();
            }

            public void Release() {
                lock(_outer.mutex) {
                    _outer.ReleaseStreamer(this);
                }
            }

            public void Skip() {
                lock(_outer.mutex) {
                    _outer.SkipChunk();
                }
            }
        }
        
    }
}
