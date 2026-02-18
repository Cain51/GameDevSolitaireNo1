using RG.Basic.Helper;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RG.Basic {

    public class Grid<T>{

        public event _D_InnerT<int, int, T> OnSet;//rol, col, value
        public event _D_InnerT<int> OnRemoveRow;
        public event _D_Void OnRemoveAll;

        public Seq<Seq<T>> Data { get { return data; } }
        internal Seq<Seq<T>> data = new Seq<Seq<T>>();
        internal bool isStrict = false;
        public int rowCount { get { return Data.Count; } }
        public int colCount { get; set; }

        public virtual T defaultCell { get { return default(T); } }

        public Grid(int _rowCount, int _colCount, bool _isStrict = false) { Initial(_rowCount, _colCount, _isStrict); }
        public Grid(Seq<Seq<T>> _data, int _colCount, bool _isStrict = false) {
            colCount = _colCount;
            isStrict = _isStrict;
            data = _data;
        }
        protected void Initial(int _rowCount, int _colCount, bool _isStrict) {
            if(_rowCount < 0)
                if(_isStrict) throw new CsvException("row count " + _rowCount + " cannot less than 0");
                else return;
            data = new Seq<Seq<T>>(new Seq<T>(default(T), _colCount), _rowCount); 
            colCount = _colCount;
            isStrict = _isStrict;
        }



        public T this[int row, int col] {
            get { return GetCell(row, col); }
            set { SetCell(row, col, value); }
        }

        public virtual T GetCell(int row, int col) {
            if(row >= Data.Count)
                if(isStrict) throw new CsvException("[GetCell] row number " + row + " is out of range . ");
                else return defaultCell;
            if(col >= Data[row].Count)
                if(isStrict) throw new CsvException("[GetCell] col number " + col + "of row " + row + " is out of range . ");
                else return defaultCell;
            return Data[row][col];
        }

        public virtual void RemoveAll() { //Without Callback
            Data.Clear();
            colCount = 0;
            if (OnRemoveAll != null) OnRemoveAll();
        }

        public virtual void RemoveRow(int row) {
            Data.RemoveAt(row);
            if (OnRemoveRow != null) OnRemoveRow(row);
            
        }

        public virtual void ClearRow(int row) {
            if (Data[row] == null) Data[row] = new Seq<T>();
            else Data[row].Clear();
            if (OnRemoveRow != null) OnRemoveRow(row);
        }

        public virtual void SetRow(int row, T[] values) {
            if (row >= Data.Count)
                if (isStrict) throw new CsvException("[SetRow] row number " + row + " is out of range . ");
                else for (int i = Data.Count; i <= row; i++) Data.Add(new Seq<T>());  
           
            if (values.Length >= Data[row].Count) {
                if (isStrict) throw new CsvException("[SetRow] the values seq " + values.Length + " is too large . ");
                else colCount = values.Length;
            }

            Seq<T> rowValue = new Seq<T>(values);
            Data[row] = rowValue; 
            if (OnSet != null)  for(int col = 0; col < rowValue.Count; col++) OnSet(row, col, rowValue[col]);  
        }

        public virtual void AppendRow(T[] values) {
            if (isStrict) throw new CsvException("[AppendRow] row cannot be append of strict grid .");
            int currentCount = rowCount;
            SetRow(currentCount, values); 
        }

        public virtual void SetCell(int row, int col, T value) {
            if(row >= Data.Count)
                if(isStrict) throw new CsvException("[SetCell] row number " + row + " is out of range . ");
                else for(int i = Data.Count; i <= row; i++) Data.Add(new Seq<T>());

            if (Data[row] == null)
                Data[row] = new Seq<T>();

            if (col >= Data[row].Count) {
                if(isStrict) throw new CsvException("[SetCell] col number " + col + "of row " + row + " is out of range . ");
                else Data[row].AddRange(new T[col + 1 - Data[row].Count]); 
                if(col + 1 > colCount) colCount = col + 1;
            }
             
            Data[row][col] = value;

            if (OnSet != null) OnSet(row, col, value); 
        }

        public virtual void DoRow(int row, System.Action<int, int, T> Executor) {
            if(Executor == null) throw new CsvException("[DoRow] executor can not be null . ");
            if(row >= rowCount) throw new CsvException("[DoRow] row number " + row + " is out of range(" + rowCount + ") . ");
            for(int col = 0; col < colCount; col++) Executor(row, col, GetCell(row, col));
        }

        public virtual void DoCol(int col, System.Action<int, int, T> Executor) {
            if(Executor == null) throw new CsvException("[DoCol] executor can not be null . ");
            if(col >= colCount) throw new CsvException("[DoCol] col number " + col + " is out of range(" + colCount + ") . ");
            for(int row = 0; row < rowCount; row++) Executor(row, col, GetCell(row, col));
        }

        public virtual void DoCell(System.Action<int, int, T> Executor) {
            if(Executor == null) throw new CsvException("[DoCol] executor can not be null . ");
            for(int row = 0; row < rowCount; row++)
                for(int col = 0; col < colCount; col++)
                    Executor(row, col, GetCell(row, col));
        }

        public Seq<T> CopyRow(int row) {
            if(row > rowCount) throw new CsvException("[GetRow] row number " + row + " is out of range(" + rowCount + ") . ");
            Seq<T> rowSeq = new Seq<T>(Seq<T>.Default, colCount);
            for(int col = 0; col < colCount; col++) rowSeq[col] = GetCell(row, col);
            return rowSeq;
        }

        public Seq<T> CopyCol(int col) {
            if(col > colCount) throw new CsvException("[GetCol] col number " + col + " is out of range(" + colCount + ") . ");
            Seq<T> colSeq = new Seq<T>(Seq<T>.Default, rowCount);
            for(int row = 0; row < rowCount; row++) {
                colSeq[row] = GetCell(row, col);
            }
            return colSeq;
        }

        public virtual Seq<T> GetRow(System.Predicate<Seq<T>> Condition) {
            if(Condition == null) throw new CsvException("[GetRow] Condition can not be null . ");
            Seq<T> row = Data.FirstMatch(Condition);
            return row;
        }

        public virtual Seq<T> GetRow(int row) {
            if(row >= rowCount) throw new CsvException("[GetRow] row index out of range . ");
            return Data[row];
        }

        public Grid<T> GetViewPort(params int[] rowIndexes) {
            Seq<Seq<T>> result = new Seq<Seq<T>>();
            for(int i = 0; i < rowIndexes.Length; i++) {
                if(rowIndexes[i] > rowCount) throw new CsvException("[GetViewPort] row number " + rowIndexes[i] + " is out of range(" + rowCount + ") . ");
                result.Add(Data[rowIndexes[i]]);
            }
            return new Grid<T>(result, colCount, isStrict);
        }

        public int FindInCol(int col, T value) { 
            for (int _row = 0; _row < rowCount; _row++) {
                if ((GetCell(_row, col) as IComparable).CompareTo(value as IComparable) == 0)
                    return _row;
            }
            return -1;
        }

       
        public virtual StrGen.Builder BuildString(StrGen.Builder _) {
            for (int r = 0; r < rowCount; r++) {
                for (int c = 0; c < colCount; c++) {
                    _ = _[GetCell(r, c)][c == colCount - 1 ? '\n' : ','];
                }
            }
            return _;
        }

        public override string ToString() {  
            return BuildString(StrGen.New).End;
        }
    } 
}