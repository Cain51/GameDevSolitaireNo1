using System.Collections;
using RG.Basic.Math;

namespace RG.Basic {
    public static class GiuAlgorithm {

        class VertexInfo {
            public int currIndex;
            public int frontIndex;
            public int backIndex;
            public Vct2 currPosition;
            public Vct2 frontPosition;
            public Vct2 backPosition;
            public enum VaildState { NoJudge, Invaild, vaild }
            public VaildState vaild;
            public float dotV;
            public bool connected;
        }

        static void UpdateVertexInfo(ref VertexInfo info) {
            Vct2 dir0 = info.frontPosition - info.currPosition;
            Vct2 dir1 = info.currPosition - info.backPosition;
            if (Vct2.Cross(dir0, dir1) > 0) info.vaild = VertexInfo.VaildState.Invaild;//因为逆时针排列点，所以大于0的是凹点，内角大于180度
            else info.vaild = VertexInfo.VaildState.NoJudge;
            info.dotV = Vct2.Dot(dir0.normal, dir1.normal);
        }

        static void JudgeVertexVaild(int index, Seq<VertexInfo> vertexs) {

            Vct2[] triangle = { vertexs[index].frontPosition, vertexs[index].currPosition, vertexs[index].backPosition };
            for (int i = 0; i < vertexs.Count; i++) {
                var v = vertexs[i];
                if (v.connected) continue;
                if (v.currIndex == vertexs[index].currIndex
                    || v.currIndex == vertexs[index].backIndex
                    || v.currIndex == vertexs[index].frontIndex) continue;
                //判断是否有未连接的点在该三角形内部，如果在，则该点目前不可用
                bool outTriangle = false;
                for (int j = 0; j < 3; j++) {
                    outTriangle = Vct2.Cross(triangle[(j + 1) % 3] - triangle[j], v.currPosition - triangle[j]) > 0;
                    if (outTriangle) break;
                }
                if (!outTriangle) {
                    vertexs[index].vaild = VertexInfo.VaildState.Invaild;
                    return;
                }
            }
            vertexs[index].vaild = VertexInfo.VaildState.vaild;
        }

        public static Seq<int> PolygonTriangulation(Seq<Vct2> polygon) {
            Seq<int> triangles = new Seq<int>();
            Seq<VertexInfo> vertexs = new Seq<VertexInfo>();
            for (int i = 0; i < polygon.Count; i++) {
                VertexInfo info = new VertexInfo();
                info.currIndex = i;
                info.frontIndex = (i + polygon.Count - 1) % polygon.Count;
                info.backIndex = (i + 1) % polygon.Count;
                info.currPosition = polygon[info.currIndex];
                info.frontPosition = polygon[info.frontIndex];
                info.backPosition = polygon[info.backIndex];
                info.connected = false;
                UpdateVertexInfo(ref info);
                vertexs.Add(info);
            }
            for (int h = 0; h < polygon.Count - 3; h++) {
                float minDotV = 1000;
                int index = -1;
                for (int i = 0; i < vertexs.Count; i++) {
                    var info = vertexs[i];
                    if (info.connected || info.vaild == VertexInfo.VaildState.Invaild) continue;
                    if (info.dotV >= minDotV) continue;
                    if (info.vaild == VertexInfo.VaildState.NoJudge) {
                        JudgeVertexVaild(i, vertexs);
                    }
                    if (info.vaild == VertexInfo.VaildState.vaild) {
                        minDotV = info.dotV;
                        index = i;
                    }
                }
                if (index == -1) {
                    throw new System.Exception("Error index == -1 ; h = " + h + " ; polygonCount = " + polygon.Count);
                }
                var vertex = vertexs[index];
                triangles.Add(vertex.frontIndex);
                triangles.Add(vertex.currIndex);
                triangles.Add(vertex.backIndex);
                vertexs[index].connected = true;
                if (h == polygon.Count - 4) {
                    var backVertex = vertexs[vertex.backIndex];
                    triangles.Add(vertex.frontIndex);
                    triangles.Add(backVertex.currIndex);
                    triangles.Add(backVertex.backIndex);
                }
                else {
                    var backVertex = vertexs[vertex.backIndex];
                    backVertex.frontIndex = vertex.frontIndex;
                    backVertex.frontPosition = vertex.frontPosition;
                    UpdateVertexInfo(ref backVertex);
                    vertexs[vertex.backIndex] = backVertex;
                    var frontVertex = vertexs[vertex.frontIndex];
                    vertexs[vertex.frontIndex].backIndex = vertex.backIndex;
                    vertexs[vertex.frontIndex].backPosition = vertex.backPosition;
                    UpdateVertexInfo(ref frontVertex);
                    vertexs[vertex.frontIndex] = frontVertex;
                }
            }
            return triangles;
            
        }
    }
}
