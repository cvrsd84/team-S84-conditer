using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AlexandraBakery
{
    [Serializable] public class OrderResult
    {
        public string recipe,customer,feedback;
        public int score,stars,experience,reputation;
    }

    [Serializable] public class ShiftState
    {
        public int version=1,day,experience,reputation;
        public float seconds;
        public string[] orders,customers;
        public OrderResult[] results;
        public string screen,reason;
    }

    public sealed partial class BakeryGame
    {
        const string SaveKey="Alexandra.shift.v1";
        Library catalogueData;
        Formula[] orders;
        string[] customers;
        readonly List<OrderResult> results=new List<OrderResult>();
        float shiftSeconds,nextSave;
        string closeReason;
        bool initialized;
        public int Planned=>orders.Length;
        public int Served=>results.Count;
        public float ShiftLimit=>catalogueData.shiftHours*3600f/catalogueData.gameSecondsPerSecond;
        string Customer=>customers[Math.Min(Served,customers.Length-1)];
        bool IsEndScreen=>Stage==Step.Report||Stage==Step.Summary;
        public static Formula[] PlanShift(Formula[] pool,int[] levels,System.Random random){
            var chosen=new List<Formula>();
            foreach(int level in levels){
                var matches=pool.Where(r=>r.fullyPlayable&&r.difficulty==level).ToArray();
                var fresh=matches.Where(r=>!chosen.Contains(r)).ToArray();
                if(fresh.Length>0)matches=fresh;
                if(matches.Length==0)throw new InvalidOperationException("Нет рабочего рецепта сложности "+level);
                chosen.Add(matches[random.Next(matches.Length)]);
            }
            return chosen.ToArray();
        }
        void InitializeShift(Library book){
            catalogueData=book;orders=new[]{Recipe};customers=new[]{book.days[0].customer};initialized=true;
            if(!Application.isBatchMode){
                foreach(string key in new[]{SaveKey,SaveKey+".backup"}){
                    if(!PlayerPrefs.HasKey(key))continue;
                    if(RestoreCheckpoint(PlayerPrefs.GetString(key)))break;
                }
            }
        }
        public void Next(){
            if(Stage==Step.Report){BeginOrder(orders[Served]);Checkpoint();return;}
            if(Stage!=Step.Summary)return;
            Day++;var rng=new System.Random(UnityEngine.Random.Range(0,int.MaxValue));
            var pattern=Day==2?catalogueData.regularPatterns[0]:catalogueData.regularPatterns[rng.Next(catalogueData.regularPatterns.Length)];
            orders=PlanShift(menu,pattern.levels,rng);
            var names=catalogueData.customers.OrderBy(n=>rng.Next()).ToArray();
            customers=orders.Select((r,i)=>names[i%names.Length]).ToArray();
            results.Clear();summaryScroll=Vector2.zero;shiftSeconds=0;nextSave=30;closeReason="";
            BeginOrder(orders[0]);Checkpoint();
        }
        void RecordOrder(){
            var row=new OrderResult{recipe=Recipe.name,customer=Customer,score=points,stars=Stars,
                experience=Stars*12*Recipe.difficulty,reputation=Stars-2,feedback=string.Join(" · ",notes)};
            results.Add(row);xp+=row.experience;rep+=row.reputation;
            if(Served==Planned)CloseShift("Все заказы переданы");
            else {SetStep(Step.Report);Checkpoint();}
        }
        public void CloseShift(string reason){
            if(Stage==Step.Summary)return;
            paused=false;bookOpen=false;
            if(painting){painting=false;overhead.enabled=false;eyes.enabled=true;if(brushTool)Destroy(brushTool);}
            closeReason=reason;SetStep(Step.Summary);Checkpoint();
        }
        void TickShift(float dt){
            if(Day==1||IsEndScreen||paused||bookOpen)return;
            shiftSeconds=Mathf.Min(ShiftLimit,shiftSeconds+Mathf.Max(0,dt));
            if(shiftSeconds>=ShiftLimit){CloseShift("Рабочее время вышло");return;}
            if(shiftSeconds>=nextSave){nextSave=shiftSeconds+30;Checkpoint();}
        }
        public string CaptureCheckpoint(){
            return JsonUtility.ToJson(new ShiftState{day=Day,experience=xp,reputation=rep,seconds=shiftSeconds,
                orders=orders.Select(r=>r.id).ToArray(),customers=customers,results=results.ToArray(),reason=closeReason,
                screen=Stage==Step.Summary?"summary":Stage==Step.Report?"review":"work"});
        }
        void Checkpoint(){
            if(!initialized||Application.isBatchMode || Day==1&&Stage!=Step.Summary)return;
            string value=CaptureCheckpoint(),old=PlayerPrefs.GetString(SaveKey,"");
            if(value==old)return;
            if(old.Length>0)PlayerPrefs.SetString(SaveKey+".backup",old);
            PlayerPrefs.SetString(SaveKey,value);PlayerPrefs.Save();
        }
        public bool RestoreCheckpoint(string json){
            ShiftState s;
            try{s=JsonUtility.FromJson<ShiftState>(json);}catch(ArgumentException){return false;}
            if(s==null||s.version!=1||s.day<1||s.orders==null||s.orders.Length<1||s.orders.Length>3||
                s.results==null||s.customers==null||s.customers.Length!=s.orders.Length||s.results.Length>s.orders.Length||
                float.IsNaN(s.seconds)||s.seconds<0||s.seconds>ShiftLimit||
                (s.screen!="summary"&&s.screen!="review"&&s.screen!="work")||
                (s.day==1&&(s.orders.Length!=1||s.screen!="summary"))||
                (s.screen=="review"&&s.results.Length==0)||(s.screen!="summary"&&s.results.Length>=s.orders.Length))return false;
            var plan=s.orders.Select(id=>menu.FirstOrDefault(r=>r.id==id)).ToArray();
            if(plan.Any(r=>r==null)||s.results.Any(r=>r==null||r.stars<1||r.stars>5)||s.customers.Any(string.IsNullOrWhiteSpace))return false;
            Day=s.day;xp=s.experience;rep=s.reputation;shiftSeconds=s.seconds;nextSave=shiftSeconds+30;
            orders=plan;customers=s.customers;results.Clear();results.AddRange(s.results);closeReason=s.reason;
            int idx=s.screen=="review"?Served-1:Math.Min(Served,Planned-1);
            BeginOrder(plan[idx]);
            if(s.screen=="review"){var r=results.Last();points=r.score;Stars=r.stars;notes.Clear();if(!string.IsNullOrEmpty(r.feedback))notes.Add(r.feedback);SetStep(Step.Report);}
            if(s.screen=="summary")SetStep(Step.Summary);
            return true;
        }
        void OnApplicationQuit(){Checkpoint();}
        void OnApplicationPause(bool suspended){if(suspended)Checkpoint();}
    }
}
