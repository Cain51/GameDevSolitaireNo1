using RG.Basic.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RG.Basic {

    public class INI {

        public readonly static char[] LABEL_COMMENT = { ';', '#' };
        public readonly static char[] LABEL_ASSIGN = { '=' };
        public readonly static char[] LABEL_FILTER = { '=', ';', '\n', '\r', '[', ']' };

        public enum _Type { Prop, Section }

        public string Key { get; private set; }
        public string Value { get; set; }
        public string Annotate { get; set; }
        public _Type Type { get; set; }

        public Dictionary<string, INI> SubNodes = new Dictionary<string, INI>();

        public INI Get(params string[] keys) {
            return GetSub(0, keys);
        }

        public string Visit(string route) {
            string[] keys = route.Split('.');
            return Get(keys).Value;
        }

        public T Visit<T>(string route) {
            string[] keys = route.Split('.');
            return Get<T>(keys);
        }

        public T Get<T>(params string[] keys) {
            INI item = Get(keys);
            if (item == null) return default(T);
            T _v = (T)Convert.ChangeType(item.Value, typeof(T));
            return _v;
        }

        public string[] GetKeys() {
            return SubNodes.Keys.ToArray();
        }

        private INI GetSub(int ind_start, params string[] keys) {
            string _name = keys[ind_start++].Trim();
            INI _item = null;
            if (SubNodes.ContainsKey(_name)) _item = SubNodes[_name];
            if (_item == null) return null;
            if (ind_start < keys.Length) return _item.GetSub(ind_start, keys);
            else return _item;
        }

        private INI GetOrCreateSub(int ind_start, params string[] keys) {
            string _name = keys[ind_start++].Trim();
            INI _item = null;
            if (SubNodes.ContainsKey(_name)) _item = SubNodes[_name];
            else {
                _item = new INI { Key = _name };
                SubNodes[_name] = _item;
            }
            if (ind_start < keys.Length) return _item.GetOrCreateSub(ind_start, keys);
            else return _item;
        }


        public void FromText(string code) {
            string[] _lines = code.Split('\n');

            INI section_current = this;
            StrGen.Builder sb_comment = StrGen.New; 

            for(int i = 0; i < _lines.Length; i++) {
                Readline(_lines[i], ref sb_comment, ref section_current);
            } 
            Annotate = sb_comment.End;
        }

        public void FromText(TextReader reader) {
            INI section_current = this;
            StrGen.Builder sb_comment = StrGen.New; 

            string _line; 
            while ((_line = reader.ReadLine()) != null) {
                Readline(_line, ref sb_comment, ref section_current);
            }
            Annotate = sb_comment.End;
        }


        private void Readline(string _line, ref StrGen.Builder _sb_comment, ref INI _section_current) {
            _line = _line.Trim();
            if (!_line.Exist()) return;
            if (_line[0] == ';' || _line[0] == '#') { //Record Comments And Then Record
                _sb_comment.Append(_line).Append('\n');
            } else if (_line.StartsWith("[") && _line.EndsWith("]")) {
                string[] section_keys = _line.Substring(1, _line.Length - 2).Split('.');
                _section_current = GetOrCreateSub(0, section_keys);
                _section_current.Type = _Type.Section;
                if (_section_current.Annotate.Exist()) _sb_comment.Insert(0, _section_current.Annotate);
                _section_current.Annotate = _sb_comment.Clear();
            } else {
                int ind_commen = _line.IndexOfAny(LABEL_COMMENT);
                if (ind_commen != -1) {
                    _sb_comment.Append(_line.Substring(ind_commen, _line.Length - ind_commen));
                    _line = _line.Substring(0, ind_commen);
                }
                string[] pair = _line.Split(LABEL_ASSIGN, StringSplitOptions.RemoveEmptyEntries);

                if (pair.Length != 2) {
                    _sb_comment.Clear();
                    return;
                }

                string _name = pair[0].Trim();
                INI _prop = null;
                if (!_section_current.SubNodes.ContainsKey(_name)) { 
                    string[] name_keys = _name.Split('.'); 
                    _prop = _section_current.GetOrCreateSub(0, name_keys);
                    _prop.Type = _Type.Prop;
                } else {
                    _prop = _section_current.SubNodes[_name];
                }
                if (_prop.Annotate.Exist()) _sb_comment.Insert(0, _prop.Annotate);

                string _value = pair[1].Trim();
                if (_value.StartsWith("\"") && _value.EndsWith("\"")) {
                    _value = _value.Substring(1, _value.Length - 2);
                }

                _prop.Key = _name;
                _prop.Value = _value;
                _prop.Annotate = _sb_comment.Clear();
                _prop.Type = _Type.Prop;
            }
        }

        private StrGen.Builder ToText(StrGen.Builder builder, string path) {
            if (Type == _Type.Section) {
                builder.Append(Annotate).Append("\n[");
                builder.Append(path);
                builder.Append("]\n");
            }

            foreach (var _key in SubNodes.Keys) {
                INI _value = SubNodes[_key];
                if (_value.Type == _Type.Prop) {
                    bool exist = _value.Value.IndexOfAny(LABEL_FILTER) < 0;
                    if (exist)
                        builder.Append(_value.Key).Append(" = ").Append(_value.Value).Append("\n");
                    else
                        builder.Append(_value.Key).Append(" = \"").Append(_value.Value).Append("\"\n");
                } else {
                    ToText(builder, path.Exist() ? StrGen.New[path]['.'][Key].End : Key);
                }
            }
            return builder;
        }

        public string ToText() {
            return ToText(StrGen.New, "").End;
        }

        public override string ToString() {
            return ToText();
        }
    }
}