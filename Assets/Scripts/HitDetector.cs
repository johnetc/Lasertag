using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HitDetector : MonoBehaviour
{

    //public List<GameScreen.TouchLocation> HitLocation = new List<GameScreen.TouchLocation>();

    public List<GameScreen.TouchLocation> ShieldLocation = new List<GameScreen.TouchLocation>();

    public bool IsHit;

    public void AddHit(GameScreen.TouchLocation location)
    {
        //Debug.Log("hit "+ this.gameObject.name);
        if (!ShieldLocation.Contains(location))
        {
            IsHit = true;
            //Debug.Log("True");
        }
        
    }

}
