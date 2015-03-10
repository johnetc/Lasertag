using UnityEngine;
using System.Collections;

public class GameData
{

    public static float ShotParticleVelocityMult = 200;
    public static float ShotParticleSize = 10;
    public static float ShotParticleLifetime = 10;
    public static Color32 ShotParticleColour = new Color32(255,255,255,255);
    public static int NumberOfParticlesPerShot = 1;
    public static float ParticleShotIntervalMS = 2000;
    public static float ObjectCreationIntervalMS = 3000;

    public static Vector3 TopLeftPoint;
    public static Vector3 BottomRightPoint;
    public static float MidPointx;
    public static float MidPointy;
    public static Vector3 MidPoint ;
    public static Vector3 TopLeftSpawnArea;
    public static Vector3 BottomRightSpawnArea;
    public static float SpawnScreenFraction = 0.35f;
    public static float StartSpeed = 0.15f;

    public enum TouchLocation
    {
        Top ,
        Left ,
        Bottom ,
        Right ,
    }

    public static void CalculateScreenDimensions()
    {
        TopLeftPoint = SceneManager.Instance.MainCamera.ViewportToWorldPoint(new Vector3(0, 1, SceneManager.Instance.MainCamera.nearClipPlane));
        BottomRightPoint = SceneManager.Instance.MainCamera.ViewportToWorldPoint ( new Vector3 ( 1 , 0 , SceneManager.Instance.MainCamera.nearClipPlane ) );
        MidPointx = TopLeftPoint.x + ( BottomRightPoint.x - TopLeftPoint.x ) * 0.5f;
        MidPointy = TopLeftPoint.y + ( BottomRightPoint.y - TopLeftPoint.y ) * 0.5f;
        MidPoint = new Vector3(MidPointx, MidPointy, 100);
    }

    public enum ObjectType
    {
        SingleShield ,
        LeftRightShield ,
        TopBottomShield,
        LeftTopShield,

        count
    }

    public class ShieldArrangement
    {
        public int Top;
        public int Bottom;
        public int Left;
        public int Right;
        public int Random;

        public ShieldArrangement(){}

        public ShieldArrangement(int t, int b, int l, int r, int rand)
        {
            Top = t;
            Bottom = b;
            Left = l;
            Right = r;
            Random = rand;
        }
    }

    public static ShieldArrangement[] ShieldArray =
    {
        new ShieldArrangement(0,0,0,0,1),
        new ShieldArrangement(0,0,1,1,0),
        new ShieldArrangement(1,1,0,0,0),
        new ShieldArrangement(1,0,1,0,0), 
    };

}
