using RG.Basic;

public static class SplitHelper  {
    public static int[] GetIntArray(this string src, char spl = '|')
    {
        Seq<int> rel = new Seq<int>();
        if (src == "") {
            return rel.ToArray();
        }
        Seq<string> relStr = src.Split(spl);
        relStr.ForEach((obj) => {
            rel.Add(int.Parse(obj));
        });
    
        return rel.ToArray();
    }
}
