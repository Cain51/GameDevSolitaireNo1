namespace RG.Basic{

    public delegate void _D_Void(); 
    public delegate void _D_InnerT<T1>(T1 t1);
    public delegate void _D_InnerT<T1, T2>(T1 t1, T2 t2);
    public delegate void _D_InnerT<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate void _D_InnerT<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

    public delegate int _D_OuterInt();
    public delegate int _D_OuterInt<T1>(T1 t1);
    public delegate int _D_OuterInt<T1, T2>(T1 t1, T2 t2);
    public delegate int _D_OuterInt<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate int _D_OuterInt<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

    public delegate bool _D_OuterBool();
    public delegate bool _D_OuterBool<T1>(T1 t1);
    public delegate bool _D_OuterBool<T1, T2>(T1 t1, T2 t2);
    public delegate bool _D_OuterBool<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate bool _D_OuterBool<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

    public delegate float _D_OuterFloat();
    public delegate float _D_OuterFloat<T1>(T1 t1);
    public delegate float _D_OuterFloat<T1, T2>(T1 t1, T2 t2);
    public delegate float _D_OuterFloat<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate float _D_OuterFloat<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

    public delegate string _D_OuterString();
    public delegate string _D_OuterString<T1>(T1 t1);
    public delegate string _D_OuterString<T1, T2>(T1 t1, T2 t2);
    public delegate string _D_OuterString<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate string _D_OuterString<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4); 
}