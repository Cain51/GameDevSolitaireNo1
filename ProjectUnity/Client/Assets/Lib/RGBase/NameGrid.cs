using RG.Basic.Helper;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RG.Basic {

    public class NameGrid<T> {

        public event _D_InnerT<int, string, T> OnSet;//rol, col, value
        public event _D_InnerT<int> OnDelRow;
        public event _D_Void OnRemoveAll;
        public void CallOnSet(int row_ind, int col_ind, T value) { if (null != OnSet) OnSet(row_ind, key2row.GetKey(col_ind), value); }
        public void CallOnDelRow(int row_ind) { if (null != OnDelRow) OnDelRow(row_ind); }
        public void CallOnRemoveAll() { if (null != OnRemoveAll) OnRemoveAll(); }

        public BiMap<string, int> key2row = new BiMap<string, int>();
        internal Grid<T> grid = new Grid<T>(0, 0);

        public int rowCount { get { return grid.rowCount; } }
        public int colCount { get { return grid.colCount; } }

        public NameGrid(params string[] _keys) {
            for (int i = 0; i < _keys.Length; i++) {
                key2row[_keys[i]] = i;
            }
            grid.OnSet += CallOnSet;
            grid.OnRemoveRow += CallOnDelRow;
            grid.OnRemoveAll += CallOnRemoveAll;
        }

        public virtual T GetCell(int row_ind, string col_name) { return grid.GetCell(row_ind, key2row[col_name]); }

        public virtual void SetCell(int row_ind, string col_name, T value) { grid.SetCell(row_ind, key2row[col_name], value); }

        public virtual void RemoveAll() { grid.RemoveAll(); }

        public virtual void RemoveRow(int row_ind) { grid.RemoveRow(row_ind); }

        public virtual void ClearRow(int row_ind) { grid.ClearRow(row_ind); }

        public virtual void SetRow(int row_ind, T[] values) { grid.SetRow(row_ind, values); } 

        public virtual void AppendRow(T[] values) { grid.AppendRow(values); }

        public virtual void DoRow(int row_ind, Action<int, string, T> Executor) { if(Executor != null) grid.DoRow(row_ind, (r, c, v) => Executor(r, key2row.GetKey(c), v)); }

        public virtual void DoCol(string col_name, Action<int, string, T> Executor) { grid.DoCol(key2row[col_name], (r, c, v) => Executor(r, key2row.GetKey(c), v)); }

        public virtual void DoCell(Action<int, string, T> Executor) { grid.DoCell((r, c, v) => Executor(r, key2row.GetKey(c), v)); } 

        public virtual int FindInCol(string col_name, T value) { return grid.FindInCol(key2row[col_name], value); }

        public virtual StrGen.Builder BuildString(StrGen.Builder _) {
            for (int c = 0; c < colCount; c++) {
                _ = _[key2row.GetKey(c)][c == colCount - 1 ? '\n' : ','];
            }
            return grid.BuildString(_);
        }

        public Seq<T> GetRow(int row_ind) {
            return grid.GetRow(row_ind);
        }

        public override string ToString() { 
            return BuildString(StrGen.New).End;
        }
    }
}