using UnityEngine;
using static AlexandraBakery.AtelierMesh;

namespace AlexandraBakery
{
    public static class Props
    {
        public static readonly string[] Pantry={"flour","sugar","powder","cocoa","baking","vanilla","chocolate","crumb"};
        public static readonly string[] Cold={"milk","butter","eggs","cream","berries","blueberry","jam","cheese"};
        public static readonly string[] Tools={"bowl","cup","whisk","spatula","round","square","muffinpan","tray","mixer","sieve","bag"};
        public static readonly string[] Extras={"sprinkles","flower","stencil","cookie","figurine"};
        public static string Name(string id){
            switch(id){
            case "flour":return "Мука";case "sugar":return "Сахар";case "powder":return "Сахарная пудра";case "cocoa":return "Какао";
            case "baking":return "Разрыхлитель";case "vanilla":return "Ванилин";case "chocolate":return "Шоколад";case "crumb":return "Бисквитная крошка";
            case "milk":return "Молоко";case "butter":return "Сливочное масло";case "eggs":return "Яйца";case "cream":return "Сливки";case "berries":return "Ягоды";
            case "blueberry":return "Черника";case "jam":return "Джем";case "cheese":return "Творожный сыр";
            case "bowl":return "Миска";case "cup":return "Мерный стакан";case "whisk":return "Венчик";case "spatula":return "Лопатка";
            case "round":return "Круглая форма";case "square":return "Квадратная форма";case "muffinpan":return "Форма на 6 маффинов";case "tray":return "Противень";
            case "mixer":return "Миксер";case "sieve":return "Сито";case "bag":return "Кондитерский мешок";case "sprinkles":return "Посыпка";case "flower":return "Съедобные цветы";
            case "stencil":return "Трафарет";case "cookie":return "Печенье";case "figurine":return "Шоколадная фигурка";
            case "sponge":return "Готовый корж";case "icing":return "Ягодный крем";case "fruit":return "Ягоды для декора";case "large":return "Коробка для торта";
            case "small":return "Коробка для пирожных";case "plate":return "Подставка для сборки";case "scale":return "Весы";case "mat":return "Место для миски";
            case "rack":return "Подставка для формы";case "cupmat":return "Место для стакана";case "book":return "Книга рецептов";case "sink":return "Раковина";case "tap":return "Кран";
            case "oven":return "Духовка";case "dessert":return "Готовый десерт";case "heat":return "Конфорка";case "delivery":return "Выдача заказа";default:return id;}
        }
        public static bool IsForm(string id)=>id=="round"||id=="square"||id=="muffinpan"||id=="tray";
        public static bool IsDish(string id)=>id=="bowl"||id=="cup"||IsForm(id)||id=="whisk"||id=="mixer"||id=="spatula"||id=="sieve";
        public static AtelierMesh Make(string id)
        {
            var m=new AtelierMesh();
            switch(id){
            case "flour":case "sugar":case "powder":case "baking":case "vanilla":case "crumb":
                string tone=id=="flour"?Mint:id=="sugar"?Rose:id=="powder"?Plum:id=="vanilla"?Gold:id=="crumb"?Wood:Blue;
                float h=id=="vanilla"||id=="baking"?.31f:.48f;
                m.Box(0,0,0,.36f,h,.22f,Milk);m.Box(0,h-.015f,0,.38f,.045f,.13f,tone);m.Box(0,h*.22f,-.006f,.365f,h*.36f,.23f,tone);
                for(int i=0;i<3;i++)m.Box(-.07f+i*.067f,h*.3f,-.125f,.032f,.057f,.012f,Milk);break;
            case "cream":case "milk":
                string band=id=="milk"?Blue:Rose;
                m.Box(0,0,0,.27f,.37f,.23f,Milk);m.Box(0,.12f,0,.275f,.13f,.235f,band);
                m.Turn(0,.36f,0,new[]{.17f,.075f},new[]{0,.10f},band,4);m.Box(0,.455f,0,.24f,.026f,.045f,Milk);break;
            case "butter":m.Box(0,0,0,.40f,.12f,.26f,Gold);m.Box(-.10f,.004f,0,.16f,.125f,.264f,Milk);break;
            case "eggs":m.Box(0,0,0,.43f,.07f,.29f,Wood);
                for(int i=0;i<6;i++)m.Fruit((i%3-1)*.126f,.045f,(i/3-.5f)*.13f,.062f,.16f,i%2==0?Milk:"E9B792");break;
            case "berries":case "blueberry":case "fruit":
                m.Box(0,0,0,.39f,.11f,.27f,Mint);
                for(int i=0;i<10;i++){float x=(i%4-1.5f)*.087f,z=(i/4-1)*.083f;
                    m.Fruit(x,.075f,z,.055f,.104f,id=="blueberry"?Plum:Berry);if(id!="blueberry")m.Disc(x,.17f,z,.027f,.012f,Deep,5);}break;
            case "cocoa":case "jam":case "cheese":case "sprinkles":
                string jar=id=="jam"?Berry:id=="cocoa"?Cocoa:id=="icing"?Rose:id=="cheese"?Milk:Mint;
                m.Turn(0,0,0,new[]{0,.12f,.145f,.145f,.12f,0},new[]{0,0,.035f,.24f,.265f,.265f},jar);
                m.Disc(0,.09f,0,.148f,.065f,Milk);m.Disc(0,.264f,0,.145f,.035f,Gold);break;
            case "chocolate":m.Box(0,0,0,.42f,.045f,.27f,Cocoa);
                for(int i=0;i<6;i++)m.Box((i%3-1)*.13f,.04f,(i/3-.5f)*.13f,.12f,.045f,.12f,Dark);break;
            case "bowl":case "round":case "cup":case "sieve":
                float r=id=="cup"?.16f:.30f, bh=id=="cup"?.36f:id=="round"?.135f:.235f;
                m.Turn(0,0,0,new[]{0,r*.55f,r*.8f,r,r,r*.92f,r*.68f,0},new[]{0,0,bh*.3f,bh*.85f,bh,bh,bh*.22f,bh*.13f},id=="bowl"?Mint:id=="cup"?Blue:Metal);
                if(id=="cup"){for(int j=1;j<6;j++)m.Box(.01f,bh*j/7,-r*.95f,.07f,.012f,.04f,Milk);}
                if(id=="sieve")m.Rod(new Vector3(.20f,.14f,0),new Vector3(.47f,.18f,0),.034f,Wood);break;
            case "square":case "tray":case "muffinpan":
                m.Box(0,0,0,.61f,.04f,.43f,Metal);if(id=="muffinpan"){
                    for(int i=0;i<6;i++)m.Turn((i%3-1)*.185f,.035f,(i/3-.5f)*.19f,new[]{.085f,.08f,.055f,0},new[]{.04f,.05f,.005f,.005f},Deep,16);}
                else {for(int v=-1;v<=1;v+=2){m.Box(v*.295f,.03f,0,.025f,.09f,.43f,Metal);m.Box(0,.03f,v*.20f,.59f,.09f,.025f,Metal);}}break;
            case "whisk":case "spatula":
                m.Rod(new Vector3(-.24f,.035f,0),new Vector3(.05f,.045f,0),.035f,Wood);
                if(id=="spatula")m.Box(.14f,.008f,0,.20f,.055f,.15f,Mint);
                else for(int i=0;i<5;i++){float a=i*Mathf.PI/5;Vector3 p=new Vector3(.17f,.085f*Mathf.Sin(a),.085f*Mathf.Cos(a));
                    foreach(int sign in new[]{-1,1}){Vector3 mid=new Vector3(p.x,.10f+sign*p.y,sign*p.z);m.Rod(new Vector3(.04f,.05f,0),mid,.008f,Metal);m.Rod(mid,new Vector3(.29f,.095f,0),.008f,Metal);}}break;
            case "mixer":m.Box(0,.12f,0,.36f,.18f,.18f,Rose);m.Box(-.04f,.29f,0,.18f,.06f,.1f,Milk);for(int i=0;i<2;i++)m.Disc(.06f+i*.09f,0,0,.032f,.145f,Metal);break;
            case "bag":m.Turn(0,0,0,new[]{.02f,.04f,.13f,.105f,.04f,.07f,0},new[]{0,.06f,.33f,.38f,.40f,.45f,.45f},Milk,12);break;
            case "portion":m.Turn(0,0,0,new[]{0,.08f,.14f,.14f,.125f,0},new[]{0,0,.115f,.13f,.13f,.105f},Milk);m.Fruit(0,.096f,0,.109f,.052f,Gold);break;
            case "icing":
                m.Turn(0,0,0,new[]{0,.12f,.21f,.23f,.23f,.21f,.12f,0},new[]{0,0,.10f,.18f,.20f,.20f,.05f,.05f},Rose);
                m.Fruit(0,.105f,0,.204f,.13f,"F4C5BE");m.Fruit(.045f,.178f,0,.096f,.095f,Milk);
                m.Rod(new Vector3(.10f,.16f,0),new Vector3(.27f,.37f,.08f),.021f,Wood);break;
            case "sponge":
                m.Disc(0,0,0,.32f,.043f,Wood);m.Disc(0,.04f,0,.31f,.083f,"E4A25B");m.Disc(0,.12f,0,.32f,.024f,Wood);
                m.Fruit(0,.13f,0,.32f,.065f,"DEA05F",32);
                for(int i=0;i<18;i++){float a=i*2.399f,crumbRadius=.22f*Mathf.Sqrt((i+1)/18f);m.Disc(crumbRadius*Mathf.Cos(a),.189f,crumbRadius*Mathf.Sin(a),.007f,.005f,Wood,7);}break;
            case "rosette":
                m.Turn(0,0,0,new[]{0,.036f,.027f,.020f,0},new[]{0,.008f,.023f,.035f,.045f},Milk,9);break;
            case "cake":case "brownie":case "muffin":case "potato":
                if(id=="cake"){m.Disc(0,0,0,.33f,.13f,Wood);m.Disc(0,.12f,0,.335f,.07f,Rose);m.Disc(0,.19f,0,.33f,.09f,Gold);m.Disc(0,.275f,0,.337f,.06f,Milk);}
                if(id=="brownie"){m.Box(0,0,0,.60f,.16f,.45f,Cocoa);m.Box(0,.154f,0,.59f,.035f,.44f,Dark);}
                if(id=="muffin"||id=="potato")for(int i=0;i<6;i++){float x=(i%3-1)*.205f,z=(i/3-.5f)*.22f;
                    if(id=="muffin"){m.Turn(x,0,z,new[]{.06f,.093f},new[]{0,.095f},Rose,12);m.Fruit(x,.085f,z,.10f,.09f,Gold);}
                    else m.Turn(x,0,z,new[]{0,.08f,.087f,.065f,0},new[]{0,.028f,.075f,.12f,.138f},Cocoa,12,1.22f);}break;
            case "scale":m.Box(0,0,0,.60f,.10f,.48f,Deep);m.Disc(0,.10f,.03f,.21f,.04f,Metal);m.Box(0,.097f,-.174f,.195f,.018f,.073f,Mint);break;
            case "mat":case "rack":case "cupmat":m.Box(0,0,0,.70f,.035f,.59f,id=="rack"?Wood:id=="cupmat"?Blue:Mint);
                for(int i=0;i<5;i++)m.Box(0,.033f,-.22f+i*.11f,.64f,.007f,.012f,Milk);break;
            case "plate":m.Turn(0,0,0,new[]{0,.21f,.19f,.105f,.105f,.41f,.42f,0},new[]{0,0,.04f,.055f,.15f,.165f,.195f,.195f},Milk);break;
            case "large":case "small":case "parcel":
                float w=id=="small"?.60f:.8f;
                m.Box(0,0,0,w,.04f,.55f,Milk);for(int i=-1;i<=1;i+=2){m.Box(w*.49f*i,.025f,0,.03f,.20f,.55f,Rose);m.Box(0,.025f,.26f*i,w,.20f,.035f,Rose);}
                if(id=="parcel"){m.Box(0,.20f,0,w*.99f,.13f,.55f,Milk);m.Box(0,.33f,0,.085f,.014f,.55f,Rose);}
                else m.Box(0,.22f,.25f,w,.30f,.027f,Milk);break;
            case "book":m.Box(0,0,0,.48f,.040f,.32f,Deep);m.Box(0,.04f,0,.45f,.06f,.29f,Milk);m.Box(0,.10f,0,.48f,.025f,.32f,Deep);m.Box(-.13f,.124f,0,.035f,.008f,.32f,Gold);break;
            case "flower":for(int i=0;i<6;i++){float a=i*Mathf.PI/3;m.Fruit(Mathf.Cos(a)*.078f,.02f,Mathf.Sin(a)*.078f,.07f,.065f,Rose);}m.Fruit(0,.05f,0,.045f,.065f,Gold);break;
            case "cookie":m.Disc(0,0,0,.17f,.055f,Gold,16);for(int i=0;i<5;i++)m.Disc(Mathf.Sin(i*2)*.095f,.054f,Mathf.Cos(i*2)*.095f,.019f,.009f,Cocoa,7);break;
            case "figurine":m.Fruit(0,0,0,.10f,.22f,Cocoa);m.Fruit(0,.18f,0,.074f,.13f,Cocoa);m.Fruit(-.045f,.28f,0,.035f,.12f,Cocoa);m.Fruit(.045f,.28f,0,.035f,.12f,Cocoa);break;
            case "stencil":m.Turn(0,0,0,new[]{.16f,.16f,.08f,.08f},new[]{0,.024f,.024f,0},Metal,8);break;
            case "hand":m.Fruit(0,0,0,.075f,.19f,"C88869");for(int i=0;i<4;i++)m.Fruit(-.050f+i*.032f,.115f,-.041f,.024f,.083f,"D69877");m.Fruit(-.073f,.065f,-.025f,.030f,.085f,"D69877");m.Disc(0,-.06f,0,.077f,.085f,Mint);break;
            case "fill":m.Disc(0,0,0,.23f,.025f,Gold);break;
            case "spill":m.Fruit(0,0,0,.33f,.045f,Cocoa,15);break;
            case "berrydot":m.Fruit(0,0,0,.025f,.045f,Berry);break;
            case "bluedot":m.Fruit(0,0,0,.025f,.045f,Plum);break;
            case "dust":m.Disc(0,0,0,.029f,.004f,Milk,8);break;
            case "cocoadot":m.Disc(0,0,0,.03f,.006f,Dark,8);break;
            default:throw new System.ArgumentException("Неизвестная модель "+id);}
            return m;
        }
    }
}

