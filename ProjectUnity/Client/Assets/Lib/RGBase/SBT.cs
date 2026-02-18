using System;
using System.Collections.Generic;

namespace RG.Basic
{
    public class SBT<TKey, TValue> where TKey : IComparable
    {
        /*
        internal class SBTNode
        {
            internal TKey key;
            internal TValue val;
            internal SBTNode leftSon;
            internal SBTNode rightSon;
            internal SBTNode father;
            internal int Size;

            public SBTNode(TKey _key, TValue _val)
            {
                key = _key;
                val = _val;
                leftSon = null;
                rightSon = null;
                father = null;
                Size = 1;
            }

            public void UpdateSize()
            {
                Size = 1;
                if (leftSon != null) Size += leftSon.Size;
                if (rightSon != null) Size += rightSon.Size;
            }

            public override string ToString()
            {
                string str = "[";
                str += key.ToString();
                str += "|" + val.ToString();
                str += "|" + Size.ToString();
                str += "|l:" + ((leftSon == null) ? "empty" : leftSon.key.ToString());
                str += "|r:" + ((rightSon == null) ? "empty" : rightSon.key.ToString());
                str += "]";
                return str;
            }
        }
        SBTNode root = null;

        SBTNode Rotate_Left(SBTNode rt)
        {
            SBTNode rrt = rt.rightSon;
            rt.rightSon = rrt.leftSon;
            if (rrt.leftSon != null) rrt.leftSon.father = rt;
            rrt.leftSon = rt;
            rrt.Size = rt.Size;
            rt.UpdateSize();
            if (rt.father != null)
            {
                if (rt.father.key.CompareTo(rrt.key) > 0)
                    rt.father.leftSon = rrt;
                else
                    rt.father.rightSon = rrt;
            }
            else root = rrt;
            rrt.father = rt.father;
            rt.father = rrt;
            return rrt;
        }

        SBTNode Rotate_Right(SBTNode rt)
        {
            SBTNode lrt = rt.leftSon;
            rt.leftSon = lrt.rightSon;
            if(lrt.rightSon != null) lrt.rightSon.father = rt;
            lrt.rightSon = rt;
            lrt.Size = rt.Size;
            rt.UpdateSize();
            if (rt.father != null)
            {
                if (rt.father.key.CompareTo(lrt.key) > 0)
                    rt.father.leftSon = lrt;
                else
                    rt.father.rightSon = lrt;
            }
            else root = lrt;
            lrt.father = rt.father;
            rt.father = lrt;
            return lrt;
        }

        SBTNode Maintain(SBTNode rt, bool leftMore)
        {
            if (rt == null) return null;
            if (leftMore)
            {
                int size_ll = 0;
                int size_lr = 0;
                int size_r = 0;
                if (rt.leftSon != null)
                {
                    if (rt.leftSon.leftSon != null) size_ll = rt.leftSon.leftSon.Size;
                    if (rt.leftSon.rightSon != null) size_lr = rt.leftSon.rightSon.Size;
                }
                if (rt.rightSon != null) size_r = rt.rightSon.Size;

                if (size_ll > size_r)
                {
                    rt = Rotate_Right(rt);
                }
                else if (size_lr > size_r)
                {
                    Rotate_Left(rt.leftSon);
                    rt = Rotate_Right(rt);
                }
                else return rt;
            }
            else
            {
                int size_rl = 0;
                int size_rr = 0;
                int size_l = 0;
                if (rt.rightSon != null)
                {
                    if (rt.rightSon.leftSon != null) size_rl = rt.rightSon.leftSon.Size;
                    if (rt.rightSon.rightSon != null) size_rr = rt.rightSon.rightSon.Size;
                }
                if (rt.leftSon != null) size_l = rt.leftSon.Size;

                if (size_rr > size_l)
                {
                    rt = Rotate_Left(rt);
                }
                else if (size_rl > size_l)
                {
                    Rotate_Right(rt.rightSon);
                    rt = Rotate_Left(rt);
                }
                else return rt;
            }
            Maintain(rt.leftSon, true);
            Maintain(rt.rightSon, false);
            rt = Maintain(rt, true);
            rt = Maintain(rt, false);

            return rt;
        }

        //G:返回值为true代表这个key本身就存在，false表示key为新加
        bool SetVal(SBTNode rt, SBTNode father,bool onLeft, TKey key, TValue val)
        {
            if(rt == null)
            {
                rt = new SBTNode(key, val);
                rt.father = father;
                if (father == null) root = rt;
                else
                {
                    if (onLeft)
                        father.leftSon = rt;
                    else
                        father.rightSon = rt;
                }
                return false;
            }
            int c = rt.key.CompareTo(key);
            if (c > 0)
            {
                bool alreadyHave = SetVal(rt.leftSon, rt,true, key, val);
                if(!alreadyHave)
                {
                    rt.Size++;
                    Maintain(rt, true);
                }
                return alreadyHave;
            }
            else if (c < 0)
            {
                bool alreadyHave = SetVal(rt.rightSon, rt,false, key, val);
                if (!alreadyHave)
                {
                    rt.Size++;
                    Maintain(rt, false);
                }
                return alreadyHave;
            }
            else
            {
                rt.val = val;
                return true;
            }
        }

        public bool ConstainsKey(TKey key)
        {
            SBTNode rt = root;
            while(rt!=null)
            {
                int c = rt.key.CompareTo(key);
                if (c == 0) return true;
                else if (c > 0) rt = rt.leftSon;
                else rt = rt.rightSon;
            }
            return false;
        }

        SBTNode DelMax(SBTNode rt)
        {
            if(rt.rightSon == null)
            {
                if (rt.father != null)
                {
                    if (rt.father.key.CompareTo(rt.key) > 0)
                        rt.father.leftSon = rt.leftSon;
                    else
                        rt.father.rightSon = rt.leftSon;
                }
                else root = rt.leftSon;
                if (rt.leftSon != null)
                {
                    rt.leftSon.father = rt.father;
                }
                rt.Size = 1;
                rt.leftSon = rt.rightSon = rt.father = null;
                return rt;
            }
            rt.Size--;
            SBTNode node = DelMax(rt.rightSon);
            Maintain(rt, true);
            return node;
        }

        bool Remove(SBTNode rt, TKey key)
        {
            if (rt == null) return false;
            int c = rt.key.CompareTo(key);
            if ( c > 0)
            {
                bool alreadyHave = Remove(rt.leftSon, key);
                if(alreadyHave)
                {
                    rt.Size--;
                    Maintain(rt, false);
                }
                return alreadyHave;
            }
            else if(c < 0)
            {
                bool alreadyHave = Remove(rt.rightSon, key);
                if (alreadyHave)
                {
                    rt.Size--;
                    Maintain(rt, true);
                }
                return alreadyHave;
            }
            else
            {
                if(rt.leftSon == null)
                { 
                    //DebugInfoDfs(rt);
                    if(rt.father != null)
                    {
                        if (rt.father.key.CompareTo(rt.key) > 0)
                            rt.father.leftSon = rt.rightSon;
                        else
                            rt.father.rightSon = rt.rightSon;
                    }
                    else
                    {
                        root = rt.rightSon;
                    }
                    if (rt.rightSon != null)
                    {
                        rt.rightSon.father = rt.father;
                    }
                    rt.father = rt.rightSon = rt.leftSon = null;
                }
                else
                { 
                    //DebugInfoDfs(rt);
                    SBTNode leftMax = DelMax(rt.leftSon);
                    leftMax.leftSon = rt.leftSon;
                    leftMax.rightSon = rt.rightSon;
                    if (leftMax.leftSon != null) leftMax.leftSon.father = leftMax;
                    if (leftMax.rightSon != null) leftMax.rightSon.father = leftMax;
                    leftMax.UpdateSize();
                    if(rt.father != null)
                    {
                        if (rt.father.key.CompareTo(rt.key) > 0)
                            rt.father.leftSon = leftMax;
                        else
                            rt.father.rightSon = leftMax;
                    }
                    else
                    {
                        root = leftMax;
                    }
                    leftMax.father = rt.father;
                    rt.father = rt.leftSon = rt.rightSon = null;
                    Maintain(leftMax, false);
                    Maintain(leftMax, true);
                }
                return true;
            }
        }

        public void SetVal(TKey key, TValue val)
        {
            SetVal(root, null,true, key, val);
        }

        public TValue GetVal(TKey key)
        {
            SBTNode rt = root;
            while (rt != null)
            {
                if (rt.key.CompareTo(key) > 0) return rt.val;
                else if (rt.key.CompareTo(key) < 0) rt = rt.leftSon;
                else rt = rt.rightSon;
            }
            return default(TValue);
        }

        public void Remove(TKey key)
        {
            Remove(root, key);
        }

        void DebugInfoDfs(SBTNode rt)
        {
            if (rt == null) return; 
            DebugInfoDfs(rt.leftSon);
            DebugInfoDfs(rt.rightSon);
        }

        public void DebugInfo()
        { 
            DebugInfoDfs(root);
        }

        public TValue this[TKey key]
        {
            get { return GetVal(key); }
            set { SetVal(key, value); }
        }

        bool test_isok = true;

        Pair<TKey,TKey> Test(SBTNode rt)
        {
            if (rt == null) return null;
            int s_l = 0;
            int s_r = 0;
            int s_ll = 0;
            int s_lr = 0;
            int s_rr = 0;
            int s_rl = 0;
            if(rt.leftSon != null)
            {
                s_l = rt.leftSon.Size;
                if (rt.leftSon.leftSon != null) s_ll = rt.leftSon.leftSon.Size;
                if (rt.leftSon.rightSon != null) s_lr = rt.leftSon.rightSon.Size;
            }
            if(rt.rightSon != null)
            {
                s_r = rt.rightSon.Size;
                if (rt.rightSon.leftSon != null) s_rl = rt.rightSon.leftSon.Size;
                if (rt.rightSon.rightSon != null) s_rr = rt.rightSon.rightSon.Size;
            }
            if(s_l < s_rr || s_l < s_rl || s_r<s_ll || s_r<s_lr)
            {
                test_isok = false;
                //Log.Default.Error("SBT Test ERROR1: " + rt + " " + (s_l < s_rr) + (s_l < s_rl) + (s_r < s_ll) + (s_r < s_lr));
                return null;
            }
            if(rt.Size != s_l + s_r + 1)
            {
                test_isok = false;
                //Log.Default.Error("SBT Test ERROR2: " + rt +" Size不对");
                return null;
            }
            if(rt.leftSon != null && rt.leftSon.key.CompareTo(rt.key) >= 0)
            {
                test_isok = false;
                //Log.Default.Error("SBT Test ERROR3: " + rt + " 左边大于等于中间");
                return null;
            }
            if (rt.rightSon != null && rt.rightSon.key.CompareTo(rt.key) <= 0)
            {
                test_isok = false;
                //Log.Default.Error("SBT Test ERROR3: " + rt + " 右边小于等于中间");
                return null;
            }
            Pair<TKey, TKey> lres = Test(rt.leftSon);
            Pair<TKey, TKey> rres = Test(rt.rightSon);
            if(lres!=null)
            {
                if(lres.v.CompareTo(rt.key)>=0)
                {
                    test_isok = false;
                    //Log.Default.Error("SBT Test ERROR4: " + rt + " 左子树中最大值大于等于中间");
                    return null;
                }
            }
            if (rres != null)
            {
                if (rres.v.CompareTo(rt.key) <= 0)
                {
                    test_isok = false;
                    //Log.Default.Error("SBT Test ERROR4: " + rt + " 右子树中最小值小于等于中间");
                    return null;
                }
            }
            Pair<TKey, TKey> res = new Pair<TKey, TKey>();
            res.k = lres != null ? lres.k : rt.key;
            res.v = rres != null ? rres.v : rt.key;
            return res;
        }

        public bool SelfTest()
        {
            test_isok = true;
            //Log.Default.Debug("","Test:");
            if (root == null)
            {
                return true;
            }
            else
            {
                Test(root);
            }
            //Log.Default.Debug("","---TestOver---");
            return test_isok;
        }*/
    }
}