using System;
using System.Linq;
using UnityEngine;

namespace AlexandraBakery
{
    public sealed partial class BakeryGame
    {
        Dose Wanted(string id)=>Recipe.doses.FirstOrDefault(d=>d.id==id&&d.group==group&&!amounts.ContainsKey(d.id));
        Dose NextDose()=>Recipe.doses.FirstOrDefault(d=>d.group==group&&!amounts.ContainsKey(d.id));
        static int Increment(Dose d,bool coarse)=>d.unit=="шт"?1:coarse?25:d.amount<=10?1:5;
        public void Adjust(BakeryTarget t,int ticks,bool coarse){
            if(t.id=="oven"&&Stage==Step.Load){if(coarse)dial=Mathf.Clamp(dial+ticks,1,90);else temperature=Mathf.Clamp(temperature+ticks*5,80,240);return;}
            if(!Held||Stage!=Step.Measure||handPortion!=null)return;
            var dose=Wanted(Held.id);if(dose!=null&&CanMeasure(t,dose)){portionTool=t;doseCount=Mathf.Clamp(doseCount+ticks*Increment(dose,coarse),0,2000);ShowMeasure(t,dose);}}
        bool CanMeasure(BakeryTarget t,Dose d)=>d.unit=="мл"?t.id=="cup"&&t.slot=="cupmat":t.id=="scale";
        void ShowMeasure(BakeryTarget device,Dose d){
            if(portionArt)Destroy(portionArt);if(doseCount<=0)return;
            var p=device.transform.position+Vector3.up*(device.id=="scale"?.145f:.055f+.24f*Mathf.Clamp01(doseCount/300f));
            portionArt=Studio.Model(studio,"fill",p);float size=device.id=="cup"?.49f:.20f+.55f*Mathf.Clamp01(doseCount/(float)d.amount);
            portionArt.transform.localScale=new Vector3(size,.6f,size);Beep(1.1f);}
        void Measure(BakeryTarget device,bool click){
            if(Stage!=Step.Measure){Notify(Guide());return;}
            if(handPortion!=null){Notify("Порция готова. ЛКМ по миске на коврике.");return;}
            var d=Wanted(Held.id);
            if(d==null){Notify("Этот продукт сейчас не нужен. G — вернуть на полку.");return;}
            if(!CanMeasure(device,d)){Notify(d.unit=="мл"?"Молоко отмеряют в стакане на подставке.":"Этот продукт отмеряют на весах.");return;}
            if(click){Adjust(device,1,Input.GetKey(KeyCode.LeftShift));return;}
            if(doseCount<=0){Notify("ЛКМ или колесо — отмерить. E — взять порцию.");return;}
            handPortion=d.id;handCount=doseCount;RefreshHand();doseCount=0;portionTool=null;if(portionArt)Destroy(portionArt);Notify("Отмерено "+handCount+" "+d.unit+". ЛКМ по миске слева.");Beep();}
        void BowlAction(BakeryTarget bowl){
            if(Stage==Step.Process){
                if(group=="melt"){Notify(Guide());return;}
                bool valid=group=="crumb"?Held.id=="spatula":Held.id=="whisk"||Held.id=="mixer";
                if(!valid){Notify(Guide());return;}
                hygiene &= Held.condition!=Cleanliness.Dirty;Held.condition=Cleanliness.Dirty;AnimateHand();var tool=Held;Release(false);Home(tool);
                if(group=="crumb"){group="base";SetStep(Step.Measure);return;}
                if(group=="cream"){bowl.fill="icing";creamReady=true;SetStep(Step.Assemble);Notify("Крем готов. Возьмите остывшую форму и перенесите на подставку.");return;}
                bowl.fill="mixed";SetStep(occupied.ContainsKey("rack")?Step.Pour:Step.Form);return;
            }
            if(Stage!=Step.Measure||handPortion==null){Notify("Сначала отмерьте продукт, затем возьмите порцию клавишей E.");return;}
            var d=Wanted(handPortion);if(d==null)return;
            float error=Mathf.Abs(handCount-d.amount)/(float)d.amount;
            if(error>.150001f&&!d.key){Notify("Порция испорчена. Отмерьте заново "+Props.Name(d.id)+" — "+d.amount+" "+d.unit+".");Consume();Beep(.65f);return;}
            if(error>.150001f)wrongTaste=Props.Name(d.id);
            hygiene &= bowl.condition!=Cleanliness.Dirty;if(bowl.condition==Cleanliness.Clean)bowl.condition=Cleanliness.Used;
            bowl.fill=group;bowl.volume+=handCount*(d.unit=="шт"?50:1);amounts[d.id]=handCount;
            Consume();Beep();if(liquid)Destroy(liquid);
            liquid=Studio.Model(bowl.transform,"fill",Vector3.up*(.060f+.125f*Mathf.Clamp01(bowl.volume/750f)));
            float size=.48f+.50f*Mathf.Clamp01(bowl.volume/750);liquid.transform.localScale=new Vector3(size,1,size);
            Notify(error>.150001f?"Вкусовой дефект учтён. Заказ можно закончить.":"Добавлено: "+Props.Name(d.id)+".");
            if(NextDose()==null)SetStep(Step.Process);
        }
        void Pour(BakeryTarget pan){
            if(Stage!=Step.Pour||Held.fill!="mixed"){Notify(Guide());return;}
            form=pan;hygiene &= pan.condition!=Cleanliness.Dirty;pan.condition=Cleanliness.Used;pan.fill="batter";pan.volume=Held.volume;
            float capacity=pan.id=="square"?850:pan.id=="round"?720:pan.id=="muffinpan"?820:690;
            overflow=pan.volume>capacity;var bowl=Held;bowl.fill="";bowl.volume=0;bowl.condition=Cleanliness.Dirty;Release(false);Home(bowl);
            if(liquid)Destroy(liquid);liquid=Studio.Model(pan.transform,overflow?"spill":"fill",Vector3.up*.065f);
            if(Recipe.bakeMinutes==0){SetStep(Step.Assemble);Notify("Пирожные сформованы. Перенесите противень на подставку.");}
            else {SetStep(Step.Load);Notify(overflow?"Тесто перелилось через край. Отнесите форму в духовку.":"Возьмите форму и подойдите к духовке.");}
        }
        bool alarm;
        void OvenAction(BakeryTarget oven){
            if(Stage==Step.Load&&form&&Held==form){
                var pan=Held;Place(pan,"oven",oven.transform.position+new Vector3(0,.58f,-.33f));pan.gameObject.SetActive(false);
                elapsed=0;alarm=false;SetStep(Step.Bake);Beep(.7f);return;}
            if(Stage==Step.Bake){
                float minutes=elapsed/SecondsPerBakeMinute;if(minutes<Recipe.bakeMinutes*.85f){Notify("Середина ещё сырая. Подождите сигнала.");return;}
                if(Recipe.critical&&minutes>Recipe.bakeMinutes*1.15f){Notify("Передержка. Начните заказ заново.");SetStep(Step.Pick);return;}
                ovenFault=Mathf.Abs(temperature-Recipe.temperatureC)>5||Mathf.Abs(minutes-Recipe.bakeMinutes)>Mathf.Max(2,Recipe.bakeMinutes*.15f)||dial!=Recipe.bakeMinutes;
                Place(form,"rack",Above("rack"));coolLeft=Recipe.coolMinutes*SecondsPerBakeMinute;SetStep(Step.Cool);Beep();return;}
            Notify(Guide());}
        string Guide(){
            switch(Stage){
            case Step.Tutorial:
                string[] needed={"sponge","icing","fruit"};
                return Held&&Held.id==needed[tutorial]?"Положите на белую подставку справа в конце острова (E или ЛКМ).":"Возьмите со стола: "+Props.Name(needed[tutorial])+" (E).";
            case Step.Measure:
                var d=NextDose();if(d==null)return "";
                if(!occupied.ContainsKey("mat"))return Held&&Held.id=="bowl"?"Поставьте миску на левый коврик острова (E).":"Откройте шкаф «Посуда», возьмите миску и поставьте на левый коврик.";
                if(d.unit=="мл"&&!occupied.ContainsKey("cupmat"))return "Поставьте мерный стакан на голубой коврик. Если рука занята продуктом, G вернёт его на полку.";
                if(handPortion!=null)return "ЛКМ по миске на коврике — добавить отмеренную порцию.";
                if(Held&&Wanted(Held.id)!=null)d=Wanted(Held.id);
                string place=d.unit=="мл"?"мерный стакан на голубой подставке":"весы на острове";
                return Props.Name(d.id)+" · "+d.amount+" "+d.unit+" · "+place+". ЛКМ/колесо — доза, E — взять порцию.";
            case Step.Process:return group=="melt"?"Возьмите миску и поставьте на конфорку над духовкой.":group=="crumb"?"Возьмите лопатку. ЛКМ по миске — измельчить бисквит.":"Возьмите чистый венчик или миксер. ЛКМ по миске — взбить.";
            case Step.Form:return "Возьмите форму из шкафа и поставьте на деревянную подставку. По рецепту: "+Props.Name(Recipe.form)+".";
            case Step.Pour:return "Возьмите миску с тестом. ЛКМ по форме на деревянной подставке.";
            case Step.Load:return "Возьмите форму. У духовки колесо — температура, Shift + колесо — минуты. E — поставить.";
            case Step.Bake:return "Дождитесь нужного времени и заберите выпечку из духовки (E).";
            case Step.Cool:return "Остужается на подставке · "+Mathf.CeilToInt(Mathf.Max(0,coolLeft))+" с";
            case Step.Assemble:if(baseOnPlate)return "Возьмите миску с ягодным кремом и добавьте крем на корж (E).";return "Возьмите готовую форму и перенесите десерт на белую подставку (E).";
            case Step.Decor:return "E на белой подставке — украсить десерт.";
            case Step.Wrap:return Held?"Положите десерт в "+Props.Name(Day==1?"large":Recipe.box).ToLower()+" на отдельном столе справа.":"Возьмите украшенный десерт с подставки (E).";
            case Step.Dispatch:return "E на упакованном заказе — передать клиенту.";
            default:return "";}
        }
        BakeryTarget Goal(){
            string id=null;
            switch(Stage){
            case Step.Tutorial:id=Held?"plate":new[]{"sponge","icing","fruit"}[tutorial];break;
            case Step.Measure:
                if(handPortion!=null)id="bowl";else if(!occupied.ContainsKey("mat"))id=Held?"mat":"bowl";
                else{var d=Held?Wanted(Held.id):NextDose();if(d!=null)id=Held?(d.unit=="мл"?"cup":"scale"):d.id;
                    if(d!=null&&d.unit=="мл"&&!occupied.ContainsKey("cupmat"))id=Held&&Held.id=="cup"?"cupmat":"cup";}
                break;
            case Step.Process:id=group=="melt"?(Held?"heat":"bowl"):Held?"bowl":group=="crumb"?"spatula":"whisk";break;
            case Step.Form:id=Held?"rack":Recipe.form;break;
            case Step.Pour:id=Held?(occupied.TryGetValue("rack",out var p)?p.id:"rack"):"bowl";break;
            case Step.Load:id=Held?"oven":form.id;break;
            case Step.Bake:id="oven";break;case Step.Cool:id="rack";break;case Step.Assemble:id=Held?"plate":baseOnPlate?"bowl":form.id;break;
            case Step.Decor:id="plate";break;case Step.Wrap:id=Held?(Day==1?"large":Recipe.box):"dessert";break;case Step.Dispatch:id=boxed.id;break;
            }
            if(id==null||!objects.TryGetValue(id,out var t))return null;
            if(!string.IsNullOrEmpty(t.home)&&string.IsNullOrEmpty(t.slot)&&objects.TryGetValue(t.home+"-frame",out var door)&&!door.opened)return objects[t.home];
            return t;}
    }
}


