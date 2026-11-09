using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlexandraBakery
{
    // Each prop is a single coloured mesh. Profiles may turn inward to model lips and cavities.
    public sealed class AtelierMesh
    {
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<Color32> colors = new List<Color32>();
        readonly List<int> triangles = new List<int>();
        public const string Mint="7CB4A1", Deep="275C57", Milk="FFF0D8", Wood="C88356", Dark="49343A", Rose="DC6479", Gold="DBAC61", Metal="A8C5BF", Blue="6799AB", Plum="584061", Berry="BA3558", Cocoa="734539";
        public void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,string hex)
        {
            ColorUtility.TryParseHtmlString("#"+hex,out Color color);
            int n=vertices.Count;
            vertices.Add(a);vertices.Add(b);vertices.Add(c);vertices.Add(d);
            for(int j=0;j<4;j++) colors.Add(color);
            triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);
            triangles.Add(n);triangles.Add(n+2);triangles.Add(n+3);
        }
        public void Box(float x,float y,float z,float w,float h,float d,string col,float bevel=.035f)
        {
            float b=Mathf.Min(bevel,Mathf.Min(w,Mathf.Min(h,d))*.24f);
            float[] ys={y,y+b,y+h-b,y+h};
            Vector3[] prev=null;
            for(int j=0;j<4;j++)
            {
                float cut=(j==0||j==3)?b*.55f:0, a=w/2-cut,c=d/2-cut,k=Mathf.Max(.0001f,b-cut);
                Vector3[] ring={new Vector3(-a+k,0,c),new Vector3(a-k,0,c),new Vector3(a,0,c-k),new Vector3(a,0,-c+k),new Vector3(a-k,0,-c),new Vector3(-a+k,0,-c),new Vector3(-a,0,-c+k),new Vector3(-a,0,c-k)};
                for(int i=0;i<8;i++) ring[i]+=new Vector3(x,ys[j],z);
                if(prev!=null) for(int i=0;i<8;i++) Face(prev[i],prev[(i+1)%8],ring[(i+1)%8],ring[i],col);
                if(j==0||j==3) for(int i=1;i<7;i++) {if(j==3) Face(ring[0],ring[i],ring[i+1],ring[i+1],col);else Face(ring[0],ring[i+1],ring[i],ring[i],col);}
                prev=ring;
            }
        }
        public void Turn(float x,float y,float z,float[] radius,float[] height,string col,int sides=24,float depth=1)
        {
            for(int j=1;j<height.Length;j++) for(int i=0;i<sides;i++){
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                Vector3 o=new Vector3(x,y,z);
                Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)*depth),q=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)*depth);
                Face(o+p*radius[j-1]+Vector3.up*height[j-1],o+p*radius[j]+Vector3.up*height[j],o+q*radius[j]+Vector3.up*height[j],o+q*radius[j-1]+Vector3.up*height[j-1],col);
            }
        }
        public void Disc(float x,float y,float z,float r,float h,string c,int n=24){Turn(x,y,z,new[]{0,r,r,0},new[]{0,0,h,h},c,n);}
        public void Fruit(float x,float y,float z,float r,float h,string c,int n=12){
            Turn(x,y,z,new[]{0,r*.78f,r,r*.78f,0},new[]{0,h*.2f,h*.53f,h*.82f,h},c,n);}
        public void Rod(Vector3 a,Vector3 b,float radius,string c){
            Vector3 axis=(b-a).normalized,normal=Vector3.Cross(axis,Vector3.right);
            if(normal.sqrMagnitude<.01f) normal=Vector3.Cross(axis,Vector3.up);
            normal=normal.normalized*radius;Vector3 side=Vector3.Cross(axis,normal);
            for(int i=0;i<8;i++){float f=i*Mathf.PI/4,g=(i+1)*Mathf.PI/4;
                Vector3 p=normal*Mathf.Cos(f)+side*Mathf.Sin(f),q=normal*Mathf.Cos(g)+side*Mathf.Sin(g);
                Face(a+p,a+q,b+q,b+p,c);}
        }
        public Mesh Finish(string id){
            var m=new Mesh{name=id,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            m.SetVertices(vertices);m.SetColors(colors);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
    }
}

