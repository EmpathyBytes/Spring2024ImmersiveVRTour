using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Builds the Buzz mascot at runtime from the text of an .obj file, one separate mesh object per
    /// "o" part, so the parts can be animated. (Unity's own OBJ importer merges everything into
    /// one mesh.) Part objects keep the names from the file, e.g. Head_0, Upper_arm_22.
    /// </summary>
    public static class BuzzMascotLoader
    {
        private class Part
        {
            public string name, mat;
            public List<int[]> faces = new List<int[]>();
        }

        public static void Build(string objText, Transform parent)
        {
            var verts = new List<Vector3>();
            var parts = new List<Part>();
            Part cur = null;
            string curMat = "gold";
            var inv = CultureInfo.InvariantCulture;

            foreach (var raw in objText.Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length < 2) continue;
                if (line[0] == 'v' && line[1] == ' ')
                {
                    var p = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                    // Negate x to match how Unity imports right-handed OBJ files.
                    verts.Add(new Vector3(-float.Parse(p[1], inv), float.Parse(p[2], inv), float.Parse(p[3], inv)));
                }
                else if (line[0] == 'o' && line[1] == ' ')
                {
                    cur = new Part { name = line.Substring(2).Trim(), mat = curMat };
                    parts.Add(cur);
                }
                else if (line.StartsWith("usemtl "))
                {
                    curMat = line.Substring(7).Trim();
                    if (cur != null) cur.mat = curMat;
                }
                else if (line[0] == 'f' && line[1] == ' ' && cur != null)
                {
                    var p = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                    var idx = new int[p.Length - 1];
                    for (int i = 1; i < p.Length; i++)
                    {
                        int slash = p[i].IndexOf('/');
                        string s = slash >= 0 ? p[i].Substring(0, slash) : p[i];
                        idx[i - 1] = int.Parse(s, inv) - 1;
                    }
                    cur.faces.Add(idx);
                }
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mats = new Dictionary<string, Material>();

            foreach (var part in parts)
            {
                var map = new Dictionary<int, int>();
                var vs = new List<Vector3>();
                var tris = new List<int>();
                foreach (var f in part.faces)
                {
                    var local = new int[f.Length];
                    for (int i = 0; i < f.Length; i++)
                    {
                        if (!map.TryGetValue(f[i], out int li)) { li = vs.Count; map[f[i]] = li; vs.Add(verts[f[i]]); }
                        local[i] = li;
                    }
                    // Fan-triangulate; reversed winding because x was negated.
                    for (int i = 1; i < local.Length - 1; i++) { tris.Add(local[0]); tris.Add(local[i + 1]); tris.Add(local[i]); }
                }
                if (tris.Count == 0) continue;

                var mesh = new Mesh { name = part.name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.SetVertices(vs);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                var go = new GameObject(part.name);
                go.transform.SetParent(parent, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = GetMat(mats, shader, part.mat);

                if (part.name.StartsWith("Striped_abdomen"))
                    AddTailStripes(go.transform, vs, GetMat(mats, shader, "gold"));
            }
        }

        /// <summary>Adds yellow bands around the tail (abdomen) so it reads as striped.</summary>
        private static void AddTailStripes(Transform tail, List<Vector3> vs, Material gold)
        {
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var v in vs) { if (v.y < minY) minY = v.y; if (v.y > maxY) maxY = v.y; }
            float height = maxY - minY;
            float[] centres = { 0.34f, 0.55f, 0.76f };
            const int segs = 44, rings = 7;
            float halfBand = height * 0.045f;
            for (int k = 0; k < centres.Length; k++)
            {
                float y0 = minY + height * centres[k];
                var verts = new List<Vector3>();
                for (int r = 0; r < rings; r++)
                {
                    float y = y0 - halfBand + 2f * halfBand * r / (rings - 1);
                    float tol = height * 0.02f;
                    float x0 = float.MaxValue, x1 = float.MinValue, z0 = float.MaxValue, z1 = float.MinValue; int n = 0;
                    foreach (var v in vs)
                    {
                        if (Mathf.Abs(v.y - y) > tol) continue;
                        n++;
                        if (v.x < x0) x0 = v.x; if (v.x > x1) x1 = v.x;
                        if (v.z < z0) z0 = v.z; if (v.z > z1) z1 = v.z;
                    }
                    if (n < 6) { x0 = -0.1f; x1 = 0.1f; z0 = -0.1f; z1 = 0.1f; }
                    float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;
                    float rx = (x1 - x0) * 0.5f * 1.03f + 0.004f, rz = (z1 - z0) * 0.5f * 1.03f + 0.004f;
                    for (int i = 0; i < segs; i++)
                    {
                        float a = i * 2f * Mathf.PI / segs;
                        verts.Add(new Vector3(cx + rx * Mathf.Cos(a), y, cz + rz * Mathf.Sin(a)));
                    }
                }
                var tris = new List<int>();
                for (int r = 0; r < rings - 1; r++)
                    for (int i = 0; i < segs; i++)
                    {
                        int a = r * segs + i, b = r * segs + (i + 1) % segs, c = (r + 1) * segs + i, d = (r + 1) * segs + (i + 1) % segs;
                        tris.Add(a); tris.Add(b); tris.Add(c); tris.Add(b); tris.Add(d); tris.Add(c);   // outside
                        tris.Add(a); tris.Add(c); tris.Add(b); tris.Add(b); tris.Add(c); tris.Add(d);   // inside (double-sided)
                    }
                var mesh = new Mesh { name = "Tail stripe " + k };
                mesh.SetVertices(verts); mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals(); mesh.RecalculateBounds();
                var go = new GameObject("Tail_stripe_" + (char)('A' + k));
                go.transform.SetParent(tail, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = gold;
            }
        }

        private static Material GetMat(Dictionary<string, Material> cache, Shader shader, string name)
        {
            if (cache.TryGetValue(name, out var m)) return m;
            Color c; float smooth = 0.3f;
            switch (name)
            {
                case "black": c = new Color(0.086f, 0.090f, 0.106f); break;
                case "white": c = new Color(0.961f, 0.961f, 0.949f); break;
                case "wing": c = new Color(0.855f, 0.863f, 0.871f); break;
                case "eye": c = new Color(0.027f, 0.039f, 0.055f); smooth = 0.8f; break;
                default: c = new Color(1f, 0.710f, 0.071f); break; // gold
            }
            m = new Material(shader) { name = name };
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            cache[name] = m;
            return m;
        }
    }
}
