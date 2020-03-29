using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CAI
{
    public class Edge
    {
        public Vector2 start;
        public Vector2 end;

        public Edge(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
        }

        public bool IsContinuous(Edge other)
        {
            return this.start == other.start
                || this.start == other.end
                || this.end == other.start
                || this.end == other.end;
        }
    }

    public class ManyCornerPolygon
    {
        public List<Vector3> vertices;
        public int startIndex;
        public int endIndex;
        public float areaSize;
        public Vector3 start => vertices[startIndex];
        public Vector3 end => vertices[endIndex];

        public static EqualityComparer equalityComparer = new EqualityComparer();
        public class EqualityComparer : IEqualityComparer<ManyCornerPolygon>
        {
            public bool Equals(ManyCornerPolygon x, ManyCornerPolygon y)
            {
                return Mathf.Min(x.startIndex, x.endIndex) <= Mathf.Min(y.startIndex, y.endIndex)
                    && Mathf.Max(x.startIndex, x.endIndex) <= Mathf.Max(y.startIndex, y.endIndex);
            }

            public int GetHashCode(ManyCornerPolygon obj)
            {
                return (obj.startIndex.GetHashCode() << 16) + obj.endIndex.GetHashCode();
            }
        }
    }

    public class ShapeUtility
    {
        static List<ManyCornerPolygon> GetHinge(List<Vector3> verteces)
        {
            var infos = verteces
                .Select((v, i) => GetMostFarPointByAreaSize(verteces, i))
                .ToDebugArray();

            infos = infos
                .Distinct(ManyCornerPolygon.equalityComparer).ToArray();
            return infos
                .Where(info => !infos.Any(_info => IsInclude(_info, info)))
                .ToList();
        }

        static bool IsInclude(ManyCornerPolygon includer, ManyCornerPolygon includee)
        {
            if(includer == includee)
            {
                return false;
            }

            return includer.startIndex <= includee.startIndex && includer.endIndex >= includee.endIndex;
        }

        static ManyCornerPolygon GetMostFarPointByAreaSize(List<Vector3> verteces, int taergetIndex)
        {
            int maxIndex = 0;
            ManyCornerPolygon maxValue = null;
            for (int i = 0, il = verteces.Count; i < il; i++)
            {
                if(IsCross(verteces, new Edge(verteces[i], verteces[taergetIndex])))
                {
                    continue;
                }

                int start = Mathf.Min(i, taergetIndex);
                int end = Mathf.Max(i, taergetIndex);
                int count = end - start + 1;
                List<Vector3> span = verteces.GetRange(start, count);
                float distance = GetAreaSize(span);

                if(maxValue == null || maxValue.areaSize < distance)
                {
                    maxIndex = i;
                    maxValue = new ManyCornerPolygon()
                    {
                        vertices = verteces,
                        startIndex = start,
                        endIndex = end,
                        areaSize = distance,
                    };
                }
            }
            return maxValue; 
        }

        /// <summary>
        /// 頂点配列をループしない線とみなし、edgeが交差しているかを判定。
        /// </summary>
        static bool IsCross(List<Vector3> vertices, Edge edge)
        {
            return Split(vertices, false)
                .Any(e => IsCross(e, edge));
        }

        static bool IsCross(Edge edge1, Edge edge2)
        {
            var orthantC = (edge1.start.x - edge1.end.x) * (edge2.start.y - edge1.start.y) + (edge1.start.y - edge1.end.y) * (edge1.start.x - edge2.start.x);
            var orthantD = (edge1.start.x - edge1.end.x) * (edge2.end.y - edge1.start.y) + (edge1.start.y - edge1.end.y) * (edge1.start.x - edge2.end.x);
            var orthantA = (edge2.start.x - edge2.end.x) * (edge1.start.y - edge2.start.y) + (edge2.start.y - edge2.end.y) * (edge2.start.x - edge1.start.x);
            var orthantB = (edge2.start.x - edge2.end.x) * (edge1.end.y - edge2.start.y) + (edge2.start.y - edge2.end.y) * (edge2.start.x - edge1.end.x);
            bool isCross = orthantC * orthantD < 0 && orthantA * orthantB < 0;
            return isCross;
        }

        /// <summary>
        /// 頂点配列をループした多角形とみなし面積を返す。
        /// </summary>
        static float GetAreaSize(List<Vector3> vertices)
        {
            if(vertices.Count < 3)
            {
                return 0;
            }

            var areaSize = Split(vertices, true)
                .Select(e => Vector3.Cross((Vector2)e.start, (Vector2)e.end))
                .Select(v3 => v3.z)
                .Sum();
            return Mathf.Abs(areaSize);
        }
    
        static List<Edge> Split(List<Vector3> vertices, bool isLoop)
        {
            var retValue = new List<Edge>();
            if(vertices.Count < 2)
            {
                return retValue;
            }


            for (int i = 0, il = vertices.Count - 1; i < il; i++)
            {
                var start = vertices[i];
                var end = vertices[i + 1];
                retValue.Add(new Edge(start, end));
            }

            if(isLoop)
            {
                retValue.Add(new Edge(vertices[vertices.Count - 1], vertices[0]));
            }

            return retValue;
        }
    }
}