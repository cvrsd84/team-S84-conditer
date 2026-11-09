using UnityEngine;
using static AlexandraBakery.AtelierMesh;

namespace AlexandraBakery
{
    public static class StudioDetails
    {
        static void Plant(AtelierMesh m,float x,float y,float z,float s,bool blooms){
            m.Turn(x,y,z,new[]{0,.13f*s,.18f*s,.19f*s,.19f*s,0},new[]{0,0,.22f*s,.23f*s,.27f*s,.27f*s},"BB755A",14);
            for(int i=0;i<10;i++){float a=i*2.4f,spread=(i%3+1)*.064f*s;
                Vector3 start=new Vector3(x,y+.25f*s,z),end=start+new Vector3(Mathf.Cos(a)*spread,(.18f+.055f*(i%4))*s,Mathf.Sin(a)*spread);
                m.Rod(start,end,.012f*s,Deep);m.Fruit(end.x,end.y-.065f*s,end.z,.065f*s,.15f*s,i%2==0?"67896A":"8EA87A",8);
                if(blooms&&i%2==0)for(int j=0;j<5;j++)m.Fruit(end.x+Mathf.Cos(j*1.257f)*.036f*s,end.y+.055f*s,end.z+Mathf.Sin(j*1.257f)*.036f*s,.034f*s,.05f*s,i==0?Gold:"CB7986",7);}
        }
        static void Window(AtelierMesh m,float x,float z){
            m.Box(x,2.81f,z,3.48f,1.15f,.04f,"9ECBD3");
            // A miniature street seen through the panes.
            for(int i=0;i<5;i++){float xx=x-1.36f+i*.67f;float y=2.86f+(i%2)*.13f;
                m.Box(xx,2.84f,z-.035f,.55f,y-2.80f+.26f,.03f,i%2==0?"DFAD86":"E7C99F");
                m.Turn(xx,y+.22f,z-.04f,new[]{.39f,0},new[]{0,.29f},i%2==0?"A97867":"A18577",4,.15f);
                for(int j=0;j<2;j++)m.Box(xx-.13f+.26f*j,2.92f,z-.06f,.085f,.13f,.02f,"779E99");}
            for(int i=0;i<3;i++){float xx=x-1.42f+i*1.40f;m.Box(xx,2.79f,z-.09f,.07f,.28f,.035f,Wood);
                m.Fruit(xx,2.98f,z-.09f,.185f,.35f,i%2==0?"81A78D":"729882",11);}
            for(int i=0;i<15;i++){float ax=x-1.68f+i*.24f;string col=i%2==0?Milk:"C78082";
                m.Box(ax,3.75f,z-.12f,.24f,.24f,.19f,col,.009f);
                m.Fruit(ax,3.67f,z-.18f,.12f,.18f,col,12);}
            m.Box(x,3.95f,z-.14f,3.88f,.08f,.23f,Wood);
            m.Box(x,2.75f,z-.19f,.055f,.99f,.045f,Milk);m.Box(x,3.31f,z-.18f,3.51f,.045f,.06f,Milk);
        }
        public static void Build(Transform p){
            var m=new AtelierMesh();
            for(int i=-1;i<=1;i++)Window(m,i*4.37f,5.48f);
            for(int i=0;i<14;i++)for(int j=0;j<3;j++){
                float x=1.25f+i*.39f,y=1.18f+j*.37f;
                m.Box(x,y,5.71f,.377f,.355f,.045f,(i+j)%2==0?"F4E4D0":"EADBC9",.008f);
                if((i+j)%4==0)m.Fruit(x,y+.13f,5.66f,.043f,.085f,"A3C1B4",6);}
            Plant(m,5.95f,1.05f,4.65f,.94f,true);Plant(m,-.79f,2.64f,4.43f,.72f,false);
            Plant(m,6.1f,.014f,2.5f,2.5f,false);
            for(int j=0;j<4;j++){float x=3.76f+j*.39f;m.Box(x,2.15f,5.22f,.035f,.31f,.045f,Gold);
                if(j%2==0)m.Fruit(x,1.99f,5.22f,.088f,.19f,Wood);else m.Box(x,1.96f,5.22f,.135f,.16f,.035f,Wood);}
            m.Box(4.40f,2.45f,5.29f,1.79f,.052f,.12f,Gold);
            // Loose striped linen drapes over the front edge of the island.
            m.Box(-2.92f,1.054f,-.35f,.37f,.009f,.94f,Milk);
            m.Box(-2.92f,.63f,-.826f,.37f,.43f,.018f,Milk);
            for(int i=0;i<3;i++){m.Box(-3.05f+i*.045f,1.062f,-.35f,.018f,.005f,.94f,"BD7780");
                m.Box(-3.05f+i*.045f,.63f,-.838f,.018f,.43f,.008f,"BD7780");}
            // Herbarium prints on the left wall, above the panel moulding.
            for(int i=0;i<3;i++){float z=-2.8f+i*1.64f;m.Box(-6.79f,1.68f,z,.12f,1.12f,.82f,Wood);
                m.Box(-6.71f,1.73f,z,.035f,1.02f,.72f,Milk);
                m.Rod(new Vector3(-6.68f,1.85f,z-.12f),new Vector3(-6.68f,2.53f,z+.13f),.013f,Deep);
                for(int j=0;j<5;j++)m.Fruit(-6.65f,1.97f+j*.10f,z+(j%2==0?-.085f:.09f),.08f,.13f,i==0?Mint:i==1?Rose:Gold,8);}
            var trim=Studio.Model(p,"interior-details",Vector3.zero,m);trim.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
