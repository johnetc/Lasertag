using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class ParticleManager
{
    #region singleton

    private static readonly ParticleManager instance = new ParticleManager();

    public static ParticleManager Instance 
    {
        get
        {
            return instance;
        }
    }

    #endregion
    
    public GameObject ParticleContainer;

    private GameObject _NewParticle;
    
    private BaseShotType CurrentShotClass;
    private class ParticleBurst
    {
        public int LeftToFire;
        public int TotalToFire;
        public int DeadParticles;
        public ParticleSystem ThisSystem;
        public ParticleSystem.Particle [] TheseParticles;
        public Stopwatch StopW = new Stopwatch ();
        public bool Started;
        public bool ReadyForDisposal;
        public BaseShotType BaseShotTypeClass;
    }

    private List<ParticleBurst> _ActiveAdvParticleBursts = new List<ParticleBurst> ();
    private List<ParticleBurst> _ActiveBasicParticleBursts = new List<ParticleBurst> ();
    private List<ParticleBurst> _DeathExplosionsList = new List<ParticleBurst> ();

    private Dictionary<string, Material> _MaterialDict;
    private Dictionary<string , GameObject> _ParticlePrefabDict;

    public void Preload ()
    {
        LoadAssets ();
    }

    public void Initiate()
    {
        Reset();
    }

    public void Play ()
    {
        FireScheduledParticles ();
        CheckParticleSystemDisposal ();
        CheckAdvForDeadParticles ();
    }

    public void Pause()
    {
        ParticlePause(true);
    }

    public void Unpaused ()
    {
        ParticlePause(false);
    }

    public void LoadAssets()
    {

        _ParticlePrefabDict = new Dictionary<string , GameObject> ();

        _MaterialDict = new Dictionary<string, Material>();

        var resources = Resources.LoadAll ( "Particles" );

        foreach ( var resource in resources )
        {
            if ( resource is GameObject )
            {
                _ParticlePrefabDict.Add ( resource.name , ( GameObject ) resource );
            }
            if ( resource is Material )
            {
                _MaterialDict.Add ( resource.name , ( Material ) resource );
            }
        }

        ParticleContainer = new GameObject("Particle_Container");
        ParticleContainer.transform.position = SceneManager.Instance.MainCamera.transform.position;
    }

    public void NewShot(GameData.TouchLocation loc)
    {
        switch (SceneManager.Instance.CurrentParticleShotType)
        {
            case GameData.ParticleShotType.LaserShot:
            {
                CurrentShotClass = new LaserShot();
                NewShotSystemBasic(loc, CurrentShotClass); 
            }
            break;

            case GameData.ParticleShotType.BubbleShot:
            {
                CurrentShotClass = new BubbleShot();
                NewShotSystemAdv(loc, CurrentShotClass);
            }
            break;
            case GameData.ParticleShotType.SpreadShot:
            {
                CurrentShotClass = new SpreadShot();
                NewShotSystemBasic( loc , CurrentShotClass );
            }
            break;
        }
    }

    public void NewShotSystemBasic ( GameData.TouchLocation loc, BaseShotType shotClass )
    {
        string prefabType = shotClass.ShotPrefabName;

        float initialAngleOfShot = 0;
        float shotAngleIncrement = 0;

        if (shotClass.ShotField != 0)
        {
            initialAngleOfShot = -(shotClass.ShotField*0.5f);
            shotAngleIncrement = shotClass.ShotField/(shotClass.NumberOfPrefabsPerShot - 1);
            Debug.Log(initialAngleOfShot + " increment " + shotAngleIncrement);
        }

        for (int i = 0; i < shotClass.NumberOfPrefabsPerShot; i++)
        {
            float shotAngleModifier = 0;

            if (shotClass.ShotField != null)
            {
                shotAngleModifier = initialAngleOfShot + (i*shotAngleIncrement);
            }

            _NewParticle = GameObject.Instantiate(_ParticlePrefabDict[prefabType]);

            switch (loc)
            {
                case GameData.TouchLocation.Left:
                {
                    _NewParticle.transform.eulerAngles = new Vector3 ( 0 , 0 , 0 + shotAngleModifier );
                    _NewParticle.GetComponent<ParticleColliderMono>().thisLocation = GameData.TouchLocation.Left;
                    break;
                }
                case GameData.TouchLocation.Right:
                {
                    _NewParticle.transform.eulerAngles = new Vector3(0, 0, 180+shotAngleModifier);
                    _NewParticle.GetComponent<ParticleColliderMono>().thisLocation = GameData.TouchLocation.Right;
                    break;
                }
                case GameData.TouchLocation.Top:
                {
                    _NewParticle.transform.eulerAngles = new Vector3 ( 0 , 0 , 270 + shotAngleModifier );
                    _NewParticle.GetComponent<ParticleColliderMono>().thisLocation = GameData.TouchLocation.Top;
                    break;
                }
                case GameData.TouchLocation.Bottom:
                {
                    _NewParticle.transform.eulerAngles = new Vector3 ( 0 , 0 , 90 + shotAngleModifier );
                    _NewParticle.GetComponent<ParticleColliderMono>().thisLocation = GameData.TouchLocation.Bottom;
                    break;
                }

            }

            ParticleSystem tempSys = _NewParticle.GetComponent<ParticleSystem>();

            ParticleBurst tempBurst = new ParticleBurst();

            tempBurst.ThisSystem = tempSys;

            tempBurst.BaseShotTypeClass = shotClass;

            _NewParticle.GetComponent<ParticleColliderMono>().ThisParticleShotType = shotClass.ShotType;

            _NewParticle.transform.SetParent(ParticleContainer.transform);

            _NewParticle.transform.position =
                SceneManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                    Input.mousePosition.y, 100));

            _ActiveBasicParticleBursts.Add(tempBurst);
        }
    }

    public void NewShotSystemAdv ( GameData.TouchLocation loc , BaseShotType shotClass )
    {

        string prefabType = shotClass.ShotPrefabName;

        _NewParticle = GameObject.Instantiate ( _ParticlePrefabDict [ prefabType ] );

        _NewParticle.GetComponent<ParticleColliderMono> ().ThisParticleShotType = shotClass.ShotType;

        _NewParticle.transform.SetParent ( ParticleContainer.transform );

        _NewParticle.transform.position = SceneManager.Instance.MainCamera.ScreenToWorldPoint ( new Vector3 ( Input.mousePosition.x , Input.mousePosition.y , 100 ) );

        ParticleSystem tempSys = _NewParticle.GetComponent<ParticleSystem> ();

        ParticleBurst tempBurst = new ParticleBurst ();

        tempBurst.BaseShotTypeClass = shotClass;

        tempBurst.ThisSystem = tempSys;

        ParticleSystem.Particle [] tempPart = new ParticleSystem.Particle [ shotClass.NumberOfParticlesPerShot ];

        tempBurst.LeftToFire = tempPart.Count () - 1;
        tempBurst.TotalToFire = tempBurst.LeftToFire;

        switch ( loc )
        {
            case GameData.TouchLocation.Left:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.right, shotClass );

                    _NewParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Left;
                    break;
                }
            case GameData.TouchLocation.Right:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.left , shotClass );

                    _NewParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Right;
                    break;
                }
            case GameData.TouchLocation.Top:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.down , shotClass );

                    _NewParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Top;
                    break;
                }
            case GameData.TouchLocation.Bottom:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.up , shotClass );

                    _NewParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Bottom;
                    break;
                }

        }

        tempSys.Emit ( tempPart [ 0 ] );
        tempBurst.TheseParticles = tempPart;
        _ActiveAdvParticleBursts.Add ( tempBurst );
    }

    public void FireDeathExplosion(Vector3 pos, string col)
    {
        GameObject tempDeathExplosion = GameObject.Instantiate ( _ParticlePrefabDict [ col ] , pos , Quaternion.identity ) as GameObject;

        tempDeathExplosion.transform.SetParent ( ParticleContainer.transform );

        ParticleBurst tempParticleBurst = new ParticleBurst();

        ParticleSystem tempSys = tempDeathExplosion.GetComponent<ParticleSystem> ();

        //tempSys.GetComponent<Renderer>().material = _MaterialDict[col];

        tempParticleBurst.ThisSystem = tempSys;

        _DeathExplosionsList.Add(tempParticleBurst);
    }

    public void ParticleSystemDebug ( ParticleSystem partSys )
    {
        ParticleSystem.Particle [] particles = new ParticleSystem.Particle [ partSys.maxParticles ];
        var partNumber = partSys.GetParticles ( particles );

        Debug.Log ( particles [ 0 ].velocity );
        Debug.Log ( particles [ 0 ].lifetime );
        Debug.Log ( particles [ 0 ].startLifetime );

    }

    public ParticleSystem.Particle [] ParticleDefiner ( Vector3 pos, Vector3 vector, BaseShotType shotClass )
    {
        ParticleSystem.Particle [] particles = new ParticleSystem.Particle [ shotClass.NumberOfParticlesPerShot ];

        for ( int i = 0; i < particles.Count (); i++ )
        {
            particles [ i ].position = pos;
            particles [ i ].velocity = vector * shotClass.ShotParticleVelocityMult;
            particles [ i ].size = shotClass.ShotParticleSize;
            particles [ i ].lifetime = shotClass.ShotParticleLifetime;
            particles [ i ].startLifetime = shotClass.ShotParticleLifetime;
            particles [ i ].color = shotClass.ShotParticleColour;
        }

        return particles;
    }

    public void FireScheduledParticles ()
    {
        foreach ( var particle in _ActiveAdvParticleBursts )
        {
            if ( !particle.Started )
            {
                particle.StopW.Start ();
            }
        }

        foreach ( var mParticleBurst in _ActiveAdvParticleBursts )
        {
            //Debug.Log(mParticleBurst.StopW.ElapsedMilliseconds);
            if ( mParticleBurst.StopW.ElapsedMilliseconds > mParticleBurst.BaseShotTypeClass.ParticleShotIntervalMS && mParticleBurst.LeftToFire > 0 )
            {
                mParticleBurst.StopW.Reset ();
                mParticleBurst.StopW.Start ();
                mParticleBurst.ThisSystem.Emit ( mParticleBurst.TheseParticles [ mParticleBurst.TotalToFire - mParticleBurst.LeftToFire + 1 ] );
                mParticleBurst.LeftToFire--;
            }
            if ( mParticleBurst.LeftToFire < 1 )
            {
                mParticleBurst.StopW.Stop ();
                //mParticleBurst.ReadyForDisposal = true;
            }
        }

    }

    public void CheckParticleSystemDisposal ()
    {
        for ( int i = _ActiveAdvParticleBursts.Count - 1; i > -1; i-- )
        {
            if ( _ActiveAdvParticleBursts [ i ].DeadParticles > _ActiveAdvParticleBursts [ i ].TotalToFire )
            {
                //_SpentAdvParticleBursts.Add ( _ActiveAdvParticleBursts [ i ] );
                _ActiveAdvParticleBursts [ i ].ThisSystem.gameObject.SetActive ( false );
                Object.Destroy(_ActiveAdvParticleBursts[i].ThisSystem.gameObject);
                _ActiveAdvParticleBursts.Remove ( _ActiveAdvParticleBursts [ i ] );
            }
        }


        for (int i = _DeathExplosionsList.Count - 1; i > -1; i--)
        {
            if ( !_DeathExplosionsList [ i ].ThisSystem.isPlaying)
            {
                //_SpentAdvParticleBursts.Add ( _ActiveAdvParticleBursts [ i ] );
                _DeathExplosionsList [ i ].ThisSystem.gameObject.SetActive ( false );
                Object.Destroy ( _DeathExplosionsList [ i ].ThisSystem.gameObject );
                _DeathExplosionsList.Remove ( _DeathExplosionsList [ i ] );
            }
        }

        for ( int i = _ActiveBasicParticleBursts.Count - 1; i > -1; i-- )
        {
            if (_ActiveBasicParticleBursts [ i ].ThisSystem == null )
            {
                _ActiveBasicParticleBursts.Remove ( _ActiveBasicParticleBursts [ i ] );
                return;
            }
            
            if ( !_ActiveBasicParticleBursts [ i ].ThisSystem.isPlaying )
            {
                //_SpentAdvParticleBursts.Add ( _ActiveAdvParticleBursts [ i ] );
                _ActiveBasicParticleBursts [ i ].ThisSystem.gameObject.SetActive ( false );
                Object.Destroy ( _ActiveBasicParticleBursts [ i ].ThisSystem.gameObject );
                _ActiveBasicParticleBursts.Remove ( _ActiveBasicParticleBursts [ i ] );
            }
        }

    }

    public void CheckAdvForDeadParticles ()
    {
        foreach ( var mActiveParticleBurst in _ActiveAdvParticleBursts )
        {
            ParticleSystem.Particle [] part = new ParticleSystem.Particle [ mActiveParticleBurst.ThisSystem.maxParticles ];
            var partNo = mActiveParticleBurst.ThisSystem.GetParticles ( part );
            for ( int i = 0; i < partNo; i++ )
            {
                if ( CheckParticleStats ( part [ i ], mActiveParticleBurst.BaseShotTypeClass ) )
                {
                    part [ i ].lifetime = 0;
                    mActiveParticleBurst.DeadParticles++;
                }
            }
            mActiveParticleBurst.ThisSystem.SetParticles ( part , partNo );
        }
    }

    public bool CheckParticleStats ( ParticleSystem.Particle particle, BaseShotType shotClass )
    {
        if ( particle.velocity.magnitude < shotClass.ShotParticleVelocityMult )
        {
            return true;
        }
        if ( particle.position.x > GameData.BottomRightPoint.x || particle.position.x < GameData.TopLeftPoint.x )
        {
            return true;
        }
        if ( particle.position.y > GameData.TopLeftPoint.y || particle.position.y < GameData.BottomRightPoint.y )
        {
            return true;
        }

        return false;
    }

    public void ParticlePause(bool pause)
    {
        if (pause)
        {
            for ( int i = _ActiveAdvParticleBursts.Count - 1; i > -1; i-- )
            {
                _ActiveAdvParticleBursts [ i ].ThisSystem.Pause();
            }


            for ( int i = _DeathExplosionsList.Count - 1; i > -1; i-- )
            {
               _DeathExplosionsList [ i ].ThisSystem.Pause();
            }

            for ( int i = _ActiveBasicParticleBursts.Count - 1; i > -1; i-- )
            {
                if ( _ActiveBasicParticleBursts [ i ].ThisSystem == null )
                {
                    return;
                }
                
                _ActiveBasicParticleBursts [ i ].ThisSystem.Pause();
            }
        }
        else
        {
            for ( int i = _ActiveAdvParticleBursts.Count - 1; i > -1; i-- )
            {
                _ActiveAdvParticleBursts [ i ].ThisSystem.Play(); 
            }


            for ( int i = _DeathExplosionsList.Count - 1; i > -1; i-- )
            {
                _DeathExplosionsList [ i ].ThisSystem.Play ();
            }

            for ( int i = _ActiveBasicParticleBursts.Count - 1; i > -1; i-- )
            {
                if ( _ActiveBasicParticleBursts [ i ].ThisSystem == null )
                {
                    return;
                }

                if ( !_ActiveBasicParticleBursts [ i ].ThisSystem.isPlaying )
                {
                    _ActiveBasicParticleBursts [ i ].ThisSystem.Play ();
                }
            }
        }
    }

    public void Reset()
    {
        foreach (var activeAdvParticleBurst in _ActiveAdvParticleBursts)
        {
            Object.Destroy ( activeAdvParticleBurst.ThisSystem.gameObject );
        }
        foreach (var activeBasicParticleBurst in _ActiveBasicParticleBursts)
        {
            Object.Destroy ( activeBasicParticleBurst.ThisSystem.gameObject );
        }
        foreach (var particleBurst in _DeathExplosionsList)
        {
            Object.Destroy(particleBurst.ThisSystem.gameObject);
        }
        
        _ActiveAdvParticleBursts = new List<ParticleBurst> ();
        _ActiveBasicParticleBursts = new List<ParticleBurst> ();
        _DeathExplosionsList = new List<ParticleBurst> ();
    }

}
