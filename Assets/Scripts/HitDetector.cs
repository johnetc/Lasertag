using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HitDetector : MonoBehaviour
{

    public int Id;

    //public List<GameScreen.TouchLocation> ShieldLocations = new List<GameScreen.TouchLocation>();

    //public bool IsHit;

    public void CheckHit(GameData.TouchLocation location)
    {
        //if (!ShieldLocations.Contains(location))
        //{
        //    IsHit = true;
        //}
        EnemyManager.Instance.CheckHit(Id, location );
    }

}
