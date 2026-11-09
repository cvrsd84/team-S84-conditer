using UnityEngine;
using static AlexandraBakery.AtelierMesh;

namespace AlexandraBakery
{
    public static class Studio
    {
        public static System.Func<string,AtelierMesh,Mesh> Bake;
        public static Material surface;
        public static GameObject Group(Transform p,string name,Vector3 pos){
            var g=new GameObject(name);g.transform.SetParent(p,false);g.transform.localPosition=pos;return g;}
        public static GameObject Model(Transform p,string id,Vector3 pos,AtelierMesh shape=null){
            var g=Group(p,id,pos);Mesh mesh=Resources.Load<Mesh>("Atelier/"+id);
            if(!mesh || Bake!=null && shape!=null){var geo=shape??Props.Make(id);mesh=Bake!=null?Bake(id,geo):geo.Finish(id);}
            g.AddComponent<MeshFilter>().sharedMesh=mesh;
            g.AddComponent<MeshRenderer>().sharedMaterial=surface?surface:Resources.Load<Material>("Atelier/Glaze");return g;}
        public static BoxCollider Solid(GameObject g,Vector3 center,Vector3 size){
            var c=g.AddComponent<BoxCollider>();c.center=center;c.size=size;return c;}
        public static BakeryTarget Target(Transform p,string id,Purpose kind,Vector3 pos,string home="",AtelierMesh geometry=null){
            var g=Model(p,id,pos,geometry);var t=g.AddComponent<BakeryTarget>();t.id=id;t.purpose=kind;t.origin=g.transform.position;t.home=home;
            var b=g.GetComponent<MeshFilter>().sharedMesh.bounds;
            Solid(g,b.center,new Vector3(Mathf.Max(b.size.x,.16f),Mathf.Max(b.size.y,.095f),Mathf.Max(b.size.z,.16f)));
            g.name=Props.Name(id);return t;}
        static void Label(Transform p,string value,float x,float y,float z,float size){
            var g=Group(p,value,new Vector3(x,y,z));var txt=g.AddComponent<TMPro.TextMeshPro>();
            txt.font=TMPro.TMP_Settings.defaultFontAsset;txt.text=value;txt.enableAutoSizing=true;txt.fontSizeMin=1.1f;txt.fontSizeMax=size*52f;txt.fontSize=txt.fontSizeMax;txt.alignment=TMPro.TextAlignmentOptions.Center;
            txt.rectTransform.sizeDelta=new Vector2(2.04f,.235f);txt.margin=new Vector4(.055f,.018f,.055f,.018f);txt.overflowMode=TMPro.TextOverflowModes.Truncate;txt.color=new Color(.99f,.94f,.81f);
            txt.textWrappingMode=TMPro.TextWrappingModes.NoWrap;}
        static void Counter(Transform p,string id,float x,float z,float width,float depth){
            var m=new AtelierMesh();
            m.Box(0,.10f,0,width-.14f,.85f,depth-.15f,Mint);m.Box(0,.93f,0,width,.12f,depth,Wood);
            for(int i=0;i<Mathf.RoundToInt(width/.9f);i++){float step=width/Mathf.RoundToInt(width/.9f),xx=-width/2+(i+.5f)*step;
                for(int sign=-1;sign<=1;sign+=2){m.Box(xx,.19f,sign*(depth/2-.055f),step-.12f,.63f,.035f,Deep);
                    m.Box(xx,.23f,sign*(depth/2-.075f),step-.19f,.55f,.082f,Mint);m.Box(xx,.71f,sign*(depth/2-.021f),.19f,.035f,.045f,Gold);}}
            var g=Model(p,id,new Vector3(x,0,z),m);Solid(g,new Vector3(0,.53f,0),new Vector3(width,1.05f,depth));}
        static void Cabinet(Transform p,string id,string title,float x,string[] ids,Purpose kind,string tone){
            const float width=2.35f;
            const float shelfWidth=2.16f, shelfDepth=.93f, shelfZ=.19f;
            var m=new AtelierMesh();
            m.Box(0,0,.22f,width,.15f,1.1f,Deep);m.Box(0,.15f,.70f,shelfWidth,2.21f,.1f,tone);
            for(int i=-1;i<=1;i+=2)m.Box(i*1.13f,.15f,.21f,.09f,2.21f,1.08f,tone);
            for(int i=0;i<4;i++)m.Box(0,.28f+i*.59f,shelfZ,shelfWidth,.07f,shelfDepth,i==3?tone:Wood);
            m.Box(0,2.36f,.15f,width,.27f,1.13f,Deep);
            var body=Model(p,id+"-shelves",new Vector3(x,0,4.5f),m);
            Solid(body,new Vector3(0,1.255f,.70f),new Vector3(shelfWidth,2.21f,.10f));
            for(int sign=-1;sign<=1;sign+=2)Solid(body,new Vector3(sign*1.13f,1.255f,.21f),new Vector3(.09f,2.21f,1.08f));
            for(int i=0;i<4;i++)Solid(body,new Vector3(0,.315f+i*.59f,shelfZ),new Vector3(shelfWidth,.07f,shelfDepth));
            Label(body.transform,title,0,2.49f,-.43f,.034f);
            var hinge=Group(body.transform,"Петля",new Vector3(-width/2,.12f,-.41f));var door=new AtelierMesh();
            door.Box(0,0,0,width-.045f,2.22f,.11f,tone);door.Box(0,.14f,-.065f,width-.27f,1.92f,.035f,id=="fridge"?Milk:Mint);
            door.Box(.91f,.78f,-.12f,.045f,.53f,.065f,Gold);if(id=="fridge"){door.Box(0,1.43f,-.10f,2.04f,.026f,.025f,Gold);for(int k=0;k<3;k++)door.Box(-.85f,.26f+k*.058f,-.097f,.22f,.018f,.015f,Deep);}
            var target=Target(hinge.transform,id,Purpose.Door,new Vector3(width/2,0,0),"",door);target.title=title;
            // The fixed frame owns the rotating hinge, so it can update even while the door swings.
            var drive=body.AddComponent<BakeryTarget>();drive.id=id+"-frame";drive.title=title;drive.purpose=Purpose.Door;drive.hinge=hinge.transform;
            
            for(int i=0;i<ids.Length;i++){
                float xx=(i%4-1.5f)*.535f;int row=i/4;
                var t=Target(body.transform,ids[i],kind,new Vector3(xx,.355f+row*.59f,-.06f),id);
                if(Props.IsForm(t.id)||t.id=="bowl")t.transform.localScale=Vector3.one*.79f;}
        }
        public static void Build(Transform p)
        {
            var shell=new AtelierMesh();shell.Box(0,-.60f,0,14,.60f,12,Wood);
            for(int i=0;i<30;i++){float z=-5.80f+i*.40f;
                for(int j=0;j<5;j++){string c=new[]{"D5A37A","DDB087","E5BA92","DDAA80"}[(i*7+j*3)%4];
                    shell.Box(-5.6f+j*2.8f,-.012f,z,2.793f,.025f,.396f,c,.003f);}}
            shell.Box(0,0,5.94f,14,4.5f,.16f,"F1DEC0");shell.Box(0,0,-5.94f,14,4.5f,.16f,"E8D7BA");
            for(int s=-1;s<=1;s+=2){shell.Box(s*6.94f,0,0,.16f,4.5f,12,"F4DDC0");shell.Box(s*6.83f,0,0,.09f,1.15f,12,Mint);shell.Box(s*6.77f,1.12f,0,.11f,.09f,12,Milk);
                for(int i=0;i<12;i++)shell.Box(s*6.76f,.08f,-5.5f+i,.055f,.99f,.04f,Deep);}
            shell.Box(0,0,5.81f,14,1.13f,.13f,Mint);shell.Box(0,1.13f,5.75f,14,.085f,.15f,Milk);
            for(int i=0;i<3;i++){float x=(i-1)*4.37f;shell.Box(x,2.75f,5.78f,3.8f,1.32f,.16f,Wood);
                shell.Box(x,2.84f,5.66f,3.56f,1.14f,.08f,"A8DAD8");
                for(int j=0;j<3;j++)shell.Fruit(x-1.23f+j*.94f,2.86f,5.59f,.32f,.27f,"82B9A0",12);
                shell.Box(x,2.80f,5.52f,.075f,1.23f,.085f,Milk);shell.Box(x,3.42f,5.53f,3.64f,.066f,.08f,Milk);
                shell.Box(x,2.72f,5.54f,3.90f,.085f,.4f,Wood);}
            shell.Box(0,4.48f,0,14,.18f,12,Milk);
            for(int i=-1;i<=1;i++){float x=i*3.65f;shell.Box(x,4.39f,0,.13f,.10f,12,Wood);
                shell.Disc(x,3.68f,.25f,.014f,.74f,Dark,8);
                shell.Turn(x,3.43f,.25f,new[]{0,.35f,.35f,.22f,.052f,0},new[]{0,0,.035f,.18f,.26f,.26f},i==0?Rose:Mint);
                shell.Disc(x,3.421f,.25f,.30f,.015f,"FFF3BD");}
            var room=Model(p,"studio-room",Vector3.zero,shell);
            Solid(room,new Vector3(0,-.29f,0),new Vector3(14,.60f,12));
            for(int i=-1;i<=1;i+=2){Solid(room,new Vector3(i*6.94f,2.25f,0),new Vector3(.20f,4.5f,12));
                Solid(room,new Vector3(0,2.25f,i*5.94f),new Vector3(14,4.5f,.20f));}
            Counter(p,"island",0,0,6.5f,1.64f);Counter(p,"dispatch",4.78f,-1.8f,2.45f,1.18f);
            Counter(p,"wash-counter",5.16f,4.5f,2.63f,1.25f);
            Cabinet(p,"fridge","Холодильник",-5.02f,Props.Cold,Purpose.Stock,Milk);
            Cabinet(p,"pantry","Бакалея",-2.54f,Props.Pantry,Purpose.Stock,Wood);
            Cabinet(p,"dishes","Посуда",-.06f,Props.Tools,Purpose.Utensil,Mint);
            var oven=new AtelierMesh();oven.Box(0,0,0,1.55f,1.46f,1.18f,Deep);oven.Box(0,.18f,-.59f,1.38f,.88f,.06f,Metal);
            oven.Box(0,.27f,-.635f,1.19f,.65f,.038f,Dark);oven.Box(0,.81f,-.685f,1.08f,.055f,.08f,Gold);
            for(int i=0;i<3;i++)oven.Box((i-1)*.38f,1.21f,-.62f,.16f,.12f,.06f,Milk);
            Target(p,"oven",Purpose.Station,new Vector3(2.55f,0,4.5f),"",oven);
            var stove=new AtelierMesh();stove.Disc(0,0,0,.30f,.05f,Dark);stove.Disc(0,.05f,0,.23f,.02f,Metal);
            Target(p,"heat",Purpose.Station,new Vector3(2.55f,1.46f,4.48f),"",stove);
            var wash=new AtelierMesh();wash.Turn(0,0,0,new[]{.47f,.47f,.40f,.32f,0},new[]{0,.12f,.12f,.025f,.025f},Metal,24,.72f);
            Target(p,"sink",Purpose.Station,new Vector3(5.1f,1.051f,4.33f),"",wash);
            var tap=new AtelierMesh();tap.Disc(0,0,0,.083f,.47f,Metal);tap.Rod(new Vector3(0,.43f,0),new Vector3(0,.45f,-.28f),.045f,Metal);tap.Disc(0,.35f,-.28f,.05f,.11f,Metal);
            Target(p,"tap",Purpose.Station,new Vector3(5.1f,1.05f,4.85f),"",tap);
            string[] stations={"mat","scale","cupmat","rack","plate"};
            for(int i=0;i<5;i++)Target(p,stations[i],Purpose.Station,new Vector3(-2.48f+i*1.23f,1.052f,-.34f));
            Target(p,"book",Purpose.Station,new Vector3(-2.55f,1.052f,.48f));
            string[] supplies={"sponge","icing","fruit"};for(int i=0;i<3;i++)Target(p,supplies[i],Purpose.Supply,new Vector3(-1.28f+i*1.02f,1.052f,.43f));
            Target(p,"large",Purpose.Station,new Vector3(4.17f,1.052f,-1.80f));
            Target(p,"small",Purpose.Station,new Vector3(5.35f,1.052f,-1.80f));
            var shelf=new AtelierMesh();shelf.Box(0,0,0,3.05f,.12f,.64f,Wood);
            var ledge=Model(p,"garnish-shelf",new Vector3(-3.7f,1.1f,-5.49f),shelf);
            Solid(ledge,new Vector3(0,.06f,0),new Vector3(3.05f,.12f,.64f));
            for(int i=0;i<Props.Extras.Length;i++){
                var decor=Target(p,Props.Extras[i],Purpose.Stock,new Vector3(-4.89f+i*.58f,1.22f,-5.47f));
                var hitbox=decor.GetComponent<BoxCollider>();
                float height=Mathf.Max(.32f,hitbox.size.y);
                hitbox.size=new Vector3(.58f,height,.49f);
                hitbox.center=Vector3.up*(height/2);
            }
            var light=Group(p,"Солнечный свет",Vector3.zero).AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.18f;light.color=new Color(1,.94f,.85f);
            light.transform.rotation=Quaternion.Euler(48,-28,0);light.shadows=LightShadows.Soft;
            // The ceiling is decorative. Let sunlight reach the work surfaces.
            room.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            StudioDetails.Build(p);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.75f,.73f,.70f);
        }
    }
}


