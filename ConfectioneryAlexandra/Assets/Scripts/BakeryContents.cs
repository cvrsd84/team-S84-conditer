using UnityEngine;

namespace AlexandraBakery
{
    public sealed partial class BakeryGame
    {
        bool creamNeedsStir;
        GameObject panContents;
        static string IngredientModel(string id){
            switch(id){
                case "eggs":return "egg-yolk";
                case "milk":return "milk-surface";
                case "cream":return "cream-surface";
                case "cocoa":return "cocoa-surface";
                case "sugar":return "sugar-pile";
                case "crumb":return "crumb-pile";
                case "berries":return "berrydot";
                case "blueberry":return "bluedot";
                case "cheese":return "rosette";
                case "butter":return "butter-piece";
                case "chocolate":return "chocolate-chip";
                default:return "flour-pile";
            }
        }
        static bool IsPool(string id)=>id=="milk"||id=="cream"||id=="cocoa";
        static void DrawPortion(Transform root,string id,int quantity){
            bool pieces=id=="eggs"||id=="berries"||id=="blueberry"||id=="chocolate"||id=="butter"||id=="cheese";
            int count=id=="eggs"?Mathf.Clamp(quantity,0,12):pieces?Mathf.Clamp(Mathf.CeilToInt(quantity/28f),1,9):1;
            for(int i=root.childCount;i<count;i++)Studio.Model(root,IngredientModel(id),Vector3.zero);
            for(int i=0;i<root.childCount;i++){
                var part=root.GetChild(i);part.gameObject.SetActive(i<count);
                if(i>=count)continue;
                float radius=pieces&&count>1?.13f*Mathf.Sqrt((i+.5f)/count):0;
                part.localPosition=new Vector3(Mathf.Cos(i*2.40f)*radius,0,Mathf.Sin(i*2.40f)*radius);
                float factor=pieces?1:Mathf.Clamp(Mathf.Pow(quantity/180f,1f/3),.12f,1.05f);
                part.localScale=IsPool(id)?new Vector3(factor,Mathf.Clamp(quantity/150f,.20f,2f),factor):Vector3.one*factor;
            }
        }
        void AddToBowl(BakeryTarget bowl,Dose dose,int quantity){
            if(!liquid)liquid=Studio.Group(bowl.transform,"Содержимое миски",Vector3.zero);
            float level=.052f+.085f*Mathf.Clamp01(bowl.volume/900);
            // Raise the visible ingredients as a liquid bed fills the vessel.
            foreach(Transform part in liquid.transform){
                var p=part.localPosition;p.y=level+(part.name.StartsWith("Слой ")?0:.018f);part.localPosition=p;
            }
            int index=liquid.transform.childCount;
            bool pool=IsPool(dose.id);
            float offset=pool?0:.065f;
            var layer=Studio.Group(liquid.transform,(pool?"Слой ":"Порция ")+dose.id,
                new Vector3(Mathf.Cos(index*2.4f)*offset,level+(pool?.002f*index:.018f),Mathf.Sin(index*2.4f)*offset));
            DrawPortion(layer.transform,dose.id,quantity);
            layer.transform.localScale=Vector3.one*(pool?1:.75f);
        }
        void SetMixture(BakeryTarget bowl,string model){
            if(liquid){liquid.SetActive(false);liquid.transform.SetParent(null,false);Destroy(liquid);}
            liquid=Studio.Group(bowl.transform,"Содержимое миски",Vector3.zero);
            Studio.Model(liquid.transform,model,Vector3.up*(.06f+.12f*Mathf.Clamp01(bowl.volume/750)));
        }
        string BatterModel()=>amounts.ContainsKey("base/cocoa")||amounts.ContainsKey("melt/chocolate")?"cocoa-surface":"fill";
        void FinishCream(BakeryTarget bowl){
            creamNeedsStir=true;SetMixture(bowl,"cocoa-surface");Notify("Шоколад растопился. Размешайте ганаш венчиком.");
        }
        void CompleteCream(BakeryTarget bowl){
            SetMixture(bowl,Recipe.finishGroup=="ganache"?"cocoa-surface":"cream-surface");
            creamNeedsStir=false;bowl.fill="icing";creamReady=true;SetStep(Step.Assemble);
            Notify("Крем готов. Перенесите корж, затем миску с кремом на подставку.");
        }
    }
}
