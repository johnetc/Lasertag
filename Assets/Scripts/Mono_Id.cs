using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Mono_Id : MonoBehaviour
{
    public int Id;
    public GameData.ComponentType ThisObjType;

    public void OnCollisionEnter(Collision collision)
    {
       //Debug.Log(this.gameObject.name);
        EnemyManager.Instance.CheckColliders(this.gameObject, collision.gameObject);
    }

}
