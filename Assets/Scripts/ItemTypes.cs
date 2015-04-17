using UnityEngine;
using System.Collections;

public abstract class BaseItemTypes
{
    public float ItemSize;
    public string ItemName;
    public float VisibleTimeMS;
    public abstract void ActivateItem();

}

public class LaserShotItem : BaseItemTypes
{
    public LaserShotItem ()
    {
        ItemSize = 20;
        VisibleTimeMS = 5000;
        ItemName = "LaserShotItem";
    }

    public override void ActivateItem ()
    {
        SceneManager.Instance.CurrentParticleShotType = GameData.ParticleShotType.LaserShot;
    }
}

public class BubbleShotItem : BaseItemTypes
{
    public BubbleShotItem ()
    {
        ItemSize = 20;
        VisibleTimeMS = 5000;
        ItemName = "BubbleShotItem";
    }

    public override void ActivateItem ()
    {
        SceneManager.Instance.CurrentParticleShotType = GameData.ParticleShotType.BubbleShot;
    }
}

public class SpreadShotItem : BaseItemTypes
{
    public SpreadShotItem ()
    {
        ItemSize = 20;
        VisibleTimeMS = 5000;
        ItemName = "SpreadShotItem";
    }

    public override void ActivateItem ()
    {
        SceneManager.Instance.CurrentParticleShotType = GameData.ParticleShotType.SpreadShot;
    }
}
