using System.Collections;
using System.Collections.Generic;
using System;

namespace RG.Basic {

    public class CsvTable : Csv { 

        public CsvTable(Seq<Seq<string>> _data, int _colCount) : base(_data, _colCount) { GenerateTitleMap(); } 
        public CsvTable(string code) : base (code) { GenerateTitleMap(); } 
        public CsvTable(Seq<string> title) : base(new Seq<Seq<string>>(title), title.Count) { GenerateTitleMap(); }

        public Seq<string> Title { get { return GetRow(0); } } 
        protected Map<string, int> titleMap = new Map<string, int>();
        protected void GenerateTitleMap() {
            titleMap.Clear();
            DoRow(0, (row, col, s) => titleMap[s] = col);
        }

        public override void SetCell(int row, int col, string value) {
            bool titleChanged = row == 0;
            base.SetCell(row, col, value);
            if(titleChanged) {
                GenerateTitleMap();
                GenerateIndexTable(currentIndexKeys);
            }
        }

        

        public class Index : IComparable{
            public IComparable[] keys;
            public int aimRow;
             
            public int CompareTo(object obj) {
                Index index = obj as Index;
                return CompareTo(index);
            }

            public int CompareTo(Index index) {
                if(keys.Length != index.keys.Length) return -(keys.Length - index.keys.Length);
                for(int i = 0; i < keys.Length; i++) {
                    IComparable indexkeysi = keys[i];
                    IComparable keysi = index.keys[i];
                    int keyCompare = keysi.CompareTo(indexkeysi);
                    if(keyCompare != 0) return keyCompare; 
                }
                return 0;
            }

            public static int Compare(Index index1, Index index2) {
                return index1.CompareTo(index2);
            } 
        }
	    
	    public Seq<string> IndexKeys = new Seq<string>(); // 用于记录生成的Index的key, 因为后续不再用到, 提供查询手段
        public Seq<Index> IndexTable = new Seq<Index>();  

        public bool IndexGenerated { get { return currentIndexKeys != null && currentIndexKeys.Length > 0; } }
        protected string[] currentIndexKeys = null;
        public void GenerateIndexTable<T1>(string key1) where T1 : IComparable {
            StringIndexHashSet.Clear();
	        IndexTable = new Seq<Index>(Seq<Index>.Default, rowCount - 1);
            int indexKey1 = titleMap[key1];
            string keyStr1 = "";
            for (int row = 1; row < rowCount; row++)
            {
                keyStr1 = GetCell(row, indexKey1);
                IndexTable[row - 1] = new Index() { aimRow = row, keys = new IComparable[1] {
                    CheckType<T1>(keyStr1)
                }};
                if (typeof(T1) == typeof(string) && !StringIndexHashSet.Contains(keyStr1)) StringIndexHashSet.Add(keyStr1);
            }
	        IndexTable.Sort();
	        IndexKeys = new Seq<string>() { key1 };
        }
         
        public Func<T1, T2, string, string> GenerateIndexTable<T1, T2>(string key1, string key2) where T1 : IComparable where T2 : IComparable {
            StringIndexHashSet.Clear();
	        IndexTable = new Seq<Index>(Seq<Index>.Default, rowCount - 1);
	        int indexKey1 = titleMap[key1];
	        int indexKey2 = titleMap[key2]; 

            for(int row = 1; row < rowCount; row++) { 
                IndexTable[row - 1] = new Index() {
                    aimRow = row,
                    keys = new IComparable[2] {
                        CheckType<T1>(GetCell(row, indexKey1)),
	                    CheckType<T2>(GetCell(row, indexKey2))
                    }
                }; 
            }
	        IndexTable.Sort(); 
	        IndexKeys = new Seq<string>() { key1, key2 };
            return Query;
        }

        public Func<T1, T2, T3, string, string> GenerateIndexTable<T1, T2, T3>(string key1, string key2, string key3) where T1 : IComparable where T2 : IComparable where T3 : IComparable {
            StringIndexHashSet.Clear();
	        IndexTable = new Seq<Index>(Seq<Index>.Default, rowCount - 1);
	        int indexKey1 = titleMap[key1];
	        int indexKey2 = titleMap[key2];
	        int indexKey3 = titleMap[key3]; 
             
            for (int row = 1; row < rowCount; row++)
            {  
                IndexTable[row - 1] = new Index() {
                    aimRow = row, 
                    keys = new IComparable[3] {
                        CheckType<T1>(GetCell(row, indexKey1)),
	                    CheckType<T2>(GetCell(row, indexKey2)),
	                    CheckType<T3>(GetCell(row, indexKey3))
                    }
                }; 
            }
	        IndexTable.Sort();
	        IndexKeys = new Seq<string>() { key1, key2, key3 };
            return Query;
        }

        public HashSet<string> StringIndexHashSet = new HashSet<string>(); //所有string的key会被记录在这个HashSet中(不分列), 查询时对于不存在的项直接使用HashSet加速剪掉
        public virtual CsvTable GenerateIndexTable(params string[] keys) {
            StringIndexHashSet.Clear();
            currentIndexKeys = keys;
            int[] keyindexes = new int[keys.Length];
            for(int i = 0; i < keys.Length; i++) {
                keyindexes[i] = titleMap[keys[i]];
            }
            IndexTable = new Seq<Index>(Seq<Index>.Default, rowCount - 1);
            for(int row = 1; row < rowCount; row++) {
                string[] indexKeys = new string[keyindexes.Length];
                for(int i = 0; i < keyindexes.Length; i++) {
                    indexKeys[i] = GetCell(row, keyindexes[i]);
                    if (!StringIndexHashSet.Contains(indexKeys[i])) 
                        StringIndexHashSet.Add(indexKeys[i]);
                }
                IndexTable[row - 1] = new Index() { aimRow = row, keys = indexKeys };
            }
	        IndexKeys = new Seq<string>(keys);
            IndexTable.Sort();
            return this;
        }
         
        public IComparable CheckType<T> (string str) where T : IComparable {
            Type targetType = typeof(T);
            if(targetType == typeof(int)) return int.Parse(str);
            if(targetType == typeof(float)) return float.Parse(str);
            if(targetType == typeof(bool)) return bool.Parse(str);
            if(targetType == typeof(byte)) return byte.Parse(str);
            if(targetType == typeof(long)) return long.Parse(str);
            if (targetType == typeof(string))
            {
                if (!StringIndexHashSet.Contains(str)) StringIndexHashSet.Add(str);
                return str;
            }
            else throw new CsvException("[ConvertToType] unsupported type " + targetType);
        }

        protected Index lastQuery = new Index() { keys = new IComparable[0] };

        public string Query<T1>(T1 key1, string key) where T1 : IComparable {
            if(!titleMap.ContainsKey(key)) throw new CsvException("[Query] the title " + key + " you required is not eixst");
            if (typeof(T1) == typeof(string) && StringIndexHashSet.Count > 0 && !StringIndexHashSet.Contains(key1 as string)) { return defaultCell; }
            Index search = new Index() { keys = new IComparable[] { key1 } };
            return Search(search, key);
        }

        public string Query<T1, T2>(T1 key1, T2 key2, string key) where T1 : IComparable where T2 : IComparable {
            if(!titleMap.ContainsKey(key)) throw new CsvException("[Query] the title " + key + " you required is not eixst");
            if (typeof(T1) == typeof(string) && StringIndexHashSet.Count > 0 && !StringIndexHashSet.Contains(key1 as string)) { return defaultCell; }
            if (typeof(T2) == typeof(string) && StringIndexHashSet.Count > 0 && !StringIndexHashSet.Contains(key2 as string)) { return defaultCell; }
            Index search = new Index() { keys = new IComparable[] { key1, key2 } }; 
            return Search(search, key);
        }

        public string Query<T1, T2, T3>(T1 key1, T2 key2, T3 key3, string key) where T1 : IComparable where T2 : IComparable where T3 : IComparable {
            if(!titleMap.ContainsKey(key)) throw new CsvException("[Query] the title " + key + " you required is not eixst");
            if (typeof(T1) == typeof(string) && StringIndexHashSet.Count > 0 && !StringIndexHashSet.Contains(key1 as string)) { return defaultCell; }
            if (typeof(T2) == typeof(string) && StringIndexHashSet.Count > 0 && !StringIndexHashSet.Contains(key2 as string)) { return defaultCell; }
            if (typeof(T3) == typeof(string) && StringIndexHashSet.Count > 0 && !StringIndexHashSet.Contains(key3 as string)) { return defaultCell; }
            Index search = new Index() { keys = new IComparable[] { key1, key2, key3 } };
            return Search(search, key);
        }

        public string Search(Index searchIndex, string key) {
            int min = 0, max = IndexTable.Count - 1;
            if(lastQuery == searchIndex) return GetCell(lastQuery.aimRow, titleMap[key]);
            while(min <= max) {
                int center = (min + max) / 2;
                int result = IndexTable[center].CompareTo(searchIndex);
                if(result == 0) {
                    lastQuery = IndexTable[center];
                    return GetCell(IndexTable[center].aimRow, titleMap[key]);
                }
                if(result < 0) min = center + 1;
                if(result > 0) max = center - 1;
            }
            return defaultCell;
        }
   
         
    }
}