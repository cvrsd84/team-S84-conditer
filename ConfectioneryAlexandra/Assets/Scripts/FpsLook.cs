using UnityEngine;
namespace AlexandraBakery
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FpsLook : MonoBehaviour
    {
        public static readonly Vector3 StartPoint=new Vector3(-.48f,.12f,-4.28f);
        CharacterController motor; Camera eyes; float pitch=6,fall;
        void Awake(){motor=GetComponent<CharacterController>();}
        public void Recover(){motor.enabled=false;transform.SetPositionAndRotation(StartPoint,Quaternion.identity);motor.enabled=true;fall=0;pitch=6;
            if(eyes)eyes.transform.localRotation=Quaternion.Euler(pitch,0,0);}
        void Update(){
            if(transform.position.y < -1 || Mathf.Abs(transform.position.x)>7.12f || Mathf.Abs(transform.position.z)>6.12f)Recover();
            if(!eyes)eyes=GetComponentInChildren<Camera>();
            var game=BakeryGame.Current;if(!game||game.IsGameplayInputBlocked||!eyes)return;
            if(Cursor.lockState==CursorLockMode.Locked){
                transform.Rotate(0,Input.GetAxis("Mouse X")*2.15f,0);
                pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*2.15f,-82,82);eyes.transform.localRotation=Quaternion.Euler(pitch,0,0);}
            var move=Vector3.ClampMagnitude(transform.forward*Input.GetAxisRaw("Vertical")+transform.right*Input.GetAxisRaw("Horizontal"),1);
            float dt=Mathf.Min(Time.deltaTime,.08f);fall=motor.isGrounded?-2:Mathf.Max(fall+Physics.gravity.y*dt,-18);
            motor.Move((move*(Input.GetKey(KeyCode.LeftShift)?4.5f:3.0f)+fall*Vector3.up)*dt);
        }
    }
}

