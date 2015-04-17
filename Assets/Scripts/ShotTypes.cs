using UnityEngine;
using System.Collections;

public abstract class BaseShotType
{
    public string ShotPrefabName;
    public float ShotParticleVelocityMult;
    public float ShotParticleSize;
    public float ShotParticleLifetime;
    public Color32 ShotParticleColour;
    public int NumberOfParticlesPerShot;
    public int NumberOfPrefabsPerShot;
    public float ShotField;
    public float ParticleShotIntervalMS;
    public string ShotMaterialName;
    public GameData.ParticleShotType ShotType;
}

public class BubbleShot : BaseShotType
{
    public BubbleShot ()
    {
        ShotPrefabName = "BubbleShot";
        ShotParticleVelocityMult = 200;
        ShotParticleSize = 10;
        ShotParticleLifetime = 10;
        ShotParticleColour = new Color32 ( 255 , 255 , 255 , 255 );
        NumberOfParticlesPerShot = 1;
        ParticleShotIntervalMS = 2000;
        ShotMaterialName = "BubbleShot";
        ShotType = GameData.ParticleShotType.BubbleShot;
    }
}

public class LaserShot : BaseShotType
{
    public LaserShot ()
    {
        ShotPrefabName = "LaserShot";
        NumberOfPrefabsPerShot = 1;
        ShotField = 0;
        ShotType = GameData.ParticleShotType.LaserShot;
    }
}

public class SpreadShot : BaseShotType
{
    public SpreadShot ()
    {
        ShotPrefabName = "SpreadShot";
        NumberOfPrefabsPerShot = 3;
        ShotField = 90;
        ShotType = GameData.ParticleShotType.SpreadShot;
    }
}
