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
