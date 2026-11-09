using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AlexandraBakery
{
    public sealed partial class BakeryGame : MonoBehaviour
    {
        public static BakeryGame Current;
        public TextAsset catalogue;
        public Font interfaceFont;
        public Transform studio;
        public Step Stage {get;private set;}
        public int Day {get;private set;}=1;
        public Formula Recipe {get;private set;}
        public bool IsGameplayInputBlocked=>paused||painting||bookOpen||Stage==Step.Report||Stage==Step.Pick;
        public readonly Dictionary<string,BakeryTarget> objects=new Dictionary<string,BakeryTarget>();
        public BakeryTarget Held {get;private set;}
        Camera eyes,overhead;GameObject heldArt,leftHand,actionTool,brushTool,selection,display,parcel,liquid,portionArt;
        AudioSource speaker; AudioClip actionSound;
        Formula[] menu; Formula[] library;DayPlan[] calendar;
        DayPlan Today=>calendar[Day-1];
        readonly Dictionary<string,BakeryTarget> occupied=new Dictionary<string,BakeryTarget>();
        readonly Dictionary<string,int> amounts=new Dictionary<string,int>();
        readonly List<string> notes=new List<string>(); readonly HashSet<int> sectors=new HashSet<int>();
        readonly HashSet<int> toppingSectors=new HashSet<int>();
        bool useCream; int[] sectorMarks=new int[8];
        readonly List<Vector2> strokes=new List<Vector2>();
        BakeryTarget aim,portionTool,form,boxed;
        int tutorial,temperature=180,dial=30,points,rep,xp;
        float elapsed,coolLeft,toastLeft,moveLeft,nextPaint;
        string group,toast,wrongTaste,handPortion,boxUsed;int doseCount,handCount;
        bool creamReady,baseOnPlate,paused,painting,bookOpen,hygiene,ovenFault,overflow;
        int page; Transform dessertAnchor;
        const float SecondsPerBakeMinute=.50f;
        public int Stars {get;private set;}
        public float Coverage=>sectors.Count/(float)RequiredSectors;
        public void GenerateScenePreview(){studio=Studio.Group(transform,"Мастерская Александры",Vector3.zero).transform;Studio.Build(studio);}
        void Awake(){
            Current=this;
            if(!studio)GenerateScenePreview();
            foreach(var t in studio.GetComponentsInChildren<BakeryTarget>(true))objects[t.id]=t;
            var book=JsonUtility.FromJson<Library>(catalogue.text);library=book.recipes;calendar=book.days;
            menu=library.Where(r=>r.fullyPlayable).ToArray();Recipe=menu[0];
            var player=Studio.Group(transform,"Александра",FpsLook.StartPoint);
            var cc=player.AddComponent<CharacterController>();cc.height=1.78f;cc.radius=.28f;cc.center=Vector3.up*.91f;cc.stepOffset=.22f;cc.skinWidth=.035f;
            eyes=Studio.Group(player.transform,"Взгляд",Vector3.up*1.59f).AddComponent<Camera>();eyes.tag="MainCamera";eyes.nearClipPlane=.035f;eyes.farClipPlane=60;eyes.fieldOfView=65;eyes.transform.localRotation=Quaternion.Euler(6,0,0);
            eyes.gameObject.AddComponent<AudioListener>();player.AddComponent<FpsLook>();
            overhead=Studio.Group(transform,"Вид сверху",Vector3.zero).AddComponent<Camera>();overhead.orthographic=true;overhead.nearClipPlane=.03f;overhead.enabled=false;
            speaker=player.AddComponent<AudioSource>();actionSound=AudioClip.Create("Фарфор",2400,1,24000,false);
            float[] wave=new float[2400];for(int i=0;i<wave.Length;i++){float t=i/24000f;wave[i]=.15f*Mathf.Exp(-t*48)*(Mathf.Sin(2*Mathf.PI*t*875)+.25f*Mathf.Sin(t*2*Mathf.PI*1620));}actionSound.SetData(wave,0);
            dessertAnchor=Studio.Group(studio,"Десерт на подставке",objects["plate"].transform.position+Vector3.up*.20f).transform;
            foreach(string station in new[]{"mat","scale","cupmat","rack"})objects[station].gameObject.SetActive(false);
            foreach(string item in new[]{"sponge","icing","fruit"}){var t=objects[item];var pos=t.transform.position;pos.z=-.34f;t.transform.position=pos;}
            Stage=Step.Tutorial;SetCursor();Physics.SyncTransforms();
        }
        void SetCursor(){Cursor.lockState=IsGameplayInputBlocked?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=IsGameplayInputBlocked;}
        void Notify(string message){toast=message;toastLeft=3.8f;}
        void Beep(float pitch=1){speaker.pitch=pitch;speaker.PlayOneShot(actionSound);}
        void SetStep(Step step){Stage=step;SetCursor();}
        public BakeryTarget Trace(Ray ray){
            if(!Physics.Raycast(ray,out var hit,3.65f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))return null;
            var t=hit.collider.GetComponentInParent<BakeryTarget>();
            if(t && t.home!="" && t.slot=="" && objects.TryGetValue(t.home+"-frame",out var door)&&!door.opened)return null;
            return t;
        }
        void Highlight(BakeryTarget target){
            if(aim==target)return;aim=target;if(selection)Destroy(selection);
            if(!target)return;var mesh=target.GetComponent<MeshFilter>();if(!mesh||target.hinge)return;
            selection=Studio.Group(target.transform,"Контур",Vector3.zero);selection.AddComponent<MeshFilter>().sharedMesh=mesh.sharedMesh;
            selection.AddComponent<MeshRenderer>().sharedMaterial=Resources.Load<Material>("Atelier/Rim");
        }
        void Update(){
            if(Input.GetKeyDown(KeyCode.Escape)){
                if(painting)EndPainting();else if(bookOpen){bookOpen=false;SetCursor();}else if(Stage!=Step.Pick&&Stage!=Step.Report){paused=!paused;SetCursor();}return;}
            if(paused||bookOpen)return;
            AdvanceTime(Time.deltaTime);
            if(painting){if(Input.GetKeyDown(KeyCode.Alpha1))useCream=true;if(Input.GetKeyDown(KeyCode.Alpha2))useCream=false;var pointer=overhead.ScreenPointToRay(Input.mousePosition);
                var surface=new Plane(Vector3.up,dessertAnchor.position+Vector3.up*(TopHeight()+.012f));
                if(brushTool&&surface.Raycast(pointer,out float tip))brushTool.transform.position=pointer.GetPoint(tip);
                if(Input.GetMouseButton(0)&&Time.time>=nextPaint){nextPaint=Time.time+.035f;Ray r=overhead.ScreenPointToRay(Input.mousePosition);
                var plane=new Plane(Vector3.up,dessertAnchor.position+Vector3.up*TopHeight());if(plane.Raycast(r,out float d)){var v=r.GetPoint(d)-dessertAnchor.position;Paint(new Vector2(v.x,v.z));}}
                if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.E))EndPainting();return;}
            if(IsGameplayInputBlocked){Highlight(null);return;}
            Highlight(Trace(new Ray(eyes.transform.position,eyes.transform.forward)));
            if(Input.GetKeyDown(KeyCode.Tab)){bookOpen=true;page=Array.IndexOf(library,Recipe);SetCursor();return;}
            if(Input.GetKeyDown(KeyCode.G)){ReturnHand();return;}
            if(!aim)return;
            if(Stage==Step.Wrap&&!Held&&(aim.id=="plate"||aim.id=="dessert")&&Input.GetMouseButtonDown(1)){SetStep(Step.Decor);StartPainting();return;}
            int wheel=Math.Sign(Input.mouseScrollDelta.y);
            if(wheel!=0)Adjust(aim,wheel,Input.GetKey(KeyCode.LeftShift));
            if(Input.GetMouseButtonDown(0))Act(aim,true);
            else if(Input.GetKeyDown(KeyCode.E))Act(aim,false);
        }
        public void AdvanceTime(float dt){
            toastLeft-=dt;moveLeft-=dt;if(leftHand){leftHand.SetActive(moveLeft>0);if(moveLeft>0)leftHand.transform.localRotation=Quaternion.Euler(48+10*Mathf.Sin(moveLeft*28),15,-14);}
            if(actionTool){if(moveLeft<=0)Destroy(actionTool);else actionTool.transform.localRotation=Quaternion.Euler(48,0,18*Mathf.Sin(moveLeft*29));}
            if(Stage==Step.Bake){elapsed+=dt;if(!alarm && elapsed>=dial*SecondsPerBakeMinute){alarm=true;Beep(1.7f);Notify("Прозвенел таймер духовки.");}}
            if(Stage==Step.Cool){coolLeft-=dt;if(coolLeft<=0){if(Recipe.doses.Any(d=>d.group=="cream")){group="cream";SetStep(Step.Measure);}else SetStep(Step.Assemble);}}
        }
        void AnimateHand(){
            if(!leftHand){leftHand=Studio.Model(eyes.transform,"hand",new Vector3(-.24f,-.28f,.47f));leftHand.transform.localRotation=Quaternion.Euler(50,15,-14);}
            leftHand.SetActive(true);moveLeft=.7f;
            if(Held && (Held.id=="whisk"||Held.id=="mixer"||Held.id=="spatula")){
                if(actionTool)Destroy(actionTool);actionTool=Studio.Model(eyes.transform,Held.id,new Vector3(-.12f,-.16f,.45f));actionTool.transform.localScale=Vector3.one*.60f;}
            Beep(1.23f);}
        void RefreshHand(){
            if(heldArt){heldArt.SetActive(false);Destroy(heldArt);}if(!Held)return;
            heldArt=Studio.Group(eyes.transform,"Правая рука",new Vector3(.24f,-.245f,.5f));
            var hand=Studio.Model(heldArt.transform,"hand",Vector3.zero);hand.transform.localRotation=Quaternion.Euler(38,-16,-18);
            string id=Held.id=="dessert"?DessertModel():Held.id;
            if(Held.id=="dessert"&&display){display.transform.SetParent(heldArt.transform,false);display.transform.localPosition=new Vector3(0,.105f,.09f);display.transform.localScale=Vector3.one*.51f;}
            else {var model=Studio.Model(heldArt.transform,handPortion!=null?"portion":id,new Vector3(0,.10f,.065f));model.transform.localScale=Vector3.one*.57f;}
        }
        void Take(BakeryTarget t){
            if(Held){Notify("Правая рука занята. G — вернуть предмет.");return;}
            if(t.slot!=""){occupied.Remove(t.slot);t.slot="";}
            Held=t;t.gameObject.SetActive(false);RefreshHand();Beep();
        }
        void Release(bool show){
            if(!Held)return;Held.gameObject.SetActive(show);Held=null;handPortion=null;handCount=0;RefreshHand();
        }
        void Home(BakeryTarget t){t.slot="";t.transform.position=t.origin;t.gameObject.SetActive(true);}
        void ReturnHand(){
            if(!Held)return;
            if(Held.id=="dessert"){display.transform.SetParent(dessertAnchor,false);display.transform.localPosition=Vector3.zero;display.transform.localScale=Vector3.one;Release(true);return;}
            if(Held==form&&Stage>=Step.Load){Notify("Поставьте заполненную форму на подставку или в духовку.");return;}
            if(Held.id=="bowl" && amounts.Count>0 && Stage<=Step.Pour){Notify("Поставьте миску на коврик слева.");return;}
            doseCount=0;portionTool=null;if(portionArt)Destroy(portionArt);var t=Held;Release(false);Home(t);Notify("Предмет возвращён на место.");
        }
        IEnumerator RestoreStock(BakeryTarget t){yield return new WaitForSeconds(1.15f);if(Held!=t)Home(t);}
        void Consume(){var t=Held;Release(false);if(t.purpose==Purpose.Stock)StartCoroutine(RestoreStock(t));}
        void Place(BakeryTarget t,string slot,Vector3 position){
            if(t.slot!="")occupied.Remove(t.slot);
            if(Held==t)Release(false);t.slot=slot;occupied[slot]=t;t.transform.position=position;t.gameObject.SetActive(true);Beep();
        }
        public void Act(BakeryTarget t,bool click){
            if(!t||!t.gameObject.activeInHierarchy||IsGameplayInputBlocked)return;
            string id=t.id;
            if(!string.IsNullOrEmpty(t.home)&&string.IsNullOrEmpty(t.slot)&&objects.TryGetValue(t.home+"-frame",out var gate)&&!gate.opened)return;
            if(t.purpose==Purpose.Door){if(!click){var door=t.hinge?t:objects[id+"-frame"];door.opened=!door.opened;Beep(.8f);}return;}
            if(id=="book"){bookOpen=true;page=Array.IndexOf(library,Recipe);SetCursor();return;}
            if(id=="tap"){
                if(occupied.TryGetValue("sink",out var dish)){dish.condition=Cleanliness.Clean;dish.fill="";dish.volume=0;AnimateHand();Notify("Посуда вымыта. E — забрать из раковины.");}
                else Notify("Сначала положите посуду в раковину.");return;}
            if(id=="sink"){
                if(Held&&Props.IsDish(Held.id)&&!occupied.ContainsKey("sink")&&string.IsNullOrEmpty(Held.fill)){Place(Held,"sink",t.transform.position+Vector3.up*.13f);}
                else Notify("В раковину можно поставить одну пустую посудину.");return;}
            if(id=="plate"||id=="dessert"){PlateAction();return;}
            if(id=="large"||id=="small"){BoxAction(t);return;}
            if(id=="oven"){OvenAction(t);return;}
            if(id=="heat"&&Stage==Step.Process&&group=="melt"){
                if(!Held||Held.id!="bowl"){Notify("Возьмите миску с шоколадом и маслом.");return;}
                Place(Held,"mat",Above("mat"));group="base";SetStep(Step.Measure);Notify("Шоколад и масло растоплены.");return;}
            if(id=="scale"||id=="cup"&&t.slot=="cupmat"){
                if(Held&&Held.purpose==Purpose.Stock){Measure(t,click);return;}
                if(id=="scale")return;
            }
            if(t.id=="bowl"&&t.slot=="mat"&&Held&&click){BowlAction(t);return;}
            if(Props.IsForm(id)&&t.slot=="rack"&&Held&&Held.id=="bowl"&&click){Pour(t);return;}
            if(id=="mat"||id=="rack"||id=="cupmat"){
                if(!Held){Notify("Возьмите "+(id=="mat"?"миску":id=="cupmat"?"мерный стакан":"форму")+" в шкафу «Посуда».");return;}
                bool fits=id=="mat"?Held.id=="bowl":id=="cupmat"?Held.id=="cup":Props.IsForm(Held.id);
                if(id=="rack"&&form&&form!=Held&&(Stage==Step.Bake||Stage==Step.Cool)){Notify("Здесь остынет форма из духовки.");return;}
                if(!fits||occupied.ContainsKey(id)){Notify("Этому предмету нужно другое свободное место.");return;}
                var item=Held;Place(item,id,Above(id));if(id=="rack"&&Stage==Step.Form)SetStep(Step.Pour);return;}
            if(!click&&!Held&&(t.purpose==Purpose.Stock||t.purpose==Purpose.Utensil||t.purpose==Purpose.Supply)){
                if(t==form&&(Stage==Step.Cool||Stage==Step.Bake)){Notify("Подождите, пока завершится этап.");return;}
                if(portionTool==t&&doseCount>0){Notify("Закончите дозирование, пока стакан стоит на столе.");return;}
                Take(t);return;}
            if(Held)Notify("Сейчас в руке "+Held.Display+". "+Guide());
        }
        Vector3 Above(string id)=>objects[id].transform.position+Vector3.up*.046f;
        string DessertModel()=>Recipe.model;
        void SetDisplay(string model,Transform parent){
            if(display)Destroy(display);display=Studio.Model(parent,model,Vector3.zero);
        }
        void PlateAction(){
            if(Stage==Step.Tutorial){
                string[] order={"sponge","icing","fruit"};
                if(!Held||Held.id!=order[tutorial]){Notify(Guide());return;}
                Consume();tutorial++;Beep();
                if(tutorial==1)SetDisplay("sponge",dessertAnchor);
                if(tutorial==2)SetDisplay("cake",dessertAnchor);
                if(tutorial==3){SetStep(Step.Decor);StartPainting();}return;}
            if(Stage==Step.Assemble){
                if(creamReady&&baseOnPlate){
                    if(!Held||Held.id!="bowl"||Held.fill!="icing"){Notify("Возьмите миску с ягодным кремом.");return;}
                    var icing=Held;icing.fill="";icing.volume=0;icing.condition=Cleanliness.Dirty;Release(false);Home(icing);if(liquid)Destroy(liquid);
                    SetDisplay("cake",dessertAnchor);SetStep(Step.Decor);Beep();return;}
                if(!Held||Held!=form){Notify("Возьмите форму с готовым десертом.");return;}
                var used=Held;used.fill="";used.volume=0;used.condition=Cleanliness.Dirty;Release(false);Home(used);
                if(creamReady){baseOnPlate=true;SetDisplay("sponge",dessertAnchor);Notify("Теперь перенесите крем из миски на корж.");}
                else{SetDisplay(DessertModel(),dessertAnchor);SetStep(Step.Decor);}Beep();return;}
            if(Stage==Step.Decor){if(Held){Notify("Освободите руку клавишей G.");return;}StartPainting();return;}
            if(Stage==Step.Wrap&&!Held){var t=objects["dessert"];Take(t);return;}Notify(Guide());
        }
        float TopHeight()=>Recipe.topHeight;
        void StartPainting(){
            painting=true;Highlight(null);overhead.transform.SetPositionAndRotation(dessertAnchor.position+Vector3.up*2.25f,Quaternion.Euler(90,0,0));
            brushTool=Studio.Group(studio,"Кисть кондитера",dessertAnchor.position);
            var brushHand=Studio.Model(brushTool.transform,"hand",new Vector3(-.13f,.12f,-.11f));brushHand.transform.localRotation=Quaternion.Euler(-40,0,-30);
            var nozzle=Studio.Model(brushTool.transform,"bag",Vector3.zero);nozzle.transform.localRotation=Quaternion.Euler(-28,0,32);nozzle.transform.localScale=Vector3.one*.75f;
            overhead.orthographicSize=.67f;eyes.enabled=false;overhead.enabled=true;SetCursor();
        }
        public bool Paint(Vector2 uv){
            if(!painting||!display||strokes.Count>=190)return false;
            float radius=uv.magnitude;int sector;
            if(Recipe.layout=="six"){
                int near=-1;float best=.10f;for(int i=0;i<6;i++){float d=Vector2.Distance(uv,new Vector2((i%3-1)*.205f,(i/3-.5f)*.22f));if(d<best){best=d;near=i;}}
                if(near<0)return false;sector=near;
            }else{
                if(Recipe.layout=="square"?Mathf.Abs(uv.x)>.29f||Mathf.Abs(uv.y)>.21f:radius>.321f)return false;
                sector=(Mathf.FloorToInt((Mathf.Atan2(uv.y,uv.x)+Mathf.PI)*4/Mathf.PI))%8;}
            foreach(var p in strokes)if(Vector2.Distance(p,uv)<.035f)return false;
            strokes.Add(uv);if(Recipe.layout=="six"||radius>.105f){sectors.Add(sector);sectorMarks[sector]++;if(!useCream)toppingSectors.Add(sector);}
            var mark=Studio.Model(display.transform,useCream?"rosette":Recipe.topping,new Vector3(uv.x,TopHeight(),uv.y));
            mark.transform.localRotation=Quaternion.Euler(0,strokes.Count*137,0);return true;
        }
        int RequiredSectors=>Recipe.layout=="six"?6:8;
        public void EndPainting(){
            if(!painting)return;painting=false;if(brushTool)Destroy(brushTool);overhead.enabled=false;eyes.enabled=true;
            if(strokes.Count>0){SetStep(Step.Wrap);
                if(!objects.ContainsKey("dessert")){var g=Studio.Group(studio,"Готовый десерт",dessertAnchor.position);var t=g.AddComponent<BakeryTarget>();t.id="dessert";t.purpose=Purpose.Finished;Studio.Solid(g,new Vector3(0,.10f,0),new Vector3(.66f,.22f,.58f));objects["dessert"]=t;}
                Notify("Возьмите десерт с подставки и отнесите к коробкам.");}
            SetCursor();
        }
        void BoxAction(BakeryTarget t){
            if(Stage==Step.Dispatch&&t==boxed){Evaluate();return;}
            if(Stage!=Step.Wrap||!Held||Held.id!="dessert"){Notify(Guide());return;}
            boxed=t;boxUsed=t.id;display.transform.SetParent(t.transform,false);display.transform.localPosition=Vector3.up*.044f;display.transform.localScale=Vector3.one*.77f;
            Release(false);t.GetComponent<MeshRenderer>().enabled=false;parcel=Studio.Model(t.transform,"parcel",Vector3.zero);
            if(t.id=="small")parcel.transform.localScale=new Vector3(.77f,1,1);
            SetStep(Step.Dispatch);Beep();Notify("Заказ упакован. E — передать клиенту.");}
        void Evaluate(){
            points=100;notes.Clear();
            if(Day==2){Deduct(wrongTaste!=null,18,"Отклонение дозировки: "+wrongTaste);Deduct(!hygiene,12,"Использована немытая посуда");
                Deduct(form.id!=Recipe.form,12,"Форма не соответствует рецепту");Deduct(overflow,9,"Тесто вышло за край");
                Deduct(ovenFault,18,"Нарушен режим духовки");Deduct(boxUsed!=Recipe.box,12,"Размер коробки не подходит");
                Deduct(toppingSectors.Count<RequiredSectors,10,"Особая просьба выполнена частично");Deduct(strokes.Count<RequiredSectors||sectors.Count<RequiredSectors/2 || sectorMarks.Max()>Mathf.Max(3,strokes.Count*.42f),9,"Неравномерный декор");}
            Stars=Mathf.Clamp(Mathf.CeilToInt(points/20f),1,5);xp+=Stars*12;rep+=Stars-2;
            SetStep(Step.Report);Beep(1.4f);}
        void Deduct(bool condition,int n,string reason){if(condition){points-=n;notes.Add(reason);}}
        public void Next(){
            if(Stage!=Step.Report)return;Day=2;SetStep(Step.Pick);}
        public void SelectOrder(int i){
            if(Stage!=Step.Pick||i<0||i>=menu.Length)return;
            StopAllCoroutines();Recipe=menu[i];if(display)Destroy(display);if(parcel)Destroy(parcel);if(liquid)Destroy(liquid);if(portionArt)Destroy(portionArt);
            if(objects.TryGetValue("dessert",out var dessert)){objects.Remove("dessert");Destroy(dessert.gameObject);}
            foreach(var t in objects.Values){if(t.purpose==Purpose.Utensil||t.purpose==Purpose.Stock){Home(t);t.condition=Cleanliness.Clean;t.fill="";t.volume=0;}
                if(t.purpose==Purpose.Supply)t.gameObject.SetActive(false);if(t.hinge)t.opened=false;}
            if(boxed)boxed.GetComponent<MeshRenderer>().enabled=true;boxed=null;Held=null;form=null;wrongTaste=null;baseOnPlate=false;creamReady=false;hygiene=true;overflow=false;ovenFault=false;
            foreach(string station in new[]{"mat","scale","cupmat","rack"})objects[station].gameObject.SetActive(true);
            amounts.Clear();occupied.Clear();sectors.Clear();toppingSectors.Clear();Array.Clear(sectorMarks,0,sectorMarks.Length);useCream=false;strokes.Clear();temperature=170;dial=20;elapsed=0;doseCount=0;handPortion=null;portionTool=null;
            group=Recipe.doses[0].group;SetStep(Step.Measure);toastLeft=0;Physics.SyncTransforms();}
    }
}



