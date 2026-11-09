using UnityEngine;
namespace AlexandraBakery
{
    public enum Purpose { Station, Stock, Utensil, Supply, Door, Finished, Packed }
    public enum Cleanliness { Clean, Used, Dirty }
    public sealed class BakeryTarget : MonoBehaviour
    {
        public string id="", title="", home="", slot="";
        public Purpose purpose;
        public Cleanliness condition;
        public Transform hinge;
        public bool opened;
        public float volume;
        public string fill;
        public Vector3 origin;
        public string Display => string.IsNullOrEmpty(title)?Props.Name(id):title;
        Quaternion shut;
        void Awake(){if(hinge)shut=hinge.localRotation;}
        void Update(){if(hinge)hinge.localRotation=Quaternion.RotateTowards(hinge.localRotation,shut*Quaternion.Euler(0,opened?102:0,0),Time.deltaTime*220);}
    }
}

