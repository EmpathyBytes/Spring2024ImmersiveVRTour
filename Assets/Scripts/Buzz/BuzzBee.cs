using UnityEngine;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Builds Buzz as a simple original bee from Unity primitives (no model file needed).
    /// Put it on the same object as BuzzGuide. It hides the old statue named "Buzz#1" and
    /// creates a new "Buzz#1" so BuzzMotion, BuzzSummon and the speech bubble pick it up.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class BuzzBee : MonoBehaviour
    {
        [SerializeField] private float flapSpeed = 28f;
        [SerializeField] private float flapAngle = 28f;

        [Tooltip("Optional: a character prefab (front along +Z). If set, it is used instead of the primitive bee.")]
        [SerializeField] private GameObject characterPrefab;
        [Tooltip("Optional: an .obj file imported as a TextAsset. Built at runtime as separate animated parts (takes priority over characterPrefab).")]
        [SerializeField] private TextAsset mascotObj;
        [SerializeField] private float characterScale = 0.5f;
        [Tooltip("BuzzMotion lifts the body to this height, so the model is lowered by the same amount to stand on the floor.")]
        [SerializeField] private float characterFootOffset = 1f;

        private Transform wingL, wingR;

        private void Awake()
        {
            var old = GameObject.Find("Buzz#1");
            Vector3 startPos = Vector3.zero;
            if (old != null) { startPos = old.transform.position; old.SetActive(false); }

            // Root rotation matches the old statue so BuzzMotion's "front is -up" rule still works;
            // the model child cancels it so the bee's nose points along +Z in the world.
            var root = new GameObject("Buzz#1").transform;
            root.position = startPos;
            root.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var m = new GameObject("BeeModel").transform;
            m.SetParent(root, false);
            m.localRotation = Quaternion.Euler(90f, 0f, 0f);

            if (mascotObj != null)
            {
                m.position = root.position + Vector3.down * characterFootOffset;
                var mc = new GameObject("BuzzCharacter");
                mc.transform.SetParent(m, false);
                mc.transform.localPosition = Vector3.zero;
                mc.transform.localRotation = Quaternion.identity;
                mc.transform.localScale = Vector3.one * characterScale;
                BuzzMascotLoader.Build(mascotObj.text, mc.transform);
                mc.AddComponent<BuzzCharacterAnimator>().Build();
                return;
            }

            if (characterPrefab != null)
            {
                m.position = root.position + Vector3.down * characterFootOffset;
                var c = Instantiate(characterPrefab);
                c.transform.SetParent(m, false);
                c.name = "BuzzCharacter";
                c.transform.localPosition = Vector3.zero;
                c.transform.localRotation = Quaternion.identity;
                c.transform.localScale = Vector3.one * characterScale;
                ConvertToURP(c);
                c.AddComponent<BuzzCharacterAnimator>().Build();
                return;
            }

            Color yellow = new Color(1f, 0.78f, 0.1f);
            Color black = new Color(0.08f, 0.08f, 0.08f);
            Color white = Color.white;
            Color wing = new Color(0.8f, 0.92f, 1f, 0.55f);
            Color gold = new Color(0.7f, 0.55f, 0.1f);

            Part(m, "Body", PrimitiveType.Sphere, new Vector3(0, 0, -0.1f), new Vector3(0.7f, 0.65f, 0.95f), yellow);
            Part(m, "Stripe1", PrimitiveType.Sphere, new Vector3(0, 0, -0.15f), new Vector3(0.72f, 0.67f, 0.14f), black);
            Part(m, "Stripe2", PrimitiveType.Sphere, new Vector3(0, 0, -0.38f), new Vector3(0.66f, 0.6f, 0.14f), black);
            Part(m, "Stinger", PrimitiveType.Sphere, new Vector3(0, 0, -0.62f), new Vector3(0.12f, 0.12f, 0.2f), black);
            Part(m, "Head", PrimitiveType.Sphere, new Vector3(0, 0.08f, 0.45f), new Vector3(0.5f, 0.5f, 0.5f), yellow);
            Part(m, "EyeL", PrimitiveType.Sphere, new Vector3(-0.13f, 0.14f, 0.68f), new Vector3(0.18f, 0.22f, 0.12f), white);
            Part(m, "EyeR", PrimitiveType.Sphere, new Vector3(0.13f, 0.14f, 0.68f), new Vector3(0.18f, 0.22f, 0.12f), white);
            Part(m, "PupilL", PrimitiveType.Sphere, new Vector3(-0.13f, 0.14f, 0.745f), new Vector3(0.08f, 0.1f, 0.05f), black);
            Part(m, "PupilR", PrimitiveType.Sphere, new Vector3(0.13f, 0.14f, 0.745f), new Vector3(0.08f, 0.1f, 0.05f), black);
            Part(m, "Smile", PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.72f), new Vector3(0.16f, 0.05f, 0.05f), black);
            for (int s = -1; s <= 1; s += 2)
            {
                var a = Part(m, s < 0 ? "AntennaL" : "AntennaR", PrimitiveType.Cylinder,
                    new Vector3(0.1f * s, 0.42f, 0.55f), new Vector3(0.025f, 0.14f, 0.025f), black);
                a.localRotation = Quaternion.Euler(-20f, 0f, -25f * s);
                Part(m, "Tip", PrimitiveType.Sphere, new Vector3(0.2f * s, 0.55f, 0.5f), new Vector3(0.08f, 0.08f, 0.08f), gold);
            }
            wingL = Wing(m, "WingL", -1, wing);
            wingR = Wing(m, "WingR", 1, wing);
        }

        // Imported OBJ/FBX models bring Standard-shader materials, which render pink in URP.
        // Swap any non-URP material for a URP Lit one with the same colour.
        private static void ConvertToURP(GameObject go)
        {
            var urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp == null) return;
            var cache = new System.Collections.Generic.Dictionary<Material, Material>();
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null || m.shader == urp || m.shader.name.StartsWith("Universal")) continue;
                    if (!cache.TryGetValue(m, out var n))
                    {
                        Color col = m.HasProperty("_Color") ? m.color : Color.white;
                        n = new Material(urp) { name = m.name };
                        n.color = col;
                        n.SetColor("_BaseColor", col);
                        n.SetFloat("_Smoothness", m.name.ToLower().StartsWith("eye") ? 0.8f : 0.3f);
                        cache[m] = n;
                    }
                    mats[i] = n;
                }
                r.sharedMaterials = mats;
            }
        }

        private Transform Wing(Transform parent, string name, int side, Color c)
        {
            var pivot = new GameObject(name + "Pivot").transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = new Vector3(0.12f * side, 0.3f, -0.05f);
            Part(pivot, name, PrimitiveType.Sphere, new Vector3(0.3f * side, 0.04f, 0f), new Vector3(0.6f, 0.04f, 0.32f), c, true);
            return pivot;
        }

        private Transform Part(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color c, bool transparent = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            var mat = new Material(sh);
            mat.color = c;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go.transform;
        }

        private void Update()
        {
            if (wingL == null) return;
            float a = Mathf.Sin(Time.time * flapSpeed) * flapAngle;
            wingL.localRotation = Quaternion.Euler(0f, 0f, -a);
            wingR.localRotation = Quaternion.Euler(0f, 0f, a);
        }
    }
}
