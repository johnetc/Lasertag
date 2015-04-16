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
    public float ParticleShotIntervalMS;
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
        ShotType = GameData.ParticleShotType.BubbleShot;
    }
}

public class LaserShot : BaseShotType
{
    public LaserShot ()
    {
        ShotPrefabName = "BasicShot";
        //GameData.ShotParticleVelocityMult = 200;
        //GameData.ShotParticleSize = 50;
        //GameData.ShotParticleLifetime = 10;
        //GameData.ShotParticleColour = new Color32 ( 255 , 255 , 255 , 255 );
        //GameData.NumberOfParticlesPerShot = 1;
        //GameData.ParticleShotIntervalMS = 2000;
    }
}
