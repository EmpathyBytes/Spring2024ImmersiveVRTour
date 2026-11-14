using UnityEngine;
using UnityEditor;
using System.IO;

public static class BuzzBuilder {
    static Material black, gold, white, darkGold;
    const string Folder = "Assets/Buzz/Generated";
    [MenuItem("Tools/Buzz/Build Character")]
    public static void Build() {
        Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
        black = Mat("Ink", new Color(.025f,.019f,.024f)); gold = Mat("Gold",new Color(1f,.67f,.025f));
        white = Mat("White",new Color(.98f,.98f,.94f)); darkGold = Mat("Deep Gold",new Color(.89f,.43f,.015f));
        var root = new GameObject("Buzz");
        var hips = Joint(root.transform,"Hips",new Vector3(0,1.23f,0));
        var body = Joint(hips,"Body",new Vector3(0,.23f,0));
        Ball(body,"Abdomen",new Vector3(0,-.26f,0),new Vector3(1.02f,1.08f,.83f),black);
        // Curved yellow bands follow the abdomen surface rather than floating above it.
        Band(body,"Upper gold stripe",-.13f,.13f,.516f,.423f,.54f,gold);
        Band(body,"Lower gold stripe",-.43f,.105f,.517f,.424f,.54f,gold);
        Ball(body,"Thorax",new Vector3(0,.28f,0),new Vector3(1.04f,.85f,.84f),black);
        Cone(body,"Stinger",new Vector3(0,-.73f,-.12f),new Vector3(.22f,.43f,.23f),gold,180);
        var head=Joint(body,"Head",new Vector3(0,.72f,.02f));
        Ball(head,"Head shell",new Vector3(0,.24f,0),new Vector3(1.19f,1.02f,1.04f),black);
        Ball(head,"Golden face",new Vector3(0,.12f,.30f),new Vector3(1.24f,.73f,.88f),gold);
        Ball(head,"Muzzle",new Vector3(0,.07f,.54f),new Vector3(1.04f,.52f,.62f),gold);
        Ball(head,"Grin",new Vector3(0,-.065f,.748f),new Vector3(.72f,.225f,.15f),black);
        for(int i=0;i<6;i++) { float x=(i-2.5f)*.104f; Ball(head,"Tooth "+i,new Vector3(x,-.045f+Mathf.Abs(x)*.12f,.812f-Mathf.Abs(x)*.11f),new Vector3(.098f,.101f,.065f),white); }
        Ball(head,"Oversized nose",new Vector3(0,.32f,.69f),new Vector3(.73f,.57f,.57f),black);
        Ball(head,"Nose highlight",new Vector3(-.17f,.49f,.904f),new Vector3(.15f,.09f,.035f),white);
        for(int s=-1;s<=1;s+=2) {
            string side=s<0?"Left":"Right";
            var ant=Joint(head,side+" Antenna",new Vector3(s*.23f,.62f,-.04f));
            Tube(ant,"Antenna",new Vector3[]{Vector3.zero,new Vector3(s*.10f,.26f,-.03f),new Vector3(s*.24f,.49f,-.06f),new Vector3(s*.42f,.70f,-.09f)},.038f,black);
            var wing=Joint(body,side+" Wing",new Vector3(s*.30f,.33f,-.34f));
            var a=Ball(wing,"Upper wing outline",new Vector3(s*.49f,.14f,-.07f),new Vector3(1.00f,.36f,.10f),black); a.localRotation=Quaternion.Euler(0,0,s*27);
            var b=Ball(wing,"Upper wing white",new Vector3(s*.49f,.14f,-.125f),new Vector3(.91f,.28f,.065f),white); b.localRotation=a.localRotation;
            // White on both sides for viewing from front or back.
            var c=Ball(wing,"Upper wing front",new Vector3(s*.49f,.14f,-.017f),new Vector3(.91f,.28f,.065f),white); c.localRotation=a.localRotation;
            a=Ball(wing,"Lower wing outline",new Vector3(s*.40f,-.13f,-.075f),new Vector3(.82f,.30f,.10f),black); a.localRotation=Quaternion.Euler(0,0,-s*13);
            b=Ball(wing,"Lower wing white",new Vector3(s*.40f,-.13f,-.13f),new Vector3(.73f,.23f,.065f),white); b.localRotation=a.localRotation;
            c=Ball(wing,"Lower wing front",new Vector3(s*.40f,-.13f,-.022f),new Vector3(.73f,.23f,.065f),white); c.localRotation=a.localRotation;
            var arm=Joint(body,side+" Shoulder",new Vector3(s*.43f,.30f,0));
            arm.localRotation=Quaternion.Euler(0,0,s*22f);
            Limb(arm,"Upper arm",new Vector3(0,-.02f,0),new Vector3(0,-.40f,0),.15f,gold);
            var elbow=Joint(arm,side+" Elbow",new Vector3(0,-.40f,0));
            Limb(elbow,"Forearm",Vector3.zero,new Vector3(0,-.35f,.02f),.115f,gold);
            Ball(elbow,"White cuff",new Vector3(0,-.27f,.018f),new Vector3(.25f,.16f,.25f),white);
            var hand=Joint(elbow,side+" Hand",new Vector3(0,-.38f,.02f));
            Ball(hand,"Palm",new Vector3(0,-.10f,0),new Vector3(.26f,.29f,.20f),gold);
            for(int i=0;i<3;i++) Ball(hand,"Finger "+i,new Vector3((i-1)*.075f,-.22f,.005f),new Vector3(.09f,.20f,.12f),gold);
            Ball(hand,"Thumb",new Vector3(-s*.13f,-.07f,.035f),new Vector3(.12f,.21f,.13f),gold).localRotation=Quaternion.Euler(0,0,-s*35);
            var leg=Joint(hips,side+" Hip",new Vector3(s*.27f,-.37f,0));
            leg.localRotation=Quaternion.Euler(0,0,s*8);
            Limb(leg,"Thigh",Vector3.zero,new Vector3(0,-.39f,0),.135f,black);
            var knee=Joint(leg,side+" Knee",new Vector3(0,-.39f,0));
            Limb(knee,"Shin",Vector3.zero,new Vector3(0,-.33f,0),.103f,black);
            var foot=Joint(knee,side+" Foot",new Vector3(0,-.33f,0));
            Ball(foot,"Shoe",new Vector3(0,-.07f,.15f),new Vector3(.33f,.25f,.55f),black);
            Ball(foot,"White sole",new Vector3(0,-.17f,.17f),new Vector3(.35f,.095f,.57f),white);
            Ball(foot,"White toe",new Vector3(0,-.075f,.36f),new Vector3(.31f,.17f,.22f),white);
            for(int i=0;i<2;i++) Ball(foot,"Lace "+i,new Vector3(0,.025f,.10f+i*.09f),new Vector3(.20f,.036f,.042f),white);
        }
        root.AddComponent<BuzzCharacterMotion>();
        string path=Folder+"/Buzz.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(root,path,InteractionMode.UserAction);
        Undo.RegisterCreatedObjectUndo(root,"Build Buzz"); Selection.activeGameObject=root;
        AssetDatabase.SaveAssets(); Debug.Log("Buzz prefab created at "+path+". Press Play to preview motion.");
    }
    static Material Mat(string name, Color c) {
        string p=Folder+"/"+name+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(p);
        if(!m){ var shader=Shader.Find("Universal Render Pipeline/Lit"); if(!shader || UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline==null) shader=Shader.Find("Standard"); m=new Material(shader); AssetDatabase.CreateAsset(m,p); }
        m.color=c; if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",.32f); EditorUtility.SetDirty(m); return m;
    }
    static Transform Joint(Transform p,string name,Vector3 pos) {var g=new GameObject(name);g.transform.SetParent(p,false);g.transform.localPosition=pos;return g.transform;}
    static Transform Ball(Transform p,string name,Vector3 pos,Vector3 scale,Material m) { var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform; }
    static void Limb(Transform p,string name,Vector3 a,Vector3 b,float r,Material m) {var t=Ball(p,name,(a+b)/2,new Vector3(r*2,(b-a).magnitude+r*1.3f,r*2),m);t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    static void Tube(Transform p,string name,Vector3[] pts,float r,Material m){for(int i=1;i<pts.Length;i++)Limb(p,name+" "+i,pts[i-1],pts[i],r*(1f-.12f*(i-1)),m);}
    static Mesh SaveMesh(Mesh mesh,string name){string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
    static Transform MeshObject(Transform p,string name,Mesh mesh,Material m){var t=Joint(p,name,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=SaveMesh(mesh,name);t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;return t;}
    static void Band(Transform p,string name,float y,float h,float rx,float rz,float ry,Material m){int n=64;var v=new Vector3[(n+1)*2];var tri=new int[n*6];for(int i=0;i<=n;i++){float a=i*2*Mathf.PI/n;for(int j=0;j<2;j++){float yy=y+(j==0?-h:h);float f=Mathf.Sqrt(Mathf.Max(.01f,1-Mathf.Pow((yy+.26f)/ry,2)));v[i*2+j]=new Vector3(rx*f*Mathf.Cos(a),yy,rz*f*Mathf.Sin(a));}if(i<n){int k=i*6,b=i*2;tri[k]=b;tri[k+1]=b+1;tri[k+2]=b+2;tri[k+3]=b+1;tri[k+4]=b+3;tri[k+5]=b+2;}}var mesh=new Mesh();mesh.vertices=v;mesh.triangles=tri;mesh.RecalculateNormals();MeshObject(p,name,mesh,m);}
    static void Cone(Transform p,string name,Vector3 pos,Vector3 scale,Material m,float rot){int n=32;var v=new Vector3[n+2];v[0]=new Vector3(0,.5f,0);v[1]=new Vector3(0,-.5f,0);for(int i=0;i<n;i++){float a=i*2*Mathf.PI/n;v[i+2]=new Vector3(.5f*Mathf.Cos(a),-.5f,.5f*Mathf.Sin(a));}var tr=new int[n*6];for(int i=0;i<n;i++){int a=i+2,b=(i+1)%n+2,k=i*6;tr[k]=0;tr[k+1]=b;tr[k+2]=a;tr[k+3]=1;tr[k+4]=a;tr[k+5]=b;}var mesh=new Mesh();mesh.vertices=v;mesh.triangles=tr;mesh.RecalculateNormals();var t=MeshObject(p,name,mesh,m);t.localPosition=pos;t.localScale=scale;t.localRotation=Quaternion.Euler(0,0,rot);}
}
