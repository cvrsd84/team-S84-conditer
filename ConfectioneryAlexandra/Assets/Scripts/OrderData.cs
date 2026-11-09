using System;
using UnityEngine;
namespace AlexandraBakery
{
    [Serializable] public class Dose { public string id,unit,group; public int amount; public bool key; }
    [Serializable] public class Formula {
        public string id,name,category,form,box,decoration,instruction,assembly,model,topping,layout;
        public float topHeight;
        public string[] steps,ingredients,equipment;
        public Dose[] doses;
        public int difficulty,bakeMinutes,coolMinutes,temperatureC; public bool fullyPlayable,critical;
    }
    [Serializable] public class DayPlan { public int number,orders,minimumXP; public string title,customer; public bool hints; }
    [Serializable] public class Library { public Formula[] recipes; public DayPlan[] days; public int gameSecondsPerSecond; }
    public enum Step { Tutorial, Pick, Measure, Process, Form, Pour, Load, Bake, Cool, Assemble, Decor, Wrap, Dispatch, Report }
}
