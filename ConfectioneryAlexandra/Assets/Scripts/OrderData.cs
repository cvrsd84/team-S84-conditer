using System;
using UnityEngine;
namespace AlexandraBakery
{
    [Serializable] public class Dose { public string id,unit,group; public int amount; public bool key; }
    [Serializable] public class Formula {
        public string id,name,category,form,box,decoration,instruction,assembly,model,topping,layout;
        public float topHeight,formCapacity;
        public string finishGroup;
        public string[] steps,ingredients,equipment;
        public Dose[] doses;
        public int difficulty,bakeMinutes,coolMinutes,temperatureC; public bool fullyPlayable,critical;
    }
    [Serializable] public class DayPlan { public int number,orders,minimumXP; public string title,customer; public bool hints; }
    [Serializable] public class Pattern { public int[] levels; }
    [Serializable] public class Library { public Pattern[] regularPatterns; public string[] customers; public int shiftHours; public Formula[] recipes; public DayPlan[] days; public int gameSecondsPerSecond; }
    public enum Step { Tutorial, Summary, Measure, Process, Form, Pour, Load, Bake, Cool, CreamCool, Assemble, Decor, Wrap, Dispatch, Report }
}
