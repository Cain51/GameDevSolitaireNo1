using System;
using System.Collections.Generic;
using System.Text;

namespace RG.Basic {
    public class XMLBase {
        protected static bool IsSpace(char c) { return c == ' ' || c == '\t' || c == '\n' || c == '\r'; }

        protected static void SkipSpaces(string str, ref int i) {
            while(i < str.Length) {
                if(!IsSpace(str[i])) {
                    if(str[i] == '<' && i + 4 < str.Length && str[i + 1] == '!' && str[i + 2] == '-' && str[i + 3] == '-') {
                        i += 4; // skip <!-- 
                        while(i + 2 < str.Length && !(str[i] == '-' && str[i + 1] == '-')) i++;
                        i += 2; // skip --
                    } else break;
                }
                i++;
            }
        }

        protected static string GetValue(string str, ref int i, char endChar, char endChar2, bool stopOnSpace) {
            int start = i;
            while((!stopOnSpace || !IsSpace(str[i])) && str[i] != endChar && str[i] != endChar2) i++;
            return str.Substring(start, i - start);
        }

        protected static bool IsQuote(char c) {
            return c == '"' || c == '\'';
        }

        // returns name
        protected static string ParseAttributes(string str, ref int i, Map<string, string> attributes, char endChar, char endChar2) {
            SkipSpaces(str, ref i);
            string name = GetValue(str, ref i, endChar, endChar2, true);

            SkipSpaces(str, ref i);

            while(str[i] != endChar && str[i] != endChar2) {
                string attrName = GetValue(str, ref i, '=', '\0', true);

                SkipSpaces(str, ref i);
                i++; // skip '='
                SkipSpaces(str, ref i);

                char quote = str[i];
                if(!IsQuote(quote))
                    throw new XMLParsingException("Unexpected token after " + attrName);

                i++; // skip quote
                string attrValue = GetValue(str, ref i, quote, '\0', false);
                i++; // skip quote

                attributes.Add(attrName, attrValue);
                SkipSpaces(str, ref i);
            }

            return name;
        }
    }

    /// <summary>
    /// Class representing whole DOM XML document
    /// </summary>
    public class XML : XMLBase {
        private Node root;
        private Map<string, string> declarations = new Map<string, string>();

        /// <summary>
        /// Public constructor. Loads xml document from raw string
        /// </summary>
        /// <param name="xmlString">String with xml</param>
        public XML(string xmlString) {
            int i = 0; 
            while(true) {
                SkipSpaces(xmlString, ref i);
                if(xmlString[i] != '<') throw new XMLParsingException("Unexpected token");

                i++; // skip < 
                if(xmlString[i] == '?') {// declaration 
                    i++; // skip ?
                    ParseAttributes(xmlString, ref i, declarations, '?', '>');
                    i++; // skip ending ?
                    i++; // skip ending > 
                    continue;
                }

                if(xmlString[i] == '!') {   // doctype 
                    while(xmlString[i] != '>') i++;// skip doctype 
                    i++; // skip > 
                    continue;
                }

                root = new Node(xmlString, ref i);
                break;
            }
        }
        /// <summary>
        /// Root document element
        /// </summary>
        public Node Root { get { return root; } }

        /// <summary>
        /// List of XML Declarations as <see cref="XMLAttribute"/>
        /// </summary>
        public Map<string, string> Declarations {
            get { return declarations; }
        }
         
        /// <summary>
        /// Element node of document
        /// </summary>
        public class Node : XMLBase {
            private string value;
            private string name;

            private Seq<Node> subNodes = new Seq<Node>();
            private Map<string, string> attributes = new Map<string, string>();

            internal Node(string str, ref int i) {

                name = ParseAttributes(str, ref i, attributes, '>', '/');

                if(str[i] == '/') { // if this node has nothing inside
                    i += 2; // skip />  
                    return;
                }

                i++; // skip >

                // temporary. to include all whitespaces into value, if any
                int tempI = i;

                SkipSpaces(str, ref tempI);
  
                if (str[tempI] == '<') {
                    i = tempI; 

                    while(str[i + 1] != '/') {// parse subnodes 
                        i++; // skip <
                        subNodes.Add(new Node(str, ref i)); 
                        SkipSpaces(str, ref i); 
                        if(i >= str.Length) return; // EOF 
                        if (str[i] != '<') throw new XMLParsingException("Unexpected token");
                    } 
                    i++; // skip <
                } else { // parse value 
                    value = GetValue(str, ref i, '<', '\0', false);
                    i++; // skip < 
                    if(str[i] != '/') throw new XMLParsingException("Invalid ending on tag " + name);
                }

                i++; // skip /
                SkipSpaces(str, ref i);

                string endName = GetValue(str, ref i, '>', '\0', true);
                if(endName != name) throw new XMLParsingException("Start/end tag name mismatch: " + name + " and " + endName);
                SkipSpaces(str, ref i);
                if(str[i] != '>') throw new XMLParsingException("Invalid ending on tag " + name);
                i++; // skip >
            }
            
            /// <summary>
            /// Element value
            /// </summary>
            public string Value { get { return value; } set { this.value = value; } }
            
            /// <summary>
            /// Element name
            /// </summary>
            public string Name { get { return name; } set { this.name = value; } }

            /// <summary>
            /// List of subelements
            /// </summary>
            public Seq<Node> SubNodes { get { return subNodes; } }

            /// <summary>
            /// List of attributes
            /// </summary>
            public Map<string, string> Attributes { get { return attributes; } }
              
            public Node GetChild(string nodeName) { return GetChild(n => n.name == nodeName); }

            public Node GetChild(Predicate<Node> pred) { return pred != null ? SubNodes.FirstMatch(pred) : null; }

            public Seq<Node> GetChildren(string nodeName) { return GetChildren(n => n.name == nodeName); }

            public Seq<Node> GetChildren(Predicate<Node> pred) { return pred != null ? SubNodes.Filter(pred) : new Seq<Node>(); }

            public string this[string attrName] { get { return GetAttr(attrName); } }

            public bool HasAttr(string attributeName) { return attributes.ContainsKey(attributeName); }

            public string GetAttr(string attributeName) { return attributes.GetIfExist(attributeName, ""); }

            public T GetAttrEnum<T>(string attributeName, T defaultValue) {
                string value = GetAttr(attributeName);
                if (value.Exist()) return value.TryConvertToEnum(defaultValue);
                return defaultValue;
            }

            public int GetAttrInt(string attributeName, int defaultValue = 0) {
                string value = GetAttr(attributeName);
                if(value.Exist()) int.TryParse(value, out defaultValue);
                return defaultValue;
            }

            public bool GetAttrBool(string attributeName, bool defaultValue = false) {
                string value = GetAttr(attributeName);
                if (value.Exist()) bool.TryParse(value, out defaultValue);
                return defaultValue;
            }

            public float GetAttrFloat(string attributeName, float defaultValue = 0) {
                string value = GetAttr(attributeName);
                if(value.Exist()) float.TryParse(value, out defaultValue);
                return defaultValue;
            }

            public Seq<string> GetAttrSeq(string attributeName, params char[] splitor) {
                string value = GetAttr(attributeName);
                return value.Exist() ? value.Split(splitor) : new Seq<string>();
            }

            public Seq<int> GetAttrIntVector(string attributeName, int defaultValue = 0) {
                return GetAttrSeq(attributeName, ',', '|', '_').Map<int>(s => {
                    int i = defaultValue;
                    if(s.Exist()) int.TryParse(s, out i);
                    return i;
                });
            }

            public Seq<float> GetAttrVector(string attributeName, float defaultValue = 0) {
                return GetAttrSeq(attributeName, ',', '|', '_').Map<float>(s => {
                    float f = defaultValue;
                    if(s.Exist()) float.TryParse(s, out f);
                    return f;
                });
            }

            public override string ToString() { return Serialize(-1); }

            public string Serialize(int lineOffset = 0) {
                bool usingOffset = lineOffset >= 0;

                StringBuilder stringbuilder = new StringBuilder();
                stringbuilder.Remove(0, stringbuilder.Length);
                if(usingOffset) stringbuilder.Append(' ', lineOffset * 2);
                stringbuilder.Append('<').Append(Name);

                if(attributes.Count > 0) stringbuilder.Append(' ');
                attributes.ForEachPairs((_key, _value) => stringbuilder.Append(_key + "=\"" + _value + "\" "));
                if(attributes.Count > 0) stringbuilder.Remove(stringbuilder.Length - 1, 1);

                if(subNodes.Count <= 0) {
                    if(!string.IsNullOrEmpty(Value))
                        stringbuilder.Append('>').Append(Value).Append("</").Append(Name).Append(">");
                    else
                        stringbuilder.Append("/>");
                    return stringbuilder.ToString();
                }

                stringbuilder.Append('>');
                if(usingOffset) stringbuilder.Append('\n');

                foreach(var node in SubNodes) stringbuilder.Append(node.Serialize((usingOffset) ? lineOffset : (lineOffset + 1)) + ((lineOffset >= 0) ? "\n" : ""));

                stringbuilder.Append(value).Append("</").Append(Name).Append(">");
                return stringbuilder.ToString();
            }

            public void Traversal(Action<Node, Node> nodeFunc) {
                if(nodeFunc != null) {
                    for(int i = 0; i < SubNodes.Count; i++) {
                        nodeFunc(this, SubNodes[i]);
                        SubNodes[i].Traversal(nodeFunc);
                    }
                }
            }
        }
    }

    

    public class XMLParsingException : Exception {
        public XMLParsingException(string message)
            : base(message) {
        }
    }
}
