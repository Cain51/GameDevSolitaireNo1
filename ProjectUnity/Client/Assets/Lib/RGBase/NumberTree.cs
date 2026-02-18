using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RG.Basic {
    public class NumberTree {

        float[] values = null;
        float[] tree = null;

        public NumberTree(int length) {
            values = new float[length];
            tree = new float[length + 1]; 
        } 

        public int Count { get { return values.Length; } }
        public int TreeCount { get { return tree.Length; } }

        public void Set(int index, float num) {
            float delta = num;
            if (index >= Count) { throw new Exception("[NumberTree Set] argument out of range"); }
            else { delta = num - values[index]; }

            int treeInd = index + 1; 
            while (treeInd < TreeCount) {
                tree[treeInd] += num;
                treeInd += Mth.LowBit(treeInd);
            }
        } 

        public float Get(int index) { return values[index]; }

        public float Sum(int range) { 
            float sum = 0;
            int treeInd = range + 1;
            while (treeInd > 0) {
                sum += tree[treeInd];
                treeInd -= Mth.LowBit(treeInd);
            }
            return sum; 
        }
    }
}
