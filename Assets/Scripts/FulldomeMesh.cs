// using UnityEngine;
// using System.Collections.Generic;

// namespace Avante
// {
//     public class FulldomeMesh
//     {
//         public float horizon = 0;

//         float wireStep = 45.0f * Mathf.Deg2Rad;
//         float meshStep = 10.0f * Mathf.Deg2Rad;

//         float PI2 = Mathf.PI * 2.0f;
//         float PI05 = Mathf.PI * 0.5f;

//         List<Vector3> _meshPoints = new List<Vector3>();
//         List<Vector3> _meshNormals = new List<Vector3>();
//         List<int> _meshTriangles = new List<int>();

//         public Mesh GenerateDomeMesh(float radius, float horizon)
//         {
//             float fovH = PI2;
//             float fovVN = HorizonToAltitude(horizon * Mathf.Deg2Rad);
//             float fovVP = PI05;

//             float fovHN = -PI05 + fovH * -0.5f;
//             float fovHP = -PI05 + fovH * 0.5f;

//             // Generate mesh
//             for (float lat = fovVN; lat < fovVP;)
//             {
//                 float lat2 = NextAngle(lat, fovVN, fovVP, meshStep);
//                 for (float lng = fovHN; lng < fovHP;)
//                 {
//                     float lng2 = NextAngle(lng, fovHN, fovHP, meshStep);
//                     Vector3[] ps = new Vector3[4];
//                     ps[0] = LatLngToDome(lat, lng) * radius;
//                     ps[1] = LatLngToDome(lat, lng2) * radius;
//                     ps[2] = LatLngToDome(lat2, lng2) * radius;
//                     ps[3] = LatLngToDome(lat2, lng) * radius;
//                     AddMeshQuad(ps);
//                     lng = lng2;
//                 }
//                 lat = lat2;
//             }

//             // Create mesh
//             Mesh domeMesh = new Mesh();
//             domeMesh.vertices = _meshPoints.ToArray();
//             domeMesh.normals = _meshNormals.ToArray();
//             domeMesh.triangles = _meshTriangles.ToArray();
//             domeMesh.RecalculateBounds();

            

//             return domeMesh;
//         }

//         void AddMeshQuad(Vector3[] ps)
//         {
//             int start = _meshPoints.Count;
//             for (int i = 0; i < 4; ++i)
//             {
//                 _meshPoints.Add(ps[i]);
//                 _meshNormals.Add(ps[i].normalized);
//             }
//             _meshTriangles.Add(start + 0);
//             _meshTriangles.Add(start + 1);
//             _meshTriangles.Add(start + 2);
//             _meshTriangles.Add(start + 0);
//             _meshTriangles.Add(start + 2);
//             _meshTriangles.Add(start + 3);
//         }

//         float NextAngle(float a, float from, float to, float step)
//         {
//             if (a == from)
//             {
//                 float mod = Mathf.Abs(a % step);
//                 if (mod < 0.001)
//                     a += step;
//                 else if (a < 0)
//                     a += mod;
//                 else
//                     a += (step - mod);
//             }
//             else if (a == to)
//             {
//                 a += step;
//                 return a;
//             }
//             else
//                 a += step;
//             if (a > to)
//                 a = to;
//             return a;
//         }

//         float HorizonToAltitude(float h)
//         {
//             return (PI05 - (h * 0.5f));
//         }

//         Vector3 LatLngToDome(float lat, float lng)
//         {
//             float r = Mathf.Cos(lat);
//             float x = Mathf.Cos(lng) * r;
//             float y = Mathf.Sin(lng) * r;
//             float z = Mathf.Sin(lat);
//             return new Vector3(x, y, z);
//         }
//     }
// }
