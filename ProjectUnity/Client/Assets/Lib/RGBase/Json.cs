using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using RG.Basic;

namespace RG.Basic {
    public class Json {

        public static readonly Json Empty = new Json();

        public virtual bool isSeq { get; protected set; }
        public Map<string, object> Data { get { if(data == null) data = new Map<string, object>(); return data; } }
        public Seq<object> SeqData { get { if(seqData == null) { seqData = new Seq<object>(); } return seqData; } }
        protected Map<string, object> data = null;
        protected Seq<object> seqData = null;

        public int Count { get { return isSeq ? SeqData.Count : Data.Count; } }

        public Json Add(string name, object value) {
            if(HasChild(name)) throw new JsonException("[Json Add] the Name (" + name + ") is already exist. ");
            Data[name] = value;
            return this;
        }
        public Json Add(object value) { isSeq = true; SeqData.Add(value); return this; }

        public bool Del(string key)
        {
            if (isSeq) return false;
            if(data.ContainsKey(key))
            {
                data.Remove(key);
                return true;
            }
            return false;
        }

        public bool Del(int ind)
        {
            if (!isSeq) return false;
            if (isSeq && seqData.Count>ind)
            {
                SeqData.RemoveAt(ind);
                return true;
            }
            return false;
        }

        public virtual object GetByKey(string key) {
            if(!Data.ContainsKey(key)) throw new JsonException("[Json GetByKey] key(" + key + ") is out of range. ");
            return Data[key];
        }
        public virtual object this[string key] {
            get { return GetByKey(key); }
            protected set { Data[key] = value; }
        }


        public virtual object GetByIndex(int index) {
            //if(index < 0 || index > Count) throw new JsonException("[Json GetByIndex] index(" + index + ") is out of range. "); //这种判断其实很影响性能
            return SeqData[index];
        }
        public virtual object this[int index] {
            get { return GetByIndex(index); }
            protected set { SeqData[index] = value; }
        }

        public Json CreateChild(string key, bool _isSeq = false) {
            if(HasChild(key)) throw new JsonException("[Json CreateChild] the key (" + key + ") is already exist. ");
            Json j = new Json();
            Add(key, j);
            j.isSeq = _isSeq;
            return j;
        }

        public Json CreateSeqChild() {
            Json j = new Json(); 
            Add(j);
            return j;
        }

        protected Json() { isSeq = false; }

        public static Json Create() { return new Json(); }
        public static Json Create(string code) { return Deserializer.Deserialize(code) as Json; }
        public static Json Create(Map<string, object> _data) { return new Json() { data = _data }; }
        public static Json Create(Seq<object> _seqData) { return new Json() { seqData = _seqData, isSeq = true }; }

        public static implicit operator Json(Map<string, object> _data) { return Create(_data); }
        public static implicit operator Json(List<object> _data) { return Create(new Seq<object>(_data)); }

        public Json DoMap(Action<string, object> Executor) {
            if(Executor == null) throw new JsonException("[Json DoMap] empty executor");
            if(isSeq) throw new JsonException("[Json DoMap] cannot apply DoMap to a seq json");
            else foreach(KeyValuePair<string, object> pair in Data) Executor(pair.Key, pair.Value);
            return this;
        }

        public Json DoSeq(Action<int, object> Executor) {
            if(Executor == null) throw new JsonException("[Json DoValue] empty executor");
            if(isSeq) for(int i = 0; i < SeqData.Count; i++) { Executor(i, SeqData[i]); } else throw new JsonException("[Json DoMap] cannot apply DoSeq to a map json");
            return this;
        }

        public virtual bool HasChild(string key) {
            return Data.ContainsKey(key);
        }

        public object[] Keys {
            get {
                if(isSeq) {
                    Seq<object> _keys = new Seq<object>(null, SeqData.Count);
                    for(int i = 0; i < SeqData.Count; i++) _keys[i] = i;
                    return _keys.AsArray;
                } else {
                    return Data.KeySeq.Map<object>(o => o).AsArray;
                }
            }
        }


        public virtual object Get(params object[] keys) {
            object m = this;
            for(int i = 0; i < keys.Length; i++) {
                if (keys[i] is string) {
                    m = (m as Json)[keys[i] as string];
                }  else if (keys[i] is int) {
                    m = (m as Json)[(int)keys[i]];
                } else if (keys[i] is float) {
                    m = (m as Json)[Convert.ToInt32((float)keys[i])];
                } else if (keys[i] is double) {
                    m = (m as Json)[Convert.ToInt32((double)keys[i])];
                } else throw new JsonException("[Json Get] wrong input for key " + i + " " + keys[i]);
            }
            return m;
        }

        public virtual bool GetBool(params object[] keys) { return JsonUtility.ObjectToBool(Get(keys)); }
        public virtual int GetInt(params object[] keys) { return JsonUtility.ObjectToInt(Get(keys)); }
        public virtual int GetByte(params object[] keys) { return (byte)JsonUtility.ObjectToInt(Get(keys)); }
        public virtual long GetLong(params object[] keys) { return JsonUtility.ObjectToLong(Get(keys)); }
        public virtual float GetFloat(params object[] keys) { return JsonUtility.ObjectToFloat(Get(keys)); }
        public virtual double GetDouble(params object[] keys) { return JsonUtility.ObjectToDouble(Get(keys)); }
        public virtual string GetString(params object[] keys) { return JsonUtility.ObjectToString(Get(keys)); }

        public Json ChildByIndex(int index) { return SeqData.Json(index); }
        public virtual bool BoolByIndex(int index) { return SeqData.Bool(index); }
        public virtual int IntByIndex(int index) { return SeqData.Int(index); }
        public virtual byte ByteByIndex(int index) { return (byte)SeqData.Int(index); }
        public virtual long LongByIndex(int index) { return SeqData.Long(index); }
        public virtual float FloatByIndex(int index) { return SeqData.Float(index); }
        public virtual double DoubleByIndex(int index) { return SeqData.Double(index); }
        public virtual string StringByIndex(int index, string format) { return SeqData.String(index, format); }
        public virtual string StringByIndex(int index) { return SeqData.String(index, null); }

        public Json Child(string key) { return Data.Json(key); }
        public virtual bool Bool(string key) { return Data.Bool(key); }
        public virtual int Int(string key) { return Data.Int(key); }
        public virtual byte Byte(string key) { return (byte)Data.Int(key); }
        public virtual long Long(string key) { return Data.Long(key); }
        public virtual float Float(string key) { return Data.Float(key); }
        public virtual double Double(string key) { return Data.Double(key); }
        public virtual string String(string key, string format) { return Data.String(key, format); }
        public virtual string String(string key) { return Data.String(key, null); }
        public virtual uint Uint(string key) { return Data.Uint(key); }
        //public Color ColorFormInt(string key) { return NGUIMath.IntToColor(data.Int(key)); }

        public virtual bool Null(string key) { return !Data.ContainsKey(key) || Data[key] == null; }

        public override string ToString() { return Serialize(); }

        public virtual string Serialize() {
            return Serializer.Serialize(this);
        }

        public virtual Json SetValue(string key, object value) {
            if(HasChild(key)) this[key] = value;
            else Add(key, value);
            return this;
        }

        public virtual Json SetValue(int index, object value) {
            if(index > Count) throw new JsonException("SetValue Index " + index + " is Larger than Count " + Count);
            else this[index] = value;
            return this;
        }

        public Json CopyFrom(Json json) {
            if (json.isSeq) {
                for(int i = 0; i < json.Count; i++) {
                    object data = json[i];
                    if(data is Json) {
                        this[i] = new Json().CopyFrom(data as Json);
                    } else {
                        this[i] = data;
                    }
                }
                isSeq = true;
            } else {
                json.DoMap((key, data) => {
                    if (data is Json) {
                        this[key] = new Json().CopyFrom(data as Json);
                    } else {
                        this[key] = data;
                    }
                });
            }
            
            return this;
        }

        public virtual Json Clone {
            get {
                Json d;
                if(isSeq) d = Json.Create(SeqData.Clone);
                else d = Json.Create(Data.Clone);
                return d;
            }
        }
         

        //G:感觉可能还是需要一个地方设置一个空的seqJson
        public virtual void SetisSeq(bool _isSeq)
        {
            isSeq = _isSeq;
        }


        public class Deserializer : IDisposable {
            const string WORD_BREAK = "{}[],:\"";

            public static bool IsWordBreak(char c) {
                return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
            }

            enum TOKEN {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            };

            StringReader json;

            Deserializer(string jsonString) {
                json = new StringReader(jsonString);
            }

            public static object Deserialize(string jsonString) {
                try {
                    using (var instance = new Deserializer(jsonString)) {
                        return instance.ParseValue();
                    }
                } catch (Exception ex) {
                    throw new Exception("Json Deserialize Error : [" + jsonString.Length + "| " + jsonString + "] " + ex.Message);
                }
            }

            public void Dispose() {
                json.Dispose();
                json = null;
            }

            Json ParseObject() {
                Json jsonObj = new Json();

                // ditch opening brace
                json.Read();
                 
                while(true) {
                    TOKEN nextToken = NextToken();
                    switch(nextToken) {
                        case TOKEN.NONE: return null;
                        case TOKEN.COMMA: continue;
                        case TOKEN.CURLY_CLOSE: return jsonObj;
                        default:
                            // name
                            string name = ParseString();
                            if(name == null) { return null; }
                            // :
                            nextToken = NextToken();
                            if(nextToken != TOKEN.COLON) { return null; }
                            // ditch the colon
                            json.Read();

                            // value
                            jsonObj[name] = ParseValue();
                            break;
                    }
                }
            }

            Json ParseArray() {
                Json jsonArray = new Json();
                jsonArray.isSeq = true;
                // ditch opening bracket
                json.Read();
                // [
                var parsing = true;
                while(parsing) {
                    TOKEN nextToken = NextToken();

                    switch(nextToken) {
                        case TOKEN.NONE: return null;
                        case TOKEN.COMMA: continue;
                        case TOKEN.SQUARED_CLOSE: parsing = false; break;
                        default:
                            object value = ParseByToken(nextToken);
                            jsonArray.Add(value);
                            break;
                    }
                }

                return jsonArray;
            }

            object ParseValue() {
                TOKEN nextToken = NextToken();
                return ParseByToken(nextToken);
            }

            object ParseByToken(TOKEN token) {
                switch(token) {
                    case TOKEN.STRING: return ParseString();
                    case TOKEN.NUMBER: return ParseNumber();
                    case TOKEN.CURLY_OPEN: return ParseObject();
                    case TOKEN.SQUARED_OPEN: return ParseArray();
                    case TOKEN.TRUE: return true;
                    case TOKEN.FALSE: return false;
                    case TOKEN.NULL: return null;
                    default: return null;
                }
            }

            string ParseString() {
                StringBuilder s = new StringBuilder();
                char c;

                // ditch opening quote
                json.Read();
                bool parsing = true;
                while(parsing) {

                    if(json.Peek() == -1) {
                        parsing = false;
                        break;
                    }

                    c = NextChar;
                    switch(c) {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if(json.Peek() == -1) {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch(c) {
                                case '"':
                                case '\\':
                                case '/': s.Append(c); break;
                                case 'b': s.Append('\b'); break;
                                case 'f': s.Append('\f'); break;
                                case 'n': s.Append('\n'); break;
                                case 'r': s.Append('\r'); break;
                                case 't': s.Append('\t'); break;
                                case 'u':
                                    var hex = new char[4];
                                    for(int i = 0; i < 4; i++) { hex[i] = NextChar; }
                                    s.Append((char)Convert.ToInt32(new string(hex), 16));
                                    break;
                            }
                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            object ParseNumber() {
                string number = NextWord;

                if(number.IndexOf('.') == -1) {
                    long parsedInt;
                    long.TryParse(number, out parsedInt);
                    if(parsedInt > int.MinValue && parsedInt < int.MaxValue) return (int)parsedInt;
                    else return parsedInt;
                }

                double parsedDouble;
                double.TryParse(number, out parsedDouble);
                if(parsedDouble > float.MinValue && parsedDouble < float.MaxValue) return (float)parsedDouble;
                return parsedDouble;
            }

            void EatWhitespace() { 
                while(char.IsWhiteSpace(PeekChar)) {
                    json.Read();
                    if(json.Peek() == -1) break;
                }
            }

            char PeekChar { get { return Convert.ToChar(json.Peek()); } }

            char NextChar { get { return Convert.ToChar(json.Read()); } }

            string NextWord {
                get {
                    StringBuilder word = new StringBuilder();
                    while(!IsWordBreak(PeekChar)) {
                        word.Append(NextChar);
                        if(json.Peek() == -1) break;
                    }
                    return word.ToString();
                }
            }

            TOKEN NextToken() {
                EatWhitespace();
                if(json.Peek() == -1) return TOKEN.NONE; 
                switch(PeekChar) {
                    case '{': return TOKEN.CURLY_OPEN;
                    case '}': json.Read(); return TOKEN.CURLY_CLOSE;
                    case '[': return TOKEN.SQUARED_OPEN;
                    case ']': json.Read(); return TOKEN.SQUARED_CLOSE;
                    case ',': json.Read(); return TOKEN.COMMA;
                    case '"': return TOKEN.STRING;
                    case ':': return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-': return TOKEN.NUMBER;
                }
                switch(NextWord) {
                    case "false":return TOKEN.FALSE;
                    case "true":return TOKEN.TRUE;
                    case "null":return TOKEN.NULL;
                } 
                return TOKEN.NONE;
            }
        }

        public class Serializer {
            StringBuilder builder = new StringBuilder();
            public static Serializer PublicInstance { get { if(_instance == null) _instance = new Serializer(); return _instance; } }
            private static Serializer _instance;

            public const string def_null = "null";
            public const string def_true = "true";
            public const string def_false = "false";

            protected Serializer() { }

            public static string Serialize(object obj) {
                Serializer instance = new Serializer();
                instance.builder.Remove(0, instance.builder.Length);
                instance.SerializeValue(obj);
                return instance.builder.ToString();
            }

            void SerializeValue(object value) {
                if(value == null) {
                    builder.Append(def_null);
                } else if(value is string) {
                    SerializeString(value as string);
                } else if(value is bool) {
                    builder.Append((bool)value ? def_true : def_false);
                } else if(value is Json) {
                    Json jsonObject = value as Json;
                    if(jsonObject.isSeq) SerializeArray(jsonObject);
                    else SerializeObject(jsonObject);
                } else if(value is char) {
                    SerializeStringObj(value);
                } else {
                    SerializeOther(value);
                }
            }

            void SerializeObject(Json obj) {
                bool first = true;

                builder.Append('{');

                //foreach(string e in obj.Data.Keys) {
                //    if(!first) { builder.Append(','); } 
                //    SerializeString(e);
                //    builder.Append(':'); 
                //    SerializeValue(obj[e]); 
                //    first = false;
                //}

                //obj.Data.ForEachPairs((k, v))

                foreach(var p in obj.Data) {
                    if(!first) { builder.Append(','); }
                    SerializeString(p.Key);
                    builder.Append(':');
                    SerializeValue(p.Value);
                    first = false;
                }
                builder.Append('}');
            }

            void SerializeArray(Json anArray) {
                builder.Append('[');
                bool first = true;

                //for(int i = 0; i < anArray.Count; i++) {
                //    if(!first) { builder.Append(','); }
                //    SerializeValue(anArray[i]);
                //    first = false;
                //}

                anArray.SeqData.DoSeq(obj => {
                    if(!first) { builder.Append(','); }
                    SerializeValue(obj);
                    first = false;
                });

                //foreach(object obj in anArray.SeqData) {
                //    if(!first) { builder.Append(','); }
                //    SerializeValue(obj);
                //    first = false;
                //}
                builder.Append(']');
            }

            void SerializeStringObj(object obj) {
                builder.Append('\"');
                if(obj is string) {
                    string str = obj as string;
                    for(int i = 0; i < str.Length; i++) {
                        switch(str[i]) {
                            case '"': builder.Append("\\\""); break;
                            case '\\': builder.Append("\\\\"); break;
                            case '\b': builder.Append("\\b"); break;
                            case '\f': builder.Append("\\f"); break;
                            case '\n': builder.Append("\\n"); break;
                            case '\r': builder.Append("\\r"); break;
                            case '\t': builder.Append("\\t"); break;
                            default:
                                int codepoint = Convert.ToInt32(str[i]);
                                if((codepoint >= 32) && (codepoint <= 126)) {
                                    builder.Append(str[i]);
                                } else {
                                    builder.Append("\\u");
                                    builder.Append(codepoint.ToString("x4"));
                                }
                                break;
                        }
                    }
                } else {
                    builder.Append(obj);
                }
                builder.Append('\"');
            }

            void SerializeString(string str) {
                builder.Append('\"');
                for(int i = 0; i < str.Length; i++) {
                    switch(str[i]) {
                        case '"': builder.Append("\\\""); break;
                        case '\\': builder.Append("\\\\"); break;
                        case '\b': builder.Append("\\b"); break;
                        case '\f': builder.Append("\\f"); break;
                        case '\n': builder.Append("\\n"); break;
                        case '\r': builder.Append("\\r"); break;
                        case '\t': builder.Append("\\t"); break;
                        default:
                            int codepoint = Convert.ToInt32(str[i]);
                            if((str[i] >= 32) && (str[i] <= 126)) {
                                builder.Append(str[i]);
                            } else {
                                builder.Append("\\u");
                                builder.Append(codepoint.ToString("x4"));
                            }
                            break;
                    }
                }

                builder.Append('\"');
            }

            void SerializeOther(object value) {
                // NOTE: decimals lose precision during serialization.
                // They always have, I'm just letting you know.
                // Previously floats and doubles lost precision too.
                if(value is int
                   || value is uint
                   || value is long
                   || value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is ulong) {
                    builder.Append(value);
                } else if(value is float) {
                    builder.Append(((float)value).ToString("R"));
                } else if(value is double
                      || value is decimal) {
                    builder.Append(Convert.ToDouble(value).ToString("R"));
                } else {
                    SerializeStringObj(value);
                }
            }
        }

    }

    internal static class JsonUtility {

        public static object Check(this Dictionary<string, object> map, string key) {
            if(!map.ContainsKey(key)) throw new JsonException("[JsonUtility (Parse)Check] key " + key + " not exist ");
            return map[key];
        }
        public static object Check(this Seq<object> seq, int index) {
            if(index < 0 || index >= seq.Count) throw new JsonException("[JsonUtility (Parse)Check] index " + index + " is out of range ");
            return seq[index];
        }

        public static Json Json(this Dictionary<string, object> map, string key) { return map.Check(key) as Json; }
        public static string String(this Dictionary<string, object> map, string key, string format = null) { return ObjectToString(map.Check(key), format); }
        public static bool Bool(this Dictionary<string, object> map, string key) { return ObjectToBool(map.Check(key)); }
        public static int Int(this Dictionary<string, object> map, string key) { return ObjectToInt(map.Check(key)); }
        public static long Long(this Dictionary<string, object> map, string key) { return ObjectToLong(map.Check(key)); }
        public static float Float(this Dictionary<string, object> map, string key) { return ObjectToFloat(map.Check(key)); }
        public static double Double(this Dictionary<string, object> map, string key) { return ObjectToDouble(map.Check(key)); }
        public static uint Uint(this Dictionary<string, object> map, string key) { return ObjectToUint(map.Check(key)); }

        public static Json Json(this Seq<object> seq, int index) { return seq.Check(index) as Json; }
        public static string String(this Seq<object> seq, int index, string format = null) { return ObjectToString(seq.Check(index), format); }
        public static bool Bool(this Seq<object> seq, int index) { return ObjectToBool(seq.Check(index)); }
        public static int Int(this Seq<object> seq, int index) { return ObjectToInt(seq.Check(index)); }
        public static long Long(this Seq<object> seq, int index) { return ObjectToLong(seq.Check(index)); }
        public static float Float(this Seq<object> seq, int index) { return ObjectToFloat(seq.Check(index)); }
        public static double Double(this Seq<object> seq, int index) { return ObjectToDouble(seq.Check(index)); }
        public static uint Uint(this Seq<object> seq, int index) { return ObjectToUint(seq.Check(index)); }

        public static string ObjectToString(object value, string format = null) {
            if(format.Exist()) { return string.Format(format, value); } else {
                if(value is string) return value as string;
                return value.ToString();
            }
        }

        public static bool ObjectToBool(object value) {
            if(value is bool) return (bool)value;
            else throw new JsonException("[JsonUtility (Parse)Bool] type of mapitem is dismatch  ," + value + " (" + value.GetType() + ")");
        }

        public static int ObjectToInt(object value) {
            if(value is int) return (int)value;
            else if(value is long) return (int)((long)value);

            else if(value is float) return (int)((float)value);
            else if(value is double) return (int)((double)value);
            else throw new JsonException("[JsonUtility (Parse)Int] type of mapitem is dismatch  ," + value + " (" + value.GetType() + ")");
        }

        public static uint ObjectToUint(object value)
        {
            if (value is int) return (uint)((int)value);
            if (value is uint) return (uint)value;
            else if (value is long) return (uint)((long)value);

            else if (value is float) return (uint)((float)value);
            else if (value is double) return (uint)((double)value);
            else throw new JsonException("[JsonUtility (Parse)Int] type of mapitem is dismatch  ," + value + " (" + value.GetType() + ")");
        }

        public static long ObjectToLong(object value) {
            if(value is long) return ((long)value);
            else if(value is int) return ((int)value);

            else if(value is float) return (long)((float)value);
            else if(value is double) return (long)((double)value);
            else throw new JsonException("[JsonUtility (Parse)Long] type of mapitem is dismatch  ," + value + " (" + value.GetType() + ")");
        }

        public static float ObjectToFloat(object value) {
            if(value is float) return ((float)value);
            else if(value is double) return (float)((double)value);

            else if(value is int) return ((int)value);
            else if(value is long) return ((long)value);
            else throw new JsonException("[JsonUtility (Parse)Float] type of mapitem is dismatch  ," + value + " (" + value.GetType() + ")");
        }

        public static double ObjectToDouble(object value) {
            if(value is double) return ((double)value);
            else if(value is float) return ((float)value);

            else if(value is int) return ((int)value);
            else if(value is long) return ((long)value);
            else throw new JsonException("[JsonUtility (Parse)Double] type of mapitem is dismatch  ," + value + " (" + value.GetType() + ")");
        }



    }

    public class JsonException : Exception { public JsonException(string message) : base(message) { } }
}
