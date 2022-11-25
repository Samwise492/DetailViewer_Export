using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

    public class SlicesMetadata : MonoBehaviour
    {
        Mesh _frontSideMesh;
        List<Vector3> _frontSideVertices;
        List<int> _frontSideTriangles;
        List<Vector2> _frontSideUvs;
        List<Vector3> _frontSideNormals;

        Mesh _backSideMesh;
        List<Vector3> _backSideVertices;
        List<int> _backSideTriangles;
        List<Vector2> _backSideUvs;
        List<Vector3> _backSideNormals;

        List<Vector3> _pointsAlongPlane;
        Plane _plane;
        Mesh _mesh;

        public Mesh frontSideMesh
        {
            get
            {
                if (_frontSideMesh == null)
                {
                    _frontSideMesh = new Mesh();
                }

                SetMeshData(MeshSide.front);
                return _frontSideMesh;
            }
        }
        public Mesh backSideMesh
        {
            get
            {
                if (_backSideMesh == null)
                {
                    _backSideMesh = new Mesh();
                }

                SetMeshData(MeshSide.back);

                return _backSideMesh;
            }
        }
        public enum MeshSide
        {
            front = 0,
            back = 1
        }
        public SlicesMetadata(Plane plane, Mesh mesh)
        {
            _frontSideTriangles = new List<int>();
            _frontSideVertices = new List<Vector3>();
            _backSideTriangles = new List<int>();
            _backSideVertices = new List<Vector3>();
            _frontSideUvs = new List<Vector2>();
            _backSideUvs = new List<Vector2>();
            _frontSideNormals = new List<Vector3>();
            _backSideNormals = new List<Vector3>();
            _pointsAlongPlane = new List<Vector3>();
            _plane = plane;
            _mesh = mesh;

            ComputeNewMeshes();
        }

        // Добавить данные меша к стороне и просчитать вектор, перпендикулярный плоскости
        void AddTrianglesNormalAndUvs(MeshSide side, Vector3 vertex1, Vector3? normal1, Vector2 uv1, 
        Vector3 vertex2, Vector3? normal2, Vector2 uv2, Vector3 vertex3, Vector3? normal3, Vector2 uv3, 
        bool shareVertices, bool addFirst)
        {
            if (side == MeshSide.front)
            {
                AddTrianglesNormalsAndUvs(ref _frontSideVertices, ref _frontSideTriangles, ref _frontSideNormals, ref _frontSideUvs, 
                vertex1, normal1, uv1, vertex2, normal2, uv2, vertex3, normal3, uv3, shareVertices, addFirst);
            }
            else
            {
                AddTrianglesNormalsAndUvs(ref _backSideVertices, ref _backSideTriangles, ref _backSideNormals, ref _backSideUvs, 
                vertex1, normal1, uv1, vertex2, normal2, uv2, vertex3, normal3, uv3, shareVertices, addFirst);
            }
        }

        // Добавляет вершины к мешу, устанавливает треугольники в том порядке, в котором вершины были предоставлены.
        // Если значение общих вершин false, вершины будут добавлены в список, даже если такая вершина уже существует
        void AddTrianglesNormalsAndUvs(ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector3> normals, ref List<Vector2> uvs, 
        Vector3 vertex1, Vector3? normal1, Vector2 uv1, Vector3 vertex2, Vector3? normal2, Vector2 uv2, 
        Vector3 vertex3, Vector3? normal3, Vector2 uv3, bool shareVertices, bool addFirst)
        {
            int tri1Index = vertices.IndexOf(vertex1);
            if (addFirst)
            {
                ShiftTriangleIndeces(ref triangles);
            }

            // Если вершина уже есть, добавляем ссылку треугольника на нее. Иначе добавляем вершину в список и потом добавляем индекс треугол-ка
            if (tri1Index > -1 && shareVertices)
            {                
                triangles.Add(tri1Index);
            }
            else
            {
                if (normal1 == null)
                {
                    normal1 = ComputeNormal(vertex1, vertex2, vertex3);                    
                }

                int? i = null;
                if (addFirst)
                {
                    i = 0;
                }

                AddVertNormalUv(ref vertices, ref normals, ref uvs, ref triangles, vertex1, (Vector3)normal1, uv1, i);
            }

            int tri2Index = vertices.IndexOf(vertex2);

            if (tri2Index > -1 && shareVertices)
            {
                triangles.Add(tri2Index);
            }
            else
            {
                if (normal2 == null)
                {
                    normal2 = ComputeNormal(vertex2, vertex3, vertex1);
                }
                
                int? i = null;
                
                if (addFirst)
                {
                    i = 1;
                }

                AddVertNormalUv(ref vertices, ref normals, ref uvs, ref triangles, vertex2, (Vector3)normal2, uv2, i);
            }

            int tri3Index = vertices.IndexOf(vertex3);

            if (tri3Index > -1 && shareVertices)
            {
                triangles.Add(tri3Index);
            }
            else
            {               
                if (normal3 == null)
                {
                    normal3 = ComputeNormal(vertex3, vertex1, vertex2);
                }

                int? i = null;
                if (addFirst)
                {
                    i = 2;
                }

                AddVertNormalUv(ref vertices, ref normals, ref uvs, ref triangles, vertex3, (Vector3)normal3, uv3, i);
            }
        }

        void AddVertNormalUv(ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Vector2> uvs, ref List<int> triangles, 
        Vector3 vertex, Vector3 normal, Vector2 uv, int? index)
        {
            if (index != null)
            {
                int i = (int)index;
                vertices.Insert(i, vertex);
                uvs.Insert(i, uv);
                normals.Insert(i, normal);
                triangles.Insert(i, i);
            }
            else
            {
                vertices.Add(vertex);
                normals.Add(normal);
                uvs.Add(uv);
                triangles.Add(vertices.IndexOf(vertex));
            }
        }

        void ShiftTriangleIndeces(ref List<int> triangles)
        {
            for (int j = 0; j < triangles.Count; j += 3)
            {
                triangles[j] += + 3;
                triangles[j + 1] += 3;
                triangles[j + 2] += 3;
            }
        }

        // Соединяем точки вместе вдоль плоскости с точкой в середине
        void JoinPointsAlongPlane()
        {
            Vector3 halfway = GetHalfwayPoint(out float distance);

            for (int i = 0; i < _pointsAlongPlane.Count; i += 2)
            {
                Vector3 firstVertex;
                Vector3 secondVertex;

                firstVertex = _pointsAlongPlane[i];
                secondVertex = _pointsAlongPlane[i + 1];

                Vector3 normal3 = ComputeNormal(halfway, secondVertex, firstVertex);
                normal3.Normalize();

                var direction = Vector3.Dot(normal3, _plane.normal);

                if(direction > 0)
                {                                        
                    AddTrianglesNormalAndUvs(MeshSide.front, halfway, -normal3, Vector2.zero, firstVertex, -normal3, Vector2.zero, secondVertex, -normal3, Vector2.zero, false, true);
                    AddTrianglesNormalAndUvs(MeshSide.back, halfway, normal3, Vector2.zero, secondVertex, normal3, Vector2.zero, firstVertex, normal3, Vector2.zero, false, true);
                }
                else
                {
                    AddTrianglesNormalAndUvs(MeshSide.front, halfway, normal3, Vector2.zero, secondVertex, normal3, Vector2.zero, firstVertex, normal3, Vector2.zero, false, true);
                    AddTrianglesNormalAndUvs(MeshSide.back, halfway, -normal3, Vector2.zero, firstVertex, -normal3, Vector2.zero, secondVertex, -normal3, Vector2.zero, false, true);
                }               
            }
        }

        // Для всех точек, добавленных вдоль разреза плоскости, найдем половину отрезка между первой и самой удаленной точкой
        Vector3 GetHalfwayPoint(out float distance)
        {
            if(_pointsAlongPlane.Count > 0)
            {
                Vector3 firstPoint = _pointsAlongPlane[0];
                Vector3 furthestPoint = Vector3.zero;
                distance = 0f;

                foreach (Vector3 point in _pointsAlongPlane)
                {
                    float currentDistance = 0f;
                    currentDistance = Vector3.Distance(firstPoint, point);

                    if (currentDistance > distance)
                    {
                        distance = currentDistance;
                        furthestPoint = point;
                    }
                }

                return Vector3.Lerp(firstPoint, furthestPoint, 0.5f);
            }
            else
            {
                distance = 0;
                return Vector3.zero;
            }
        }

        // Настройка объекта меша для указанной стороны
        void SetMeshData(MeshSide side)
        {
            if (side == MeshSide.front)
            {
                _frontSideMesh.vertices = _frontSideVertices.ToArray();
                _frontSideMesh.triangles = _frontSideTriangles.ToArray();
                _frontSideMesh.normals = _frontSideNormals.ToArray();
                _frontSideMesh.uv = _frontSideUvs.ToArray();
            }
            else
            {
                _backSideMesh.vertices = _backSideVertices.ToArray();
                _backSideMesh.triangles = _backSideTriangles.ToArray();
                _backSideMesh.normals = _backSideNormals.ToArray();
                _backSideMesh.uv = _backSideUvs.ToArray();                
            }
        }

        // Вычисление переднего и заднего меша на основе плоскости и меша
        void ComputeNewMeshes()
        {
            int[] meshTriangles = _mesh.triangles;
            Vector3[] meshVerts = _mesh.vertices;
            Vector3[] meshNormals = _mesh.normals;
            Vector2[] meshUvs = _mesh.uv;

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                // Вершины нужны по порядку, чтобы знать, в какую сторону накладывать  новые треугольники меша
                Vector3 vert1 = meshVerts[meshTriangles[i]];
                int vert1Index = Array.IndexOf(meshVerts, vert1);
                Vector2 uv1 = meshUvs[vert1Index];
                Vector3 normal1 = meshNormals[vert1Index];
                bool vert1Side = _plane.GetSide(vert1);

                Vector3 vert2 = meshVerts[meshTriangles[i + 1]];
                int vert2Index = Array.IndexOf(meshVerts, vert2);
                Vector2 uv2 = meshUvs[vert2Index];
                Vector3 normal2 = meshNormals[vert2Index];
                bool vert2Side = _plane.GetSide(vert2);

                Vector3 vert3 = meshVerts[meshTriangles[i + 2]];
                bool vert3Side = _plane.GetSide(vert3);
                int vert3Index = Array.IndexOf(meshVerts, vert3);
                Vector3 normal3 = meshNormals[vert3Index];
                Vector2 uv3 = meshUvs[vert3Index];

                // Если все вершины на одной и той же стороне
                if (vert1Side == vert2Side && vert2Side == vert3Side)
                {
                    // Добавить треугольник
                    MeshSide side = (vert1Side) ? MeshSide.front : MeshSide.back;
                    AddTrianglesNormalAndUvs(side, vert1, normal1, uv1, vert2, normal2, uv2, vert3, normal3, uv3, true, false);
                }
                else
                {
                    // Нам нужны две точки, где плоскость пересекает треугольник
                    Vector3 intersection1;
                    Vector3 intersection2;

                    Vector2 intersection1Uv;
                    Vector2 intersection2Uv;

                    MeshSide side1 = (vert1Side) ? MeshSide.front : MeshSide.back;
                    MeshSide side2 = (vert1Side) ? MeshSide.back : MeshSide.front;

                    // Если вершины на одной стороне
                    if (vert1Side == vert2Side)
                    {
                        // Проводим луч от v2 к v3 и от v3 к v1, чтобы получить пересечения                    
                        intersection1 = GetRayPlaneIntersectionPointAndUv(vert2, uv2, vert3, uv3, out intersection1Uv);
                        intersection2 = GetRayPlaneIntersectionPointAndUv(vert3, uv3, vert1, uv1, out intersection2Uv);

                        // Добавляем треугольники задней/передней стороны
                        AddTrianglesNormalAndUvs(side1, vert1, null, uv1, vert2, null, uv2, intersection1, null, intersection1Uv, false, false);
                        AddTrianglesNormalAndUvs(side1, vert1, null, uv1, intersection1, null, intersection1Uv, intersection2, null, intersection2Uv, false, false);

                        AddTrianglesNormalAndUvs(side2, intersection1, null, intersection1Uv, vert3, null, uv3, intersection2, null, intersection2Uv, false, false);

                    }
                    else if (vert1Side == vert3Side)
                    {                   
                        intersection1 = GetRayPlaneIntersectionPointAndUv(vert1, uv1, vert2, uv2, out intersection1Uv);
                        intersection2 = GetRayPlaneIntersectionPointAndUv(vert2, uv2, vert3, uv3, out intersection2Uv);

                        AddTrianglesNormalAndUvs(side1, vert1, null, uv1, intersection1, null, intersection1Uv, vert3, null, uv3, false, false);
                        AddTrianglesNormalAndUvs(side1, intersection1, null, intersection1Uv, intersection2, null, intersection2Uv, vert3, null, uv3, false, false);

                        AddTrianglesNormalAndUvs(side2, intersection1, null, intersection1Uv, vert2, null, uv2, intersection2, null, intersection2Uv, false, false);
                    }
                    // Есть только 1 вершина
                    else
                    {                      
                        intersection1 = GetRayPlaneIntersectionPointAndUv(vert1, uv1, vert2, uv2, out intersection1Uv);
                        intersection2 = GetRayPlaneIntersectionPointAndUv(vert1, uv1, vert3, uv3, out intersection2Uv);

                        AddTrianglesNormalAndUvs(side1, vert1, null, uv1, intersection1, null, intersection1Uv, intersection2, null, intersection2Uv, false, false);

                        AddTrianglesNormalAndUvs(side2, intersection1, null, intersection1Uv, vert2, null, uv2, vert3, null, uv3, false, false);
                        AddTrianglesNormalAndUvs(side2, intersection1, null, intersection1Uv, vert3, null, uv3, intersection2, null, intersection2Uv, false, false);
                    }

                    // Добавляем новые созданные точки на плоскость
                    _pointsAlongPlane.Add(intersection1);
                    _pointsAlongPlane.Add(intersection2);
                }
            }

            JoinPointsAlongPlane();
        }

        // Делаем луч из 1-й вершины во 2-ю и получаем точку пересечения с плоскостью
         Vector3 GetRayPlaneIntersectionPointAndUv(Vector3 vertex1, Vector2 vertex1Uv, Vector3 vertex2, Vector2 vertex2Uv, out Vector2 uv)
        {
            float distance = GetDistanceRelativeToPlane(vertex1, vertex2, out Vector3 pointOfIntersection);
            uv = InterpolateUvs(vertex1Uv, vertex2Uv, distance);
            return pointOfIntersection;
        }

        // Считаем расстояние, зная плоскость
         float GetDistanceRelativeToPlane(Vector3 vertex1, Vector3 vertex2, out Vector3 pointOfintersection)
        {
            Ray ray = new Ray(vertex1, (vertex2 - vertex1));
            _plane.Raycast(ray, out float distance);
            pointOfintersection = ray.GetPoint(distance);
            return distance;
        }

        // Получаем позицию текстуры (uv) по расстоянию между 2-мя другими uv
         Vector2 InterpolateUvs(Vector2 uv1, Vector2 uv2, float distance)
        {
            Vector2 uv = Vector2.Lerp(uv1, uv2, distance);
            return uv;
        }

        // Получаем точку, перпендикулярную грани, определяемой вершинами 
         Vector3 ComputeNormal(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {
            Vector3 side1 = vertex2 - vertex1;
            Vector3 side2 = vertex3 - vertex1;

            Vector3 normal = Vector3.Cross(side1, side2);

            return normal;
        }
    }

