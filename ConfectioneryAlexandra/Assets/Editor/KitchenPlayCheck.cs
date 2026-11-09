using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlexandraBakery;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class KitchenPlayCheck
{
    const string Flag="Alexandra.PlayCheck";
    static IEnumerator route;
    static double deadline;
    static BakeryGame game;
    static CharacterController motor;
    static int interactions;
    static string problems;
    static KitchenPlayCheck(){EditorApplication.update+=Tick;}
    public static void Run(){
        EditorSceneManager.OpenScene(SceneGenerator.Kitchen);SessionState.SetBool(Flag,true);
        EditorApplication.EnterPlaymode();
    }
    static void Tick(){
        if(!SessionState.GetBool(Flag,false)||!EditorApplication.isPlaying||EditorApplication.isPaused)return;
        try{
            if(route==null){if(!BakeryGame.Current)return;game=BakeryGame.Current;motor=UnityEngine.Object.FindFirstObjectByType<CharacterController>();route=Scenario();deadline=EditorApplication.timeSinceStartup+180;
                Application.logMessageReceived+=Watch;}
            if(problems!=null)throw new Exception(problems);
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Перебор времени маршрута");
            if(route.MoveNext())return;
            Debug.Log("KITCHEN_QA_PASS interactions="+interactions);Finish(0);
        }catch(Exception e){Debug.LogError("KITCHEN_QA_FAIL "+e);Finish(1);}
    }
    static void Watch(string message,string stack,LogType kind){if(kind==LogType.Exception||kind==LogType.Error||kind==LogType.Assert)problems=message;}
    static void Finish(int code){SessionState.SetBool(Flag,false);Application.logMessageReceived-=Watch;EditorApplication.Exit(code);}
    static void Check(bool ok,string detail){if(!ok)throw new Exception(detail);}
    static void Stage(Step expected){Check(game.Stage==expected,"Этап "+game.Stage+", ожидается "+expected);}
    static void Move(Vector3 pos){motor.enabled=false;motor.transform.position=pos;motor.enabled=true;Physics.SyncTransforms();}
    static void Ground(){
        Move(FpsLook.StartPoint);for(int i=0;i<30;i++)motor.Move(Vector3.down*.08f);
        Check(motor.isGrounded&&motor.transform.position.y>-.12f,"Провал на старте");
        foreach(var d in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right}){
            Move(new Vector3(d.x*6.3f,.06f,d.z*5.5f));for(int i=0;i<24;i++)motor.Move((d+Vector3.down*.2f)*.17f);
            Check(Mathf.Abs(motor.transform.position.x)<6.9f&&Mathf.Abs(motor.transform.position.z)<5.94f&&motor.transform.position.y>-.12f,"Проход сквозь границу");}
        Move(FpsLook.StartPoint);}

    static readonly Vector2Int[] Directions={Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down};
    static Vector2Int Grid(Vector3 p)=>new Vector2Int(Mathf.RoundToInt(p.x/.4f),Mathf.RoundToInt(p.z/.4f));
    static Vector3 Feet(Vector2Int p)=>new Vector3(p.x*.4f,.06f,p.y*.4f);
    static bool Free(Vector3 p)=>!Physics.OverlapCapsule(p+Vector3.up*.32f,p+Vector3.up*1.48f,.29f).Any(c=>c!=motor);
    static void Walk(Vector3 destination){
        var start=Grid(motor.transform.position);var finish=Grid(destination);
        var parents=new Dictionary<Vector2Int,Vector2Int>{{start,start}};var queue=new Queue<Vector2Int>();queue.Enqueue(start);
        while(queue.Count>0&&!parents.ContainsKey(finish)){
            var current=queue.Dequeue();
            foreach(var d in Directions){
                var cell=current+d;if(Math.Abs(cell.x)>16||Math.Abs(cell.y)>14||parents.ContainsKey(cell))continue;
                if(!Free(Feet(cell))&&cell!=finish)continue;parents[cell]=current;queue.Enqueue(cell);}}
        Check(parents.ContainsKey(finish),"Нет пешего пути до "+destination);
        var path=new Stack<Vector3>();for(var k=finish;k!=start;k=parents[k])path.Push(Feet(k));
        while(path.Count>0)WalkSegment(path.Pop());
        WalkSegment(destination);
    }
    static void WalkSegment(Vector3 end){
        for(int i=0;i<15;i++){var delta=end-motor.transform.position;delta.y=0;if(delta.magnitude<.055f)return;
            motor.Move(Vector3.ClampMagnitude(delta,.10f)+Vector3.down*.016f);}
        var diff=end-motor.transform.position;diff.y=0;
        Check(diff.magnitude<.20f,"Коллайдер преграждает маршрут: "+end+" от "+motor.transform.position);
    }
    static BakeryTarget Accessible(string id){
        var t=game.objects[id];Check(t.gameObject.activeInHierarchy,"Предмет скрыт: "+id);Physics.SyncTransforms();
        var bounds=t.GetComponent<Collider>().bounds;var goal=bounds.center;
        foreach(float radius in new[]{1.15f,1.65f,2.15f,2.8f,.7f})
            for(int a=0;a<24;a++){
                float angle=a*Mathf.PI/12;var foot=new Vector3(goal.x+Mathf.Sin(angle)*radius,.06f,goal.z-Mathf.Cos(angle)*radius);
                if(Mathf.Abs(foot.x)>6.45f||Mathf.Abs(foot.z)>5.55f)continue;
                var hits=Physics.OverlapCapsule(foot+Vector3.up*.33f,foot+Vector3.up*1.48f,.275f);
                if(hits.Any(h=>h!=motor))continue;
                foreach(float level in new[]{0f,.3f,-.28f}){
                    Vector3 eye=foot+Vector3.up*1.59f,direction=goal+Vector3.up*bounds.size.y*level-eye;
                    if(game.Trace(new Ray(eye,direction.normalized))==t){
                        if(!Free(Feet(Grid(foot))))continue;
                        if(game.Day==1||game.Recipe.id=="vanilla")Walk(foot);else Move(foot);
                        var cam=Camera.main;Check(cam!=null,"Нет игровой камеры");cam.transform.rotation=Quaternion.LookRotation(goal+Vector3.up*bounds.size.y*level-cam.transform.position);
                        if(game.Trace(new Ray(cam.transform.position,cam.transform.forward))==t)return t;
                    }}
            }
        throw new Exception("Из прохода нельзя навестись на "+id+" ("+game.Stage+") "+t.transform.position);}
    static void Act(string id,bool click=false){var t=Accessible(id);game.Act(t,click);interactions++;}
    static void Open(string id){
        if(!game.objects[id+"-frame"].opened)Act(id);
        // Advance the door animation to its open pose before ray-testing shelf items.
        var frame=game.objects[id+"-frame"];frame.hinge.localRotation=Quaternion.Euler(0,102,0);Physics.SyncTransforms();}
    static void Paint(){
        if(game.Recipe.id=="muffin"||game.Recipe.id=="potato"){
            for(int i=0;i<6;i++)Check(game.Paint(new Vector2((i%3-1)*.205f,(i/3-.5f)*.22f)),"Кисть не касается изделия");}
        else for(int i=0;i<8;i++){float a=(i+.5f)*Mathf.PI/4;Check(game.Paint(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.175f),"Кисть не рисует");}
        Check(!game.Paint(Vector2.one),"Штрих за пределами десерта");game.EndPainting();Stage(Step.Wrap);}
    static void Box(string box){
        Act(box);Stage(Step.Wrap);Act("dessert");Act(box);Stage(Step.Dispatch);Act(box);Stage(Step.Report);}
    static void Feed(Dose d,int amount){
        Act(d.id);var device=d.unit=="мл"?"cup":"scale";var tool=Accessible(device);
        int increment=d.unit=="шт"||d.amount<=10?1:5;
        if(Props.IsDry(d.id)){
            Act(device,true);
            for(int i=0;i<30;i++)game.UpdateDryPour(tool,true,1f/60f,false);
            int partial=game.MeasuredAmount;
            Check(partial>0&&partial<amount,"Вес должен расти при удержании");
            game.UpdateDryPour(tool,false,.75f,false);
            Check(game.MeasuredAmount==partial,"Вес вырос после отпускания ЛКМ");
            Act(device,true);
            game.UpdateDryPour(null,true,.5f,false);
            game.UpdateDryPour(tool,true,.5f,false);
            Check(game.MeasuredAmount==partial,"Насыпание продолжилось после отвода прицела");
            Act(device,true);
            for(int i=0;game.MeasuredAmount<amount&&i<2000;i++)game.UpdateDryPour(tool,true,1f/90f,false);
            game.UpdateDryPour(tool,false,.4f,false);
            Check(game.MeasuredAmount==amount,"Неверный вес сыпучего "+d.id);
        }else game.Adjust(tool,amount/increment,false);
        Act(device);Act("bowl",true);}
    static void CheckTypography(){
        var style=new GUIStyle{font=game.interfaceFont,wordWrap=true,fontStyle=FontStyle.Bold};
        var recipes=JsonUtility.FromJson<Library>(game.catalogue.text).recipes;
        foreach(var size in new[]{new Vector2(960,540),new Vector2(1024,768),new Vector2(1280,720),new Vector2(1920,1080)}){
            float scale=Mathf.Min(size.x/1080,size.y/690);
            foreach(var r in recipes){
                var title=new GUIContent(r.name);var box=new Vector2((360-33-6)*scale,(55-2)*scale);
                BakeryGame.FitText(style,title,box,Mathf.RoundToInt(22*scale));
                Check(style.CalcHeight(title,box.x)<=box.y,"Название выходит за рамку: "+title.text+" · "+size);}
        }
        foreach(var label in game.GetComponentsInChildren<TMPro.TMP_Text>(true)){
            label.ForceMeshUpdate();Check(!label.isTextTruncated,"Обрезана надпись: "+label.text);}
        Debug.Log("KITCHEN_TYPE_PASS 28 titles, 4 viewports and cabinet signs");
    }
    static IEnumerator Scenario(){
        for(int i=0;i<8;i++)yield return null;
        CheckTypography();KitchenWindows.Capture(Camera.main,"Kuhnya");Ground();Move(Vector3.down*4);yield return null;yield return null;Check(motor.transform.position.y>-.5f,"Нет возврата на кухню");
        Check(game.objects.Count>50,"Сцена неполная");Stage(Step.Tutorial);
        Act("plate");Act("large");Stage(Step.Tutorial);
        foreach(var item in new[]{"sponge","icing","fruit"}){Act(item);Act("plate",true);}
        Paint();
        for(int i=0;i<Props.Extras.Length;i++){
            string id=Props.Extras[i];Act(id);Check(game.Held==game.objects[id],"Декор не берётся: "+id);
            Act("dessert");game.EndPainting();Check(game.Held==game.objects[id],"Декор пропал при отмене");
            Act("dessert");float a=i*Mathf.PI*2/Props.Extras.Length;
            Check(game.Paint(.083f*new Vector2(Mathf.Cos(a),Mathf.Sin(a))),"Декор не наносится: "+id);
            game.EndPainting();Stage(Step.Wrap);Check(!game.Held,"Декор остался в руке после нанесения");
        }
        Debug.Log("KITCHEN_SHELF_DECOR_PASS 5 decorations");
        foreach(string id in new[]{"fridge","pantry","dishes"}){
            var colliders=game.objects[id+"-frame"].GetComponents<BoxCollider>();
            foreach(var shelf in colliders.Where(c=>c.size.y<.1f)){
                float inner=colliders.Where(c=>c.center.x>0).Min(c=>c.center.x-c.size.x/2);
                Check(shelf.size.x/2<inner,"Торец полки пересекает боковину: "+id);
            }
        }
        Box("large");Check(game.Stars==5,"Штраф в день 1");Debug.Log("KITCHEN_QA_DAY1_PASS");
        for(int run=0;run<5;run++){
            game.Next();game.SelectOrder(run%4);yield return null;Stage(Step.Measure);
            Open("fridge");Open("pantry");Open("dishes");
            Act("bowl");Act("mat");Act("cup");Act("cupmat");
            Act("jam");Act("scale",true);Act("bowl",true);Stage(Step.Measure);
            // Return the rejected stock with the same action as keyboard G.
            game.SendMessage("ReturnHand");
            string previous="";
            foreach(var d in game.Recipe.doses){
                if(d.group=="cream")continue;
                if(d.group!=previous&&previous!=""){
                    Stage(Step.Process);
                    if(previous=="melt"){Act("bowl");Act("heat");}else{Act("spatula");Act("bowl",true);}
                    Stage(Step.Measure);}
                if(run==0&&d.id=="flour"){Feed(d,100);Stage(Step.Measure);float end=Time.realtimeSinceStartup+1.3f;while(Time.realtimeSinceStartup<end)yield return null;}
                Feed(d,run==4&&d.id=="sugar"?100:d.amount);previous=d.group;
            }
            Stage(Step.Process);Act("whisk");Act("bowl",true);Stage(Step.Form);
            string form=run==4?"square":game.Recipe.form;Act(form);Act("rack");Stage(Step.Pour);Act("bowl");Act(form,true);
            if(game.Recipe.bakeMinutes>0){Stage(Step.Load);Act(form);var oven=Accessible("oven");
                game.Adjust(oven,2,false);game.Adjust(oven,game.Recipe.bakeMinutes-20,true);Act("oven");Stage(Step.Bake);Act("oven");Stage(Step.Bake);
                game.AdvanceTime(game.Recipe.bakeMinutes*.5f);Act("oven");Stage(Step.Cool);game.AdvanceTime(game.Recipe.coolMinutes*.5f+.01f);}
            if(game.Recipe.doses.Any(d=>d.group=="cream")){
                if(run!=4){
                    Act("bowl");Act("sink");Act("tap");Act("bowl");Act("mat");
                    Act("whisk");Act("sink");Act("tap");Act("whisk");game.SendMessage("ReturnHand");}
                else{Act("bowl");Act("mat");}
                foreach(var d in game.Recipe.doses.Where(d=>d.group=="cream"))Feed(d,run==0&&d.id=="cream"?230:d.amount);
                Act("whisk");Act("bowl",true);}
            Stage(Step.Assemble);Act(form);Act("plate");if(game.Recipe.id=="vanilla"){Act("bowl");Act("plate");}Stage(Step.Decor);Act("plate");Paint();
            Box(run==4?"small":game.Recipe.box);
            Check(run==4?game.Stars<5:game.Stars==5,"Неожиданная оценка "+game.Stars+" за "+game.Recipe.id);
            Debug.Log("KITCHEN_DRY_POUR_PASS "+game.Recipe.id);Debug.Log("KITCHEN_QA_DAY2_PASS "+game.Recipe.id+" stars="+game.Stars);yield return null;
        }
    }
}

