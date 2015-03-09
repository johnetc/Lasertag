using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HitDetector : MonoBehaviour
{

    //public List<GameScreen.TouchLocation> HitLocation = new List<GameScreen.TouchLocation>();

    public List<GameScreen.TouchLocation> ShieldLocations = new List<GameScreen.TouchLocation>();

    public bool IsHit;

    public void CheckHit(GameScreen.TouchLocation location)
    {
        if (!ShieldLocations.Contains(location))
        {
            IsHit = true;
        }
        
    }

}
