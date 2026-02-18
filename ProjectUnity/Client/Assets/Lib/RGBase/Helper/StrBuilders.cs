using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace RG.Basic.Helper {
    public static class StrGen {
         
        private static Seq<StringBuilder> builderPool = new Seq<StringBuilder>();

        private static Map<int, StringBuilder> builderInUse = new Map<int, StringBuilder>();
        private static int firstAvaliableInd = 0;

        public static int CountInPool { get { return builderPool.Count;} }
        public static int CountInUse { get { return builderInUse.Count; } }

        public static Builder GetBuilder() {
            if (builderPool.Count <= 0) builderPool.QueuePush(new StringBuilder());
            StringBuilder builder = builderPool.QueuePop();
            while (builderInUse.ContainsKey(firstAvaliableInd)) firstAvaliableInd++;
            builderInUse.Add(firstAvaliableInd, builder);
            return new Builder(firstAvaliableInd ++ );
        }

        public static Builder New { get {return GetBuilder(); } }

        public static Builder Start(string value, params object[] args) {
            if (args == null || args.Length <= 0) return GetBuilder().Append(value);
            else if (value.Exist()) return GetBuilder().AppendFormat(value, args);
            else return GetBuilder();
        }

        public static bool CollectBuilder(int id) {
            if (id < 0 || !builderInUse.ContainsKey(id)) return false;
            StringBuilder _sb = builderInUse[id];
            _sb.Remove(0, _sb.Length);
            builderInUse.Remove(id);
            if(!builderPool.Contains(_sb)) builderPool.QueuePush(_sb);
            if (id < firstAvaliableInd) firstAvaliableInd = id;
            return true;
        }

        public sealed class Builder : IDisposable {
            internal Builder(int _id) { id = _id; }

            public delegate Setter Setter(object obj);
             

            ~Builder() { RecycleSelf(); } 
            public void Dispose() { RecycleSelf(); }

            private void RecycleSelf() {
                if (id < 0 || sb == null) return;
                CollectBuilder(id);
                id = -1;
            }

            public int id { get; private set; }

            internal StringBuilder sb { get { if (id < 0) return null; else return builderInUse.GetIfExist(id, null); } }

            public string GetStringAndRelease() {
                string str = ToString();
                if (str != null) RecycleSelf();
                return str;
            }

            public int Length { get { return sb != null ? sb.Length : 0; } }

            public string End {
                get { return GetStringAndRelease(); }
            }
            public string Clear()
            {
                string ret = ToString();
                Remove(0, Length);
                return ret;
            }
            //========================================= For Fun

            public Builder this[object value] { get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[char[] value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[ulong value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[uint value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[ushort value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[decimal value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[double value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[long value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[int value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[short value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[char value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[float value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[sbyte value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[bool value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[byte value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            public Builder this[string value] {
                get {
                    if (id < 0 || sb == null) return this;
                    sb.Append(value);
                    return this;
                }
            }
            //=====================


            public Builder Append(object value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(char[] value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(ulong value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(uint value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(ushort value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(decimal value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(double value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(long value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(int value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(short value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(char value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(float value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(sbyte value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(bool value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(byte value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(string value) {
                if (id < 0 || sb == null) return this;
                sb.Append(value);
                return this;
            }
            public Builder Append(char value, int repeatCount) {
                if (id < 0 || sb == null) return this;
                sb.Append(value, repeatCount);
                return this;
            }
            public Builder Append(string value, int startIndex, int count) {
                if (id < 0 || sb == null) return this;
                sb.Append(value, startIndex, count);
                return this;
            }
            public Builder Append(char[] value, int startIndex, int charCount) {
                if (id < 0 || sb == null) return this;
                sb.Append(value, startIndex, charCount);
                return this;
            }
            public Builder AppendFormat(string format, object arg0) {
                if (id < 0 || sb == null) return this;
                sb.AppendFormat(format, arg0);
                return this;
            }
            public Builder AppendFormat(string format, params object[] args) {
                if (id < 0 || sb == null) return this;
                sb.AppendFormat(format, args);
                return this;
            }
            public Builder AppendFormat(string format, object arg0, object arg1) {
                if (id < 0 || sb == null) return this;
                sb.AppendFormat(format, arg0, arg1);
                return this;
            }
            public Builder AppendFormat(IFormatProvider provider, string format, params object[] args) {
                if (id < 0 || sb == null) return this;
                sb.AppendFormat(provider, format, args);
                return this;
            }
            public Builder AppendFormat(string format, object arg0, object arg1, object arg2) {
                if (id < 0 || sb == null) return this;
                sb.AppendFormat(format, arg0, arg1, arg2);
                return this;
            }
            public Builder AppendLine() {
                if (id < 0 || sb == null) return this;
                sb.AppendLine();
                return this;
            }
            public Builder AppendLine(string value) {
                if (id < 0 || sb == null) return this;
                sb.AppendLine(value);
                return this;
            }
            public Builder CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) {
                if (id < 0 || sb == null) return this;
                sb.CopyTo(sourceIndex, destination, destinationIndex, count);
                return this;
            }

            public Builder Insert(int index, ushort value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, object value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, ulong value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, uint value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, decimal value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, sbyte value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, float value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, double value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, bool value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, byte value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, short value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, string value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, char[] value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, int value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, long value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, char value) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value);
                return this;
            }
            public Builder Insert(int index, string value, int count) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value, count);
                return this;
            }
            public Builder Insert(int index, char[] value, int startIndex, int charCount) {
                if (id < 0 || sb == null) return this;
                sb.Insert(index, value, startIndex, charCount);
                return this;
            }
            public Builder Remove(int startIndex, int length) {
                if (id < 0 || sb == null) return this;
                sb.Remove(startIndex, length);
                return this;
            }
            public Builder Replace(string oldValue, string newValue) {
                if (id < 0 || sb == null) return this;
                sb.Replace(oldValue, newValue);
                return this;
            }
            public Builder Replace(char oldChar, char newChar) {
                if (id < 0 || sb == null) return this;
                sb.Replace(oldChar, newChar);
                return this;
            }
            public Builder Replace(string oldValue, string newValue, int startIndex, int count) {
                if (id < 0 || sb == null) return this;
                sb.Replace(oldValue, newValue, startIndex, count);
                return this;
            }
            public Builder Replace(char oldChar, char newChar, int startIndex, int count) { 
                if (id < 0 || sb == null) return this;
                sb.Replace(oldChar, newChar, startIndex, count);
                return this;
            }

            public string Get(int startIndex, int length) { 
                if (id < 0 || sb == null) return null;
                return sb.ToString(startIndex, length);
            }

            public override string ToString() {
                if (id < 0 || sb == null) return null;
                return sb.ToString();
            }

            public int EnsureCapacity(int capacity) { 
                if (id < 0 || sb == null) return -1;
                return sb.EnsureCapacity(capacity);
            }
            public bool Equals(Builder sb) {
                if (id < 0 || sb == null) return false;
                return sb.Equals(sb.sb);
            }

        }

    }
}
