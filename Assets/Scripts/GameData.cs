using System.Collections.Generic;
using UnityEngine;
using System.Collections;
//using UnityEngine.SocialPlatforms.Impl;

public class GameData
{
    //game
    public static int StartScore = 0;
    public static int StartLives = 5;
    public static int CurrentScore = 0;
    public static int CurrentLives = 0;
    
    //game area
    public static float CameraDepth = 100;
    public static float ShieldDepth = 90;
    
    public static Vector3 TopLeftPoint;
    public static Vector3 BottomRightPoint;

    public static float MidPointx;
    public static float MidPointy;
    public static Vector3 MidPoint;
    public static Vector3 TopLeftSpawnArea;
    public static Vector3 BottomRightSpawnArea;
    public static float SpawnScreenFraction = 0.4f;

    public static Vector3 TopLeftEnemyBorder;
    public static Vector3 BottomRightEnemyBorder;
    
    //particles
    public static float ShotParticleVelocityMult = 200;
    public static float ShotParticleSize = 10;
    public static float ShotParticleLifetime = 10;
    public static Color32 ShotParticleColour = new Color32(255,255,255,255);
    public static int NumberOfParticlesPerShot = 1;
    public static float ParticleShotIntervalMS = 2000;
    

    //enemies
    public static float MaxEnemyObjects = 7;
    public static float ObjectCreationIntervalMS = 800;
    public static float ObjectScale = 20;

    public static float StartSpeed = 0.15f;
    public static float CollisionRotSpeed = 3f;
    public static float CollisionRotTotal = 90f;
    public static float InvincibilityTimerMS = 3000f;
    
    public enum TouchLocation
    {
        Top ,
        Right ,
        Bottom ,
        Left ,
    }

    public enum ComponentType
    {
        MainBody,
        Shield,
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
        public List<TouchLocation> Top;
        public List<TouchLocation> Bottom;
        public List<TouchLocation> Left;
        public List<TouchLocation> Right;
        public int Random;

        public ShieldArrangement(){}

        public ShieldArrangement(int t, int b, int l, int r, int rand)
        {
            Top = new List<TouchLocation>();
            Bottom = new List<TouchLocation>();
            Left = new List<TouchLocation>();
            Right = new List<TouchLocation>();

            for (int i = 0; i < t; i++)
            {
                Top.Add(TouchLocation.Top);
            }
            for ( int i = 0; i < b; i++ )
            {
                Bottom.Add ( TouchLocation.Bottom );
            }
            for ( int i = 0; i < l; i++ )
            {
                Left.Add ( TouchLocation.Left );
            } 
            for ( int i = 0; i < r; i++ )
            {
                Right.Add ( TouchLocation.Right );
            }
            
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
