using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class ParticleManager
{
    #region singleton

    public static readonly ParticleManager instance = new ParticleManager();

    public static ParticleManager Instance 
    {
        get
        {
            return instance;
        }
    }

    #endregion
    
    public GameObject ParticleContainer;
    
    private List<ParticleSystem.Particle> ActiveParticles = new List<ParticleSystem.Particle> ();

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
    }

    private List<ParticleBurst> _ActiveParticleBursts = new List<ParticleBurst> ();
    private List<ParticleBurst> _SpentParticleBursts = new List<ParticleBurst> ();

    private List<ParticleBurst> _DeathExplosionsList = new List<ParticleBurst> ();

    private Dictionary<string, Material> _MaterialDict;
    private Dictionary<string , GameObject> _ParticlePrefabDict; 
    
    public ParticleManager()
    {
        LoadAssets();
    }

    public void Update()
    {
        FireScheduledParticles ();
        CheckParticleSystemDisposal ();
        CheckForDeadParticles ();
    }

    public void LoadAssets()
    {

        _ParticlePrefabDict = new Dictionary<string , GameObject> ();

        GameObject [] tempObjArray = Resources.LoadAll<GameObject> ( "Particles" );

        foreach ( var obj in tempObjArray )
        {
            _ParticlePrefabDict.Add ( obj.name , obj );
        }

        _MaterialDict = new Dictionary<string, Material>();

        Material[] tempMatArray = Resources.LoadAll<Material>("Materials");

        foreach (var material in tempMatArray)
        {
            _MaterialDict.Add(material.name, material);
        }
        
        ParticleContainer = new GameObject("Particle_Container");
        ParticleContainer.transform.position = SceneManager.Instance.MainCamera.transform.position;
    }

    public void NewShotSystem(GameScreen.PanelData panel)
    {
        GameObject newParticle = GameObject.Instantiate ( _ParticlePrefabDict [ GameData.ShotParticleSystem ] );

        newParticle.transform.SetParent ( ParticleContainer.transform );

        newParticle.transform.position = SceneManager.Instance.MainCamera.ScreenToWorldPoint ( new Vector3 ( Input.mousePosition.x , Input.mousePosition.y , 100 ) );

        ParticleSystem tempSys = newParticle.GetComponent<ParticleSystem> ();

        ParticleBurst tempBurst = new ParticleBurst ();

        tempBurst.ThisSystem = tempSys;

        ParticleSystem.Particle [] tempPart = new ParticleSystem.Particle [ GameData.NumberOfParticlesPerShot ];

        tempBurst.LeftToFire = tempPart.Count () - 1;
        tempBurst.TotalToFire = tempBurst.LeftToFire;

        switch ( panel.PanelLoc )
        {
            case GameData.TouchLocation.Left:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.right * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Left;
                    break;
                }
            case GameData.TouchLocation.Right:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.left * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Right;
                    break;
                }
            case GameData.TouchLocation.Top:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.down * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Top;
                    break;
                }
            case GameData.TouchLocation.Bottom:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.up * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Bottom;
                    break;
                }

        }
        tempSys.Emit ( tempPart [ 0 ] );
        tempBurst.TheseParticles = tempPart;
        _ActiveParticleBursts.Add ( tempBurst );
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

    public ParticleSystem.Particle [] ParticleDefiner ( Vector3 pos , Vector3 vel , float size , float life , Color32 colour )
    {
        ParticleSystem.Particle [] particles = new ParticleSystem.Particle [ GameData.NumberOfParticlesPerShot ];

        for ( int i = 0; i < particles.Count (); i++ )
        {
            particles [ i ].position = pos;
            particles [ i ].velocity = vel;
            particles [ i ].size = size;
            particles [ i ].lifetime = life;
            particles [ i ].startLifetime = life;
            particles [ i ].color = colour;
        }

        return particles;
    }

    public void FireScheduledParticles ()
    {
        foreach ( var particle in _ActiveParticleBursts )
        {
            if ( !particle.Started )
            {
                particle.StopW.Start ();
            }
        }

        foreach ( var mParticleBurst in _ActiveParticleBursts )
        {
            //Debug.Log(mParticleBurst.StopW.ElapsedMilliseconds);
            if ( mParticleBurst.StopW.ElapsedMilliseconds > GameData.ParticleShotIntervalMS && mParticleBurst.LeftToFire > 0 )
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
        for ( int i = _ActiveParticleBursts.Count - 1; i > -1; i-- )
        {
            if ( _ActiveParticleBursts [ i ].DeadParticles > _ActiveParticleBursts [ i ].TotalToFire )
            {
                //_SpentParticleBursts.Add ( _ActiveParticleBursts [ i ] );
                _ActiveParticleBursts [ i ].ThisSystem.gameObject.SetActive ( false );
                Object.Destroy(_ActiveParticleBursts[i].ThisSystem.gameObject);
                _ActiveParticleBursts.Remove ( _ActiveParticleBursts [ i ] );
            }
        }


        for (int i = _DeathExplosionsList.Count - 1; i > -1; i--)
        {
            if ( !_DeathExplosionsList [ i ].ThisSystem.isPlaying)
            {
                //_SpentParticleBursts.Add ( _ActiveParticleBursts [ i ] );
                _DeathExplosionsList [ i ].ThisSystem.gameObject.SetActive ( false );
                Object.Destroy ( _DeathExplosionsList [ i ].ThisSystem.gameObject );
                _DeathExplosionsList.Remove ( _DeathExplosionsList [ i ] );
            }
        }

    }

    public void CheckForDeadParticles ()
    {
        foreach ( var mActiveParticleBurst in _ActiveParticleBursts )
        {
            ParticleSystem.Particle [] part = new ParticleSystem.Particle [ mActiveParticleBurst.ThisSystem.maxParticles ];
            var partNo = mActiveParticleBurst.ThisSystem.GetParticles ( part );
            for ( int i = 0; i < partNo; i++ )
            {
                if ( CheckParticleStats ( part [ i ] ) )
                {
                    part [ i ].lifetime = 0;
                    mActiveParticleBurst.DeadParticles++;
                }
            }
            mActiveParticleBurst.ThisSystem.SetParticles ( part , partNo );
        }
    }

    public bool CheckParticleStats ( ParticleSystem.Particle particle )
    {
        if ( particle.velocity.magnitude < GameData.ShotParticleVelocityMult )
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
}
