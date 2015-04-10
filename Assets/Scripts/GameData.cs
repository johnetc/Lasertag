using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections;
//using UnityEngine.SocialPlatforms.Impl;

public class GameData
{
    //game
    public static int StartScore = 0;
    public static int StartLives = 500;
    public static int CurrentScore = 0;
    public static int CurrentLives = 0;

    public static int MaxTapsOnPanel = 5;
    public static int PanelCooldownMS = 1000;

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

    public static string DeathParticleShot = "DeathExplosionBlue";
    public static string DeathParticleBorder = "DeathExplosionRed";
    public static string ShotParticleSystem = "ParticleSystem";
    public static string DeathParticleExposion = "DeathExplosion";

    //enemies
    public static float MaxEnemyObjects = 7;
    public static float ObjectCreationIntervalMS = 800;
    public static float ObjectScale = 20;

    public static float StartSpeed = 0.15f;
    public static float CollisionRotSpeed = 3f;
    public static float CollisionRotTotal = 90f;
    public static float InvincibilityTimerMS = 3000f;
    public static float InvincibilityFlashSpeedMult = 2f;
    
    //background
    public static float BackgroundDepthModifier = 40;
    public static float BackgroundPillarYNumber = 10;
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
        ParticleSystem,
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
        //public List<TouchLocation> Top;
        //public List<TouchLocation> Bottom;
        //public List<TouchLocation> Left;
        //public List<TouchLocation> Right;
        public List<TouchLocation> Locations;
        public int Random;

        public ShieldArrangement()
        {
            Locations = new List<TouchLocation> ();
        }

        public ShieldArrangement(int [] tempDat)
        {

            Locations = new List<TouchLocation>();

            for ( int i = 0; i < tempDat[0]; i++ )
            {
                Locations.Add ( TouchLocation.Top );
            }
            for ( int i = 0; i < tempDat [ 1 ]; i++ )
            {
                Locations.Add ( TouchLocation.Bottom );
            }
            for ( int i = 0; i < tempDat [ 2 ]; i++ )
            {
                Locations.Add ( TouchLocation.Left );
            }
            for ( int i = 0; i < tempDat [ 3 ]; i++ )
            {
                Locations.Add ( TouchLocation.Right );
            }

            Random = tempDat [ 4 ];
        }
    }

    public static int[][] ShieldArray =
    {
        new []{0,0,0,0,1},
        new []{0,0,1,1,0},
        new []{1,1,0,0,0},
        new []{1,0,1,0,0}, 
    };

}
