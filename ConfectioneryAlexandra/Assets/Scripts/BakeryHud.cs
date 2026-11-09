using UnityEngine;

namespace AlexandraBakery
{
    public sealed partial class BakeryGame
    {
        readonly Color ink=new Color(.13f,.22f,.21f),paper=new Color(.99f,.966f,.90f,.98f),accent=new Color(.14f,.38f,.34f),pink=new Color(.63f,.25f,.32f);
        GUIStyle textStyle;Vector2 scroll;
        void Tile(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Text(Rect r,string s,int size,Color c,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft){
            if(string.IsNullOrEmpty(s))return;
            textStyle??=new GUIStyle(GUI.skin.label);
            textStyle.font=interfaceFont;textStyle.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            textStyle.alignment=anchor;textStyle.wordWrap=true;textStyle.clipping=TextClipping.Clip;
            textStyle.padding.left=textStyle.padding.right=textStyle.padding.top=textStyle.padding.bottom=0;
            textStyle.normal.textColor=c;
            var inner=new Rect(r.x+3,r.y+1,Mathf.Max(1,r.width-6),Mathf.Max(1,r.height-2));
            var content=new GUIContent(s);
            FitText(textStyle,content,inner.size,size);
            GUI.Label(inner,content,textStyle);
        }
        public static void FitText(GUIStyle style,GUIContent content,Vector2 bounds,int preferred){
            style.fontSize=preferred;
            while(style.fontSize>10){
                style.CalcMinMaxWidth(content,out float minimum,out _);
                if(minimum<=bounds.x && style.CalcHeight(content,bounds.x)<=bounds.y)break;
                style.fontSize--;
            }
        }
        bool Button(Rect r,string label){bool over=r.Contains(Event.current.mousePosition);Tile(r,over?ink:accent);Text(r,label,16,Color.white,true,TextAnchor.MiddleCenter);return GUI.Button(r,GUIContent.none,GUIStyle.none);}
        string Status(){
            if(Stage==Step.Bake)return "Духовка  "+temperature+" °C     "+Mathf.FloorToInt(elapsed/SecondsPerBakeMinute)+" / "+dial+" мин";
            if(Stage==Step.Load)return "Панель духовки  "+temperature+" °C   ·   "+dial+" мин     Рецепт  "+Recipe.temperatureC+" °C   ·   "+Recipe.bakeMinutes+" мин";
            if(heldArt&&handPortion!=null)return "В руке   "+Props.Name(handPortion)+"  "+handCount+" "+Wanted(handPortion)?.unit;
            if(portionTool&&Held)return portionTool.Display+"   "+doseCount+" "+Wanted(Held.id)?.unit;
            if(Held)return "В руке   "+Held.Display;
            return "";}
        string Verb(BakeryTarget t){
            if(t.purpose==Purpose.Door){var door=t.hinge?t:objects[t.id+"-frame"];return door.opened?"E  закрыть":"E  открыть";}
            if(t.id=="tap")return "E  вымыть посуду";if(t.id=="book")return "E  читать";
            if(t.id=="oven")return Stage==Step.Bake?"E  достать":"E  поставить форму";
            if(t.id=="plate"||t.id=="dessert")return Stage==Step.Decor?"E  украсить":Stage==Step.Wrap?"E  взять   ·   ПКМ  украсить ещё":"E  собрать";
            if(t.id=="scale"||t.id=="cup"&&t.slot=="cupmat"&&Held)return "ЛКМ  добавить   ·   E  взять порцию";
            if(t.id=="large"||t.id=="small")return Stage==Step.Dispatch?"E  передать":"E  упаковать";
            if(t.id=="mat"||t.id=="rack"||t.id=="cupmat"||t.id=="sink"||t.id=="heat")return "E  поставить";
            if(t.id=="bowl"&&t.slot=="mat"&&Held||Props.IsForm(t.id)&&t.slot=="rack"&&Held)return "ЛКМ  добавить";
            return "E  взять";}
        void OnGUI(){
            if(Recipe==null)return;var matrix=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1080f,Screen.height/690f);scale=Mathf.Max(scale,.12f);
            float w=Screen.width/scale,h=Screen.height/scale;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            if(paused||bookOpen||Stage==Step.Pick||Stage==Step.Report){Modal(w,h);GUI.matrix=matrix;return;}
            if(painting){
                Tile(new Rect(20,20,350,149),paper);Text(new Rect(38,34,324,27),Recipe.decoration,22,ink,true);
                Text(new Rect(38,70,312,55),"ЛКМ — рисовать. Покрыто участков: "+sectors.Count+" из "+RequiredSectors+".",16,accent);
                Text(new Rect(38,116,310,45),"1  Крем     2  Декор заказа\nКисть: "+(useCream?"крем":Recipe.decoration),15,ink);
                if(Button(new Rect(w-217,h-68,195,44),"Готово   ↵"))EndPainting();
            }else{
                float cw=360;Tile(new Rect(18,18,cw,147),paper);Tile(new Rect(18,18,5,147),pink);
                Text(new Rect(37,31,cw-30,21),"ДЕНЬ "+Day+"    /    "+Today.customer.ToUpperInvariant(),13,accent,true);
                Text(new Rect(37,57,cw-33,55),Day==1?Today.title:Recipe.name,22,ink,true);
                Text(new Rect(37,111,cw-34,49),Day==1?"Соберите ягодный торт и передайте его маме.":Recipe.instruction,16,ink);
                string live=Status();if(live!=""){float ww=Stage==Step.Load?536:360;Tile(new Rect(w-ww-18,18,ww,56),ink);Text(new Rect(w-ww-6,24,ww-24,44),live,16,Color.white,true,TextAnchor.MiddleCenter);}
                var goal=Goal();if(goal&&goal.gameObject.activeInHierarchy&&goal!=aim){
                    Vector3 pos=eyes.WorldToViewportPoint(goal.transform.position+Vector3.up*.24f);
                    if(pos.z>.15f&&pos.x>.08f&&pos.x<.92f&&pos.y>.19f&&pos.y<.82f){
                        float px=pos.x*w,py=(1-pos.y)*h;Tile(new Rect(px-5,py-5,10,10),new Color(1,.79f,.35f));
                        if((goal.transform.position-eyes.transform.position).sqrMagnitude>7){Tile(new Rect(px-88,py-35,176,24),accent);Text(new Rect(px-85,py-35,170,24),goal.Display,13,Color.white,true,TextAnchor.MiddleCenter);}}
                }
                float cx=w/2,cy=h/2;
                Tile(new Rect(cx-3,cy-3,6,6),ink);Tile(new Rect(cx-2,cy-2,4,4),Color.white);
                if(aim){string detail=aim.Display+"   ·   "+Verb(aim);
                    if(Props.IsDish(aim.id))detail+="   ·   "+(aim.condition==Cleanliness.Clean?"чистая":aim.condition==Cleanliness.Used?"использованная":"грязная");
                    Tile(new Rect(cx-244,cy+20,488,36),ink);Text(new Rect(cx-241,cy+21,482,33),detail,16,Color.white,false,TextAnchor.MiddleCenter);}
                Tile(new Rect(18,h-103,w-36,85),paper);Text(new Rect(35,h-93,w-70,42),Guide(),18,ink,true);
                Text(new Rect(35,h-48,w-68,25),"WASD  ходить      E  взять / поставить      ЛКМ  использовать      G  вернуть      Tab  рецепт      Esc  пауза",14,accent);
                if(toastLeft>0){Tile(new Rect(w/2-340,h-151,680,38),accent);Text(new Rect(w/2-334,h-151,668,38),toast,15,Color.white,false,TextAnchor.MiddleCenter);}
            }GUI.matrix=matrix;
        }
        void Modal(float w,float h){
            Tile(new Rect(0,0,w,h),new Color(.07f,.13f,.12f,.75f));
            float x=(w-650)/2,y=(h-480)/2;Tile(new Rect(x,y,650,480),paper);Tile(new Rect(x,y,650,7),pink);
            string title=paused?"Перерыв":bookOpen?"Книга рецептов":Stage==Step.Pick?"Первый самостоятельный шаг":"Заказ передан";
            Text(new Rect(x+34,y+28,590,49),title,30,ink,true);
            if(paused){Text(new Rect(x+34,y+96,575,72),"Духовка и охлаждение на паузе.",19,ink);
                if(Button(new Rect(x+34,y+175,245,48),"Вернуться на кухню")){paused=false;SetCursor();}return;}
            if(bookOpen){var r=library[page];Text(new Rect(x+34,y+84,582,54),r.name+"   ·   "+r.difficulty+"/3",21,accent,true);
                var view=new Rect(x+34,y+145,582,222);scroll=GUI.BeginScrollView(view,scroll,new Rect(0,0,554,450));
                Text(new Rect(0,0,550,440),string.Join(", ",r.ingredients)+"\n\n"+string.Join(". ",r.steps),17,ink);GUI.EndScrollView();
                if(Button(new Rect(x+34,y+394,130,46),"← Назад")){page=(page+library.Length-1)%library.Length;scroll=Vector2.zero;}
                if(Button(new Rect(x+177,y+394,130,46),"Дальше →")){page=(page+1)%library.Length;scroll=Vector2.zero;}
                if(Button(new Rect(x+398,y+394,212,46),"Закрыть")){bookOpen=false;SetCursor();}return;}
            if(Stage==Step.Pick){Text(new Rect(x+34,y+90,582,42),"День 2. Выберите заказ Ирины.",19,ink);
                for(int i=0;i<menu.Length;i++)if(Button(new Rect(x+34,y+149+i*68,582,52),menu[i].name))SelectOrder(i);return;}
            Text(new Rect(x+34,y+89,582,44),Stars+" / 5     ·     "+points+" из 100",26,pink,true);
            string review=Day==1?"Мама: «Спасибо, родная! Особенно приятно, что ты сделала его сама».":
                Stars==5?"Ирина: «Всё по заказу. И как аккуратно украшено!»":"Ирина: «Спасибо за десерт. Есть пара пожеланий к следующему заказу».";
            Text(new Rect(x+34,y+145,582,62),review,18,ink);
            Text(new Rect(x+34,y+218,582,115),notes.Count==0?"Заказ и пожелания выполнены.":string.Join("  ·  ",notes),16,ink);
            Text(new Rect(x+34,y+341,582,40),"Опыт  +"+Stars*12+"   ("+xp+")       Репутация  "+(Stars>=2?"+":"")+(Stars-2)+"   ("+rep+")",17,accent,true);
            if(Button(new Rect(x+34,y+398,582,50),Day==1?"Начать день 2":"Приготовить другой заказ"))Next();
        }
    }
}
