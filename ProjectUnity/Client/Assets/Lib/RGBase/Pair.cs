using System.Collections;

namespace RG.Basic
{
	[System.Serializable]
	public struct Pair<TKey, TValue>
    {
        public TKey k;
        public TValue v; 

        public Pair(TKey a, TValue b) { this.k = a; this.v = b; }

        public override string ToString()
        {
            return k.ToString() + "," + v.ToString();
        }
    }

    public struct Pair3<TA,TB,TC>
    {
        public TA a;
        public TB b;
        public TC c;

        public Pair3(TA a, TB b, TC c) { this.a = a; this.b = b; this.c = c; }

        public override string ToString()
        {
            return "a:" + a.ToString() + " b:" + b.ToString() + " c:" + c.ToString();
        }
    }

    public struct Pair4<TA, TB, TC, TD>
    {
        public TA a;
        public TB b;
        public TC c;
        public TD d;

        public TA Item1 { get { return a; } }
        public TB Item2 { get { return b; } }
        public TC Item3 { get { return c; } }
        public TD Item4 { get { return d; } }

        public Pair4(TA a, TB b, TC c,TD d) { this.a = a; this.b = b; this.c = c; this.d = d; }

        public override string ToString()
        {
            return "a:" + a.ToString() + " b:" + b.ToString() + " c:" + c.ToString() + " d:" + d.ToString();
        }
    }

    public struct Pair5<TA, TB, TC, TD, TE> {
        public TA a;
        public TB b;
        public TC c;
        public TD d;
        public TE e;

        public Pair5(TA a, TB b, TC c, TD d, TE e) { this.a = a; this.b = b; this.c = c; this.d = d; this.e = e; }

        public override string ToString() {
            return "a:" + a.ToString() + " b:" + b.ToString() + " c:" + c.ToString() + " d:" + d.ToString() + " e:" + e.ToString();
        }
    }
}
