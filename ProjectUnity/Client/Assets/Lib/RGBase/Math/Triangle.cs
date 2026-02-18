using UnityEngine;
using System.Collections;

namespace RG.Basic.Math {
    public class Triangle {
        public Vct2[] point = new Vct2[3] ;

        public Vct2 A { get { return point[0]; } }
        public Vct2 B { get { return point[1]; } }
        public Vct2 C { get { return point[2]; } }

        public Triangle(Vct2 a,Vct2 b,Vct2 c) { point[0] = a; point[1] = b; point[2] = c; }

        public Triangle(Vct2[] p) { for (int i = 0; i < 3; i++) point[i] = p[i]; }

        public bool Contain(Vct2 position) {
            int dirCount = 0;
            for (int i = 0; i < 3; i++) {
                Vct2 dir_a = point[i] - position;
                Vct2 dir_b = point[i] - point[(i + 1) % 3];
                if (Vct2.Cross(dir_a, dir_b) >= 0) dirCount++;
            }
            return (dirCount % 3) == 0;
        }
    }
}

