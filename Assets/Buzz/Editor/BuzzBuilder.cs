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
        var hips = Joint(root.transform,"Hips",new Vector3(0,1.73f,0));
        var body = Joint(hips,"Body",new Vector3(0,.23f,0));
        // Costume-style torso and hanging striped abdomen behind the legs.
        var tail=Joint(body,"Tail",new Vector3(0,-.20f,-.22f));
        Ball(tail,"Tail core",new Vector3(0,-.42f,-.12f),new Vector3(.86f,1.35f,.78f),black);
        TailBand(tail,"Tail gold upper",-.31f,.105f,gold);
        TailBand(tail,"Tail gold lower",-.79f,.095f,gold);
        Ball(tail,"Tail tip",new Vector3(0,-1.02f,-.12f),new Vector3(.24f,.22f,.25f),black);
        Ball(body,"Black jersey",new Vector3(0,.20f,0),new Vector3(1.05f,1.01f,.78f),black);
        // Front-facing interlocking GT lettering, built from persistent mesh strokes.
        Logo(body,white,gold);
        var head=Joint(body,"Head",new Vector3(0,.76f,.02f));
        Ball(head,"Yellow head",new Vector3(0,.31f,0),new Vector3(1.05f,1.30f,.99f),gold);
        Ball(head,"Tapered yellow face",new Vector3(0,.03f,.06f),new Vector3(.77f,.79f,.83f),gold);
        for(int eye=-1;eye<=1;eye+=2){
            var e=Ball(head,eye<0?"Left eye":"Right eye",new Vector3(eye*.275f,.49f,.405f),new Vector3(.31f,.43f,.19f),black);
            e.localRotation=Quaternion.Euler(0,eye*21f,eye*12f);
            Ball(head,eye<0?"Left eye glint":"Right eye glint",new Vector3(eye*.275f-.035f,.59f,.492f),new Vector3(.075f,.102f,.035f),white);
        }
        for(int s=-1;s<=1;s+=2) {
            string side=s<0?"Left":"Right";
            var ant=Joint(head,side+" Antenna",new Vector3(s*.29f,.76f,-.04f));
            Tube(ant,"Antenna",new Vector3[]{Vector3.zero,new Vector3(s*.17f,.17f,-.03f),new Vector3(s*.32f,.16f,-.05f),new Vector3(s*.43f,.02f,-.06f),new Vector3(s*.46f,-.12f,-.06f)},.038f,black);
            var wing=Joint(body,side+" Wing",new Vector3(s*.30f,.33f,-.34f));
            var a=Ball(wing,"Upper wing outline",new Vector3(s*.49f,.14f,-.07f),new Vector3(1.00f,.36f,.10f),black); a.localRotation=Quaternion.Euler(0,0,s*27);
            var b=Ball(wing,"Upper wing white",new Vector3(s*.49f,.14f,-.125f),new Vector3(.91f,.28f,.065f),white); b.localRotation=a.localRotation;
            // White on both sides for viewing from front or back.
            var c=Ball(wing,"Upper wing front",new Vector3(s*.49f,.14f,-.017f),new Vector3(.91f,.28f,.065f),white); c.localRotation=a.localRotation;
            a=Ball(wing,"Lower wing outline",new Vector3(s*.40f,-.13f,-.075f),new Vector3(.82f,.30f,.10f),black); a.localRotation=Quaternion.Euler(0,0,-s*13);
            b=Ball(wing,"Lower wing white",new Vector3(s*.40f,-.13f,-.13f),new Vector3(.73f,.23f,.065f),white); b.localRotation=a.localRotation;
            c=Ball(wing,"Lower wing front",new Vector3(s*.40f,-.13f,-.022f),new Vector3(.73f,.23f,.065f),white); c.localRotation=a.localRotation;
            wing.localScale=Vector3.one*.65f;
            var arm=Joint(body,side+" Shoulder",new Vector3(s*.43f,.30f,0));
            arm.localRotation=Quaternion.Euler(0,0,s*22f);
            Limb(arm,"Upper arm",new Vector3(0,-.02f,0),new Vector3(0,-.40f,0),.15f,gold);
            var elbow=Joint(arm,side+" Elbow",new Vector3(0,-.40f,0));
            Limb(elbow,"Forearm",Vector3.zero,new Vector3(0,-.35f,.02f),.115f,gold);
            Ball(elbow,"White cuff",new Vector3(0,-.27f,.018f),new Vector3(.25f,.16f,.25f),white);
            var hand=Joint(elbow,side+" Hand",new Vector3(0,-.38f,.02f));
            Ball(hand,"Palm",new Vector3(0,-.10f,0),new Vector3(.26f,.29f,.20f),white);
            for(int i=0;i<4;i++) Ball(hand,"Finger "+i,new Vector3((i-1.5f)*.062f,-.22f,.005f),new Vector3(.09f,.20f,.12f),white);
            Ball(hand,"Thumb",new Vector3(-s*.13f,-.07f,.035f),new Vector3(.12f,.21f,.13f),white).localRotation=Quaternion.Euler(0,0,-s*35);
            var leg=Joint(hips,side+" Hip",new Vector3(s*.27f,-.37f,0));
            leg.localRotation=Quaternion.Euler(0,0,s*8);
            Limb(leg,"Thigh",Vector3.zero,new Vector3(0,-.62f,0),.135f,black);
            var knee=Joint(leg,side+" Knee",new Vector3(0,-.62f,0));
            Limb(knee,"Shin",Vector3.zero,new Vector3(0,-.58f,0),.103f,black);
            var foot=Joint(knee,side+" Foot",new Vector3(0,-.58f,0));
            Ball(foot,"Shoe",new Vector3(0,-.07f,.15f),new Vector3(.33f,.25f,.55f),black);
            Ball(foot,"White sole",new Vector3(0,-.17f,.17f),new Vector3(.35f,.095f,.57f),white);
            Ball(foot,"White toe",new Vector3(0,-.075f,.36f),new Vector3(.31f,.17f,.22f),black);
            for(int i=0;i<2;i++) Ball(foot,"Lace "+i,new Vector3(0,.025f,.10f+i*.09f),new Vector3(.20f,.036f,.042f),white);
        }
        root.AddComponent<BuzzCharacterMotion>();
        string path=Folder+"/Buzz.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(root,path,InteractionMode.UserAction);
        Undo.RegisterCreatedObjectUndo(root,"Build Buzz"); Selection.activeGameObject=root;
        AssetDatabase.SaveAssets(); Debug.Log("Buzz prefab created at "+path+". Press Play to preview motion.");
    }
    static void Logo(Transform p, Material outline, Material fill){
        // Stylized block GT, gold fill with white outline, conforming to jersey front.
        Vector2[] g={new Vector2(.015f,.35f),new Vector2(-.16f,.35f),new Vector2(-.235f,.28f),new Vector2(-.235f,.13f),new Vector2(-.16f,.06f),new Vector2(.015f,.06f),new Vector2(.065f,.12f),new Vector2(.065f,.19f),new Vector2(-.045f,.19f)};
        Vector2[] top={new Vector2(.015f,.22f),new Vector2(.255f,.22f)};
        Vector2[] stem={new Vector2(.135f,.22f),new Vector2(.135f,-.055f)};
        LogoStroke(p,"G outline",g,.091f,outline,.012f);
        LogoStroke(p,"G gold",g,.056f,fill,.019f);
        LogoStroke(p,"T outline top",top,.091f,outline,.026f);
        LogoStroke(p,"T outline stem",stem,.091f,outline,.026f);
        LogoStroke(p,"T gold top",top,.056f,fill,.033f);
        LogoStroke(p,"T gold stem",stem,.056f,fill,.033f);
    }
    static float JerseyZ(float x,float y){return .39f*Mathf.Sqrt(Mathf.Max(.01f,1-x*x/(.525f*.525f)-(y-.20f)*(y-.20f)/(.505f*.505f)));}
    static void LogoStroke(Transform p,string name,Vector2[] pts,float width,Material mat,float offset){
        var v=new System.Collections.Generic.List<Vector3>(); var tr=new System.Collections.Generic.List<int>();
        for(int i=1;i<pts.Length;i++){
            Vector2 a=pts[i-1],b=pts[i];Vector2 d=(b-a).normalized;Vector2 n=new Vector2(-d.y,d.x)*width*.5f;
            a-=d*width*.5f;b+=d*width*.5f;
            int start=v.Count; int steps=12;
            for(int j=0;j<=steps;j++){Vector2 c=Vector2.Lerp(a,b,j/(float)steps);foreach(Vector2 q in new Vector2[]{c-n,c+n})v.Add(new Vector3(q.x,q.y,JerseyZ(q.x,q.y)+offset));
                if(j<steps){int k=start+j*2;tr.Add(k);tr.Add(k+2);tr.Add(k+1);tr.Add(k+1);tr.Add(k+2);tr.Add(k+3);}}
        }
        var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();MeshObject(p,name,mesh,mat);
    }
    static void TailBand(Transform p,string name,float y,float half,Material mat){
        int n=96, rows=16;var v=new Vector3[(n+1)*(rows+1)];var tr=new int[n*rows*6];
        for(int j=0;j<=rows;j++){float yy=y-half+2*half*j/rows;float f=Mathf.Sqrt(Mathf.Max(.001f,1-Mathf.Pow((yy+.42f)/.675f,2)));
            for(int i=0;i<=n;i++){float a=i*2*Mathf.PI/n;v[j*(n+1)+i]=new Vector3(.442f*f*Mathf.Cos(a),yy,-.12f+.402f*f*Mathf.Sin(a));}}
        int t=0;for(int j=0;j<rows;j++)for(int i=0;i<n;i++){int k=j*(n+1)+i;tr[t++]=k;tr[t++]=k+n+1;tr[t++]=k+1;tr[t++]=k+1;tr[t++]=k+n+1;tr[t++]=k+n+2;}
        var mesh=new Mesh();mesh.vertices=v;mesh.triangles=tr;mesh.RecalculateNormals();MeshObject(p,name,mesh,mat);
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
