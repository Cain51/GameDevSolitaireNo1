using System.Collections;
using System;
using RG.Basic.Helper;

namespace RG.Basic {
    public class Csv : Grid<string> {
        #region Interface

        public Csv(int _rowCount, int _colCount, bool _isStrict = false) : base(_rowCount, _colCount, _isStrict) { }
        public Csv(Seq<Seq<string>> _data, int _colCount, bool _isStrict = false) : base(_data, _colCount, _isStrict) { }
        public Csv(string code) : base(0, 0, false) { Deserialize(code); }

        public override string defaultCell {
            get {
                return "";
            }
        }

        public override string ToString() { return "[" + Serialize() + "]"; }

        public string Serialize() {
            StrGen.Builder builder = StrGen.GetBuilder();

            int lastRow = 0;
            DoCell((row, col, cellValue) => {
                if(row != lastRow) { builder.Append("\n"); lastRow = row; }
                if(col != 0) builder.Append(",");
                AppendString(builder, cellValue);
            }); 
            return builder.GetStringAndRelease();
        }

        static void AppendString(StrGen.Builder sb, string cellValue) {
            if(null == cellValue) cellValue = "";
            else {
                cellValue = cellValue.Replace("\n", "\\n");
                if(cellValue.IndexOfAny(",\"".ToCharArray()) >= 0) {
                    cellValue = cellValue.Replace("\"", "\"\"");
                    sb.AppendFormat("\"{0}\"", cellValue);
                    return;
                }
            }
            sb.Append(cellValue);
        }
        #endregion

        #region Parser


        public Csv Deserialize(string code) {
            data = new Seq<Seq<string>>();

            int iPos = 0;
            while(iPos < code.Length) {
                Seq<string> list = Parseline(code, ref iPos);
                if(list == null) break;
                Data.Add(list);
                if(list.Count > colCount) colCount = list.Count;
            }
            return this;
        }

        static Seq<string> Parseline(string text, ref int iPos) {
            Seq<string> list = new Seq<string>();

            //Line = "puig,\"placeres,\"\"cab\nr\nera\"\"algo\"\npuig";//\"Frank\npuig\nplaceres\",aaa,frank\nplaceres"; 
            int _textLength = text.Length;
            int _startPos = iPos;

            bool _isInQuote = false;

            while(iPos < _textLength) {
                char c = text[iPos];
                if(_isInQuote) {
                    if(c == '\"') //--[ Look for Quote End ]------------
                    {
                        if(iPos + 1 >= _textLength || text[iPos + 1] != '\"')  //-- Single Quote:  Quotation Ends
                        {
                            _isInQuote = false;
                        } else iPos++;  // Skip Double Quotes
                    }
                } else  //-----[ Separators ]----------------------

                    if(c == '\n' || c == ',') { AddCSVtoken(ref list, ref text, iPos, ref _startPos);
                    if(c == '\n')  // Stop the row on line breaks
                    {
                        iPos++;
                        break;
                    }
                } else //--------[ Start Quote ]--------------------
                {
                    if(c == '\"') _isInQuote = true; 
                }

                iPos++;
            }
            if(iPos > _startPos)
                AddCSVtoken(ref list, ref text, iPos, ref _startPos);

            return list;
        }

        static void AddCSVtoken(ref Seq<string> list, ref string Line, int iEnd, ref int iStart) {
            string Text = Line.Substring(iStart, iEnd - iStart);
            iStart = iEnd + 1;

            if(Text.Length > 1 && Text[0] == '\"' && Text[Text.Length - 1] == '\"')
                Text = Text.Substring(1, Text.Length - 2);

            Text = Text.BatchReplace(new string[] { "\"\"", "\\n" }, new string[] { "\"", "\n" });


            list.Add(Text);
        }

        #endregion
    }

    public class CsvException : Exception { public CsvException(string message) : base(message) { } }
}
