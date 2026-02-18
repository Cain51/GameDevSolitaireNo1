using System;
using System.Collections.Generic;
using System.Text;

namespace RG.Basic
{
    //G:相对于Hash 只保存key有无存在，相当于一个集合
    public class Set
    {
        const int PRIME_SEED = 1007;
        public class SetNode
        {
            public int key;
            public SetNode() { }
            public SetNode(int _key) { key = _key;}
        }
        Seq<SetNode>[] setTable = new Seq<SetNode>[PRIME_SEED + 3];

        public Set() { }
        public void Clear()
        {
            setTable = new Seq<SetNode>[PRIME_SEED + 3];
        }

        public bool ContainsKey(int key)
        {
            int index = HashIndex(key);
            if (setTable[index] != null)
            {
                for (int i = 0; i < setTable[index].Count; i++)
                    if (setTable[index][i].key == key)
                        return true;
            }
            return false;
        }

        int HashIndex(int code)
        {
            return code %= PRIME_SEED;
        }

        public void Add(int key)
        {
            int index = HashIndex(key);
            if (setTable[index] == null) setTable[index] = new Seq<SetNode>();
            bool alreadyHave = false;
            for (int i = 0; i < setTable[index].Count; i++)
                if (setTable[index][i].key == key)
                {
                    alreadyHave = true;
                }
            if (!alreadyHave) {
                setTable[index].Add(new SetNode(key));
                count += 1;
            }
        }

        public int Count { get { return count; } }
        private int count = 0;

        public void Remove(int key)
        {
            int index = HashIndex(key);
            if (setTable[index] == null) return;
            int seqLenBeforeRemove = setTable[index].Count;
            setTable[index].RemoveX(x => x.key == key);
            count -= (seqLenBeforeRemove - setTable[index].Count);
        }

        public void RemoveAll()
        {
            for (int i = 0; i < PRIME_SEED; i++)
                setTable[i] = null;
            count = 0;
        }

        public void Foreach(System.Action<int> func)
        {
            for(int i=0;i<PRIME_SEED;i++)
            {
                if(setTable[i] != null)
                {
                    for(int j=0;j<setTable[i].Count;j++)
                    {
                        func(setTable[i][j].key);
                    }
                }
            }
        } 

        public Seq<int> GetKeys() {
            Seq<int> keys = new Seq<int>();
            for (int i = 0; i < PRIME_SEED; i++) if (setTable[i] != null) keys.Add(i);
            return keys;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder(" ");
            Foreach(v => {
                sb.Append(v).Append(",");
            });
            return base.ToString() + sb.ToString();
        }
    }
}
