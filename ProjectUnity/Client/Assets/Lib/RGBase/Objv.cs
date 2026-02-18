using System.Collections;
using System;
using RG.Basic;
using System.Text;

namespace RG.Basic {
    public class Objv : Grid<object> {

        public string tableName;
        public Seq<string> definations = new Seq<string>();
        public Seq<string> categories = new Seq<string>();
        public Seq<Func<string, object>> categoriesConverter = new Seq<Func<string, object>>();

        #region Interface

        public Objv(int _rowCount, int _colCount, bool _isStrict = false) : base(_rowCount, _colCount, _isStrict) { }
        public Objv(Seq<Seq<object>> _data, int _colCount, bool _isStrict = false) : base(_data, _colCount, _isStrict) { }
        public Objv(string name, string code) : base(0, 0, false)
        {
            tableName = name;
            Deserialize(code);
        }
        public Objv(Seq<string> keys, Seq<string> cates) : base(new Seq<Seq<object>>(), keys.Count) {
            definations = keys;
            categories = cates;
            GenerateConvertor();
        }
        public void GenerateConvertor() {
            categoriesConverter.Clear();

            categories.DoSeq(cate => {
                categoriesConverter.Add(GetConvertor(cate));
            });
        }

        public override void SetCell(int row, int col, object value) {
            base.SetCell(row, col, value);
            if(col == 0 && value is int) idCache[(int)value] = row;
        }
        public int GetIndexOfId(int id)
        {
            return GetIndexOfId(id as IComparable);
        }
        public int GetIndexOfId(IComparable id) {
            int min = 0, max = Data.Count - 1;
            while (min <= max) {
                int center = (min + max) / 2;
                Seq<object> currentCenter = Data[center];
                int result = id.CompareTo(currentCenter.FirstOrDefault);
                if (result == 0) {
                    return center;
                    
                }
                if (result > 0) min = center + 1;
                if (result < 0) max = center - 1;
            }
            return -1;
        }

        public static int CompareNumber(object a, object b) {
            if (a is double) {
                if (b is double) return ((double)a).CompareTo((double)b);
                else if (b is int) return ((double)a).CompareTo(Convert.ToDouble((int)b));
                else if (b is float) return ((double)a).CompareTo(Convert.ToDouble((float)b));
                else return 1;
            } else {
                return (a as IComparable).CompareTo(b as IComparable);
            }
        }

        public Seq<int> FindAllIndexesInCol(string key, object value) {
            int col = GetColIndexOfDefinedKey(key);
            Seq<int> indexes = new Seq<int>();
            for (int _row = 0; _row < rowCount; _row++) {
                if (CompareNumber(value, GetCell(_row, col)) == 0) {
                    indexes.Add(_row);
                }
            } 
            return indexes;
        }

        public int GetMaxID()
        {
            return (int)Data.Last.First;
        }
        public Seq<object> TryGetLine(int id)
        {
            int i = GetIndexOfId(id);
            if (i < 0)
                return null;
            return Data[i];
        }
        public Seq<object> GetLine(object id) { return Data[GetIndexOfId(id as IComparable)]; }
        public int TryGetColIndexOfDefinedKey(string key)
        {
            return definations.IndexOf(key);
        }
        public int GetColIndexOfDefinedKey(string definedKey) {
            int index = definations.IndexOf(definedKey);
            if (index == -1) throw new ObjvException("Cannot Find Definded Key :" + definedKey + " of Table " + tableName);
            return index;
        }
        public object QueryByIndex(int i, string definedKey)
        {
            var result = _QueryByIndex(i, definedKey);

            if (result == null)
            {
                throw new Exception("[Objv QueryByIndex]Query Of Table " + tableName + " not found, i: " + i + " colKey: " + definedKey);
            }
            return result;
        }
        public object TryQueryByIndex(int i, string definedKey)
        {
            return _QueryByIndex(i, definedKey);
        }
        public object TryQueryInt(int id, string definedKey)
        {
            return _Query(id, definedKey);
        }

        StringBuilder sb = new StringBuilder();
        public object QueryInt(int id, string definedKey) {
            var result = _Query(id, definedKey);

            if (result == null) {
                sb.Remove(0, sb.Length);
                sb.Append("[Objv QueryInt]Query Of Table ").Append(tableName).Append(" not found, rowID: ").Append(id).Append(" colKey: ").Append(definedKey);
                throw new Exception(sb.ToString());
            }
            return result;
        }

        Hash<int> idCache = new Hash<int>();
        public object Query(object id, string definedKey)
        {
            var result = _Query(id, definedKey);
            if (result == null) {
                sb.Remove(0, sb.Length);
                sb.Append("[Objv Query]Query Of Table ").Append(tableName).Append(" not found, rowID: ").Append(id).Append(" colKey: ").Append(definedKey);
                throw new Exception(sb.ToString());
            } 
            return result;
        }
        private object _QueryByIndex(int i, string definedKey)
        {
            int col = TryGetColIndexOfDefinedKey(definedKey);
            if (col < 0) return null; 
            if (i > -1 && i < rowCount && col > -1) return this[i, col]; 
            else return null; 
        }

        private object _Query(object id, string definedKey) {
            int col = TryGetColIndexOfDefinedKey(definedKey);
            if (col < 0) return null; 
            int i = -1;
            if (id is int) {
                i = idCache.GetVal((int)id, -1);
                if(i < 0) i = GetIndexOfId(id as IComparable);
            }
            if (i > -1 && col > -1) {
                return this[i, col];
            } else {
                return null;
            }
        }

        public override object defaultCell { get { return null; } }

        public override string ToString() { return "[" + Serialize() + "]"; }

        public string Serialize() {
            sb.Remove(0, sb.Length);

            int lastRow = 0;
            definations.DoSeq((d, col) => {
                if (col != 0) sb.Append(",");
                AppendString(sb, "String", d);

            });
            sb.Append("\n");
            categories.DoSeq((c, col) => {
                if (col != 0) sb.Append(",");
                AppendString(sb, "String", c.ToString());
            });
            sb.Append("\n");

            DoCell((row, col, cellValue) => {
                if (row != lastRow) { sb.Append("\n"); lastRow = row; }
                if (col != 0) sb.Append(",");
                if(cellValue is Seq<object>)
                    AppendString(sb, categories[col], (cellValue as Seq<object>).ConcatToString("#"));
                else
                    AppendString(sb, categories[col], (cellValue??"").ToString());
            });

            return sb.ToString();
            //File.WriteAllText(FileName, sb.ToString(), System.Text.Encoding.UTF8);
        }

        public void AppendString(System.Text.StringBuilder sb, string cate, string cellValue) {
            if (null == cellValue) cellValue = "";
            else {
                cellValue = cellValue.Replace("\n", "\\n");
                if (cate == "String") {
                    if (cellValue.IndexOfAny(",\"".ToCharArray()) >= 0)
                        cellValue = cellValue.Replace("\"", "\"\"");
                    sb.AppendFormat("\"{0}\"", cellValue);
                    return;
                }
            }
            sb.Append(cellValue);
        }
        #endregion

        #region Parser

        public Objv Deserialize(string code) {
            data = new Seq<Seq<object>>();
            int iPos = 0;
            int step = 0;
            while (iPos < code.Length) {
                if (step == 0) definations = Parseline(code, ref iPos).Map(o => o.ToString());
                else if (step == 1) {
                    categories = Parseline(code, ref iPos).Map(o => o.ToString());
                    GenerateConvertor();
                } else {
                    Seq<object> list = Parseline(code, ref iPos);
                    if (list == null) break;
                    Data.Add(list);
                    if (list.Count > colCount) colCount = list.Count;
                }
                step++;
            }
            Data.SortByX(so => (so.FirstOrDefault as IComparable));
            if(categories.Count > 0 && categories[0] == "Int") {
                for(int i = 0; i < Data.Count; i++) {
                    idCache[(int)Data[i].First] = i;
                }
            }
            //idCache.RemoveAll();
            return this;
        }

        protected Seq<object> Parseline(string text, ref int iPos) {
            Seq<object> list = new Seq<object>();

            //Line = "puig,\"placeres,\"\"cab\nr\nera\"\"algo\"\npuig";//\"Frank\npuig\nplaceres\",aaa,frank\nplaceres"; 
            int _textLength = text.Length;
            int _startPos = iPos;

            bool _isInQuote = false;

            while (iPos < _textLength) {
                char c = text[iPos];
                if (_isInQuote) {
                    if (c == '\"') //--[ Look for Quote End ]------------
                    {
                        if (iPos + 1 >= _textLength || text[iPos + 1] != '\"')  //-- Single Quote:  Quotation Ends
                        {
                            _isInQuote = false;
                        } else iPos++;  // Skip Double Quotes
                    }
                } else  //-----[ Separators ]---------------------- 
                    if (c == '\r' || c == '\n' || c == ',') {
                    AddObjvToken(ref list, ref text, iPos, ref _startPos);
                    if (c == '\n')  // Stop the row on line breaks
                    {
                        iPos++;
                        break;
                    }
                } else //--------[ Start Quote ]--------------------
                {
                    if (c == '\"') _isInQuote = true;
                }

                iPos++;
            }
            if (iPos > _startPos) AddObjvToken(ref list, ref text, iPos, ref _startPos);

            return list;
        }

        protected void AddObjvToken(ref Seq<object> list, ref string Line, int iEnd, ref int iStart) {
            string Text = Line.Substring(iStart, iEnd - iStart);
            iStart = iEnd + 1;

            if (Text.Length > 1 && Text[0] == '\"' && Text[Text.Length - 1] == '\"') {
                Text = Text.Substring(1, Text.Length - 2);
                Text = Text.BatchReplace(new string[] { "\"\"", "\\n" }, new string[] { "\"", "\n" });
            }

            if (categories.Count > list.Count) {
                list.Add(categoriesConverter[list.Count](Text));
            } else {
                list.Add(Text);
            }
        }

        public static object GetBasicObjByCategory(string cate, string str) {
            switch (cate) {
                case "String":
                    return str;
                case "Bool":
                    bool parsedBool;
                    bool.TryParse(str, out parsedBool);
                    return parsedBool;
                case "Byte":
                    byte parsedByte;
                    byte.TryParse(str, out parsedByte);
                    return parsedByte;
                case "Int":
                    int parsedInt;
                    int.TryParse(str, out parsedInt);
                    return parsedInt;
                case "Long":
                    long parsedLong;
                    long.TryParse(str, out parsedLong);
                    return parsedLong;
                case "Float":
                    float parsedFloat;
                    float.TryParse(str, out parsedFloat);
                    return parsedFloat;
                case "Double":
                    double parsedDouble;
                    double.TryParse(str, out parsedDouble);
                    return parsedDouble;
                default:
                    return null;
            }
        }

        public static Func<string, object> GetConvertor(string cate) { 
            switch (cate) {
                case "String":
                    return s => s;
                case "Bool":
                    return s => {
                        bool parsedBool;
                        bool.TryParse(s, out parsedBool);
                        return parsedBool;
                    };
                case "Byte":
                    return s => {
                        byte parsedByte;
                        byte.TryParse(s, out parsedByte);
                        return parsedByte;
                    };
                case "Int":
                    return s => {
                        int parsedInt;
                        int.TryParse(s, out parsedInt);
                        return parsedInt;
                    };
                case "Long":
                    return s => {
                        long parsedLong;
                        long.TryParse(s, out parsedLong);
                        return parsedLong;
                    };
                case "Float":
                    return s => {
                        float parsedFloat;
                        float.TryParse(s, out parsedFloat);
                        return parsedFloat;
                    };
                case "Double":
                    return s => {
                        double parsedDouble;
                        double.TryParse(s, out parsedDouble);
                        return parsedDouble;
                    };
                default:
                    string inner = cate;
                    if (cate.StartsWith("[") && cate.EndsWith("]")) {
                        inner = cate.Substring(1, cate.Length - 2);
                    }
                    Seq<string> cateSeq = inner.Split('#');
                    return (s) => {
                        Seq<string> inputs = s.Split('#');
                        Seq<object> objs = new Seq<object>();
                        inputs.DoSeq((input, i) => {
                            objs.Add(GetBasicObjByCategory(cateSeq[i % cateSeq.Count], input));
                        });
                        return objs;
                    };
            }
        }


        #endregion
          
        
    }

    public class ObjvException : Exception { public ObjvException(string message) : base(message) { } }
}
