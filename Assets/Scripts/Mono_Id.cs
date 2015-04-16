using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Mono_Id : MonoBehaviour
{
    public int Id;
    public GameData.ComponentType ThisObjType;

    public int[] shieldlocs =
    {
        0,0,0,0
    };
    public void OnCollisionEnter (Collision collision)
    {
        //Debug.Log ( this.gameObject.name );
        if (collision.gameObject != null)
        {
            //Debug.Log(Id);
            //Debug.Log ( "c name " + collision.gameObject.name );
            //Debug.Log ( collision.gameObject.GetComponent<Mono_Id> ().Id );
            //if (Id != collision.gameObject.GetComponent<Mono_Id>().Id)
            //{
            switch (ThisObjType)
            {
                case  GameData.ComponentType.MainBody:
                case GameData.ComponentType.Shield:
                {
                    switch ( collision.gameObject.GetComponent<Mono_Id> ().ThisObjType )
                    {
                        case GameData.ComponentType.MainBody:
                        case GameData.ComponentType.Shield:
                            {
                                EnemyManager.Instance.CheckColliders ( Id , ThisObjType , collision.gameObject.GetComponent<Mono_Id> ().Id , collision.gameObject.GetComponent<Mono_Id> ().ThisObjType );
                            }
                            break;
                        case GameData.ComponentType.Item:
                            {

                            }
                            break;
                    }
                }
                break;
                case GameData.ComponentType.Item:
                {
                    
                }
                    break;
            }
            
            //}
        }
    }

    public void OnMouseDown()
    {
        switch ( ThisObjType )
        {
            case GameData.ComponentType.MainBody:
            case GameData.ComponentType.Shield:
                {

                }
                break;
            case GameData.ComponentType.Item:
                {
                    ItemManager.Instance.CheckItemPickup(Id);
                }
                break;
        }
    }

}
