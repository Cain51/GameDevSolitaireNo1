using RG.Basic.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RG.Basic.DataType
{
    [Serializable]
    public struct ID : IFormattable, IComparable, IComparable<ID>, IEquatable<ID>
    {
        public const string INVALID_OPEN_ID = "-"; //不能为space, 因为space的默认情况为root
        public static readonly ID Empty = new ID(INVALID_OPEN_ID, 0);

        private string open_id;

        private string mark_id;

        private static string default_open_id = "";
        
        public static void Reset() {
            Default_open_id = "";
        }

        public static string Default_open_id
        {
            get { return default_open_id; }
            set { default_open_id = value; }
        }

        public string OpenID { get { return open_id.Exist() ? open_id : default_open_id; } }

        public string MarkID { get { return mark_id; } }

        public static bool Exist(ID id) { return id != null && id != Empty; }

        public ID(string _open_id, long _mark_id)
        {
            open_id = null != _open_id ? _open_id : "";
            //if (_open_id != null) ...
            //else open_id = INVALID_OPEN_ID;
            mark_id = _mark_id.ToString();
        }

        public ID(string _open_id, string _mark_id) {
            open_id = null != _open_id ? _open_id : "";
            mark_id = _mark_id;
        }

        public int CompareTo(object obj)
        {
            if (obj is ID) return CompareTo((ID)obj);
            return OpenID.CompareTo(obj);
        }

        public int CompareTo(ID other)
        {
            if (other == null) other = Empty;
            int ret = OpenID.CompareTo(other.OpenID);
            if (ret != 0) return ret;
            else return mark_id.CompareTo(other.mark_id);
        }

        

        public bool Equals(ID other) { return CompareTo(other) == 0; }

        public string ToString(string format, IFormatProvider formatProvider) { return string.Format(formatProvider, format, ToString()); }

        public string ToBraceString()
        {
            char[] chars = new char[26]; chars[0] = '{'; chars[25] = '}';
            Convert.ToBase64CharArray(ToByteArray(), 0, 16, chars, 1);
            return new string(chars);
        }


        static UTF8Encoding eutf8 = new UTF8Encoding(false);
        public byte[] ToByteArray() { return eutf8.GetBytes(OpenID); }

        public override string ToString() {
            return StrGen.New[OpenID][':'][MarkID].End;
        }
        //{ return StrGen.New[open_id.Exist() ? open_id : ""][':'][MarkID].End; }

        public override bool Equals(object obj) { return base.Equals(obj); }

        public override int GetHashCode() { return ToString().GetHashCode(); }

        public static ID New { get { return new ID(Guid.NewGuid().ToString(), 0); } }


        public static bool operator ==(ID uuid1, ID uuid2)
        {
            return Compare(uuid1, uuid2);
        }

        public static bool operator !=(ID uuid1, ID uuid2)
        {
            return !Compare(uuid1, uuid2);
        }

        private static bool Compare(ID uuid1, ID uuid2) { return uuid1.CompareTo(uuid2) == 0; }

        public static ID Parse(string open_id, string mark_id) { return new ID(open_id, mark_id); }

        public static ID ParseString(string id) {
            if (!id.Exist()) return Empty;
            int ind = id.LastIndexOf(':');
            if (ind < 1) return Empty;
            return new ID(id.Substring(0, ind), id.Substring(ind + 1).TryConvertToLong());
        }

        public ID Append(string subId) { open_id = StrGen.New[OpenID]['.'][subId].End; return this; }

        public static implicit operator string(ID id) { return id.ToString(); }
    }
}
