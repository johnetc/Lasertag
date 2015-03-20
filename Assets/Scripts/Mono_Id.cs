using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Mono_Id : MonoBehaviour
{
    public int Id;
    public GameData.ComponentType ThisObjType;

    public void OnCollisionEnter (Collision collision)
    {
        //Debug.Log ( this.gameObject.name );
        if (collision.gameObject != null)
        {
            //Debug.Log(Id);
            //Debug.Log ( "c name " + collision.gameObject.name );
            //Debug.Log ( collision.gameObject.GetComponent<Mono_Id> ().Id );
            if (Id != collision.gameObject.GetComponent<Mono_Id>().Id)
            {
                EnemyManager.Instance.CheckColliders(Id, ThisObjType, collision.gameObject.GetComponent<Mono_Id>().Id, collision.gameObject.GetComponent<Mono_Id>().ThisObjType);
            }
        }
    }

}
