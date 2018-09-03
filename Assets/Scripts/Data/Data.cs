using UnityEngine;

//REQUIREMENTS
//Cannot have infinite stationaries
//Can have as many triggers as you want, but only if they don't stay in the same place
//


namespace Data
{
    [System.Serializable]
    public enum WeaponType
    {
        Contact,//Apply effect on player, or on contact with subweapon
        Explosion,//Apply effect on all in range, around player or projectile, as described by ExplosionData
        Gun,//Apply effect with projectile, as described by ShotData and ProjectileData
        Shield,//Deploy shield on Player, or
    }

    [System.Serializable]
    public struct WeaponData
    {
        public string name;

        public WeaponType type;
        public EffectData effect;

        //Ignored unless type == shield
        public ShieldData shield;

        //Ignored unless type == explosion
        public ExplosionData explosion;

        //Ignored unless type == gun
        public ShotData shot;
        public RecoilData recoil;
        public ProjectileData projectile;

        public ResourceData resource;
    }

    //public float speed_modifier;//0.5-1.5
    //public float fire_aim_modifier;//only applied during fire
    //public float aim_modifier;//only applied during fire
    //cosmetic
    //public CosmeticData? shot_cosmetic;


    [System.Serializable]
    public class SubWeaponData
    {
        public WeaponType type;//Is the projectile a shield, an AOE effect, a turret or a contact thing
        public ActivationData activation;//When does the subweapon start acting
        public EffectData effect;
        public AimData aim;

        public ShieldData shield;//Ignored unless type == shield

        public ExplosionData explosion;//Ignored unless type == explosion

        public ShotData shot;
        public ProjectileData projectile;

        public ResourceData resource;
    }



    [System.Serializable]
    public struct ProjectileData
    {
        [Range(0f, 1f)]
        public float width;

        public TrajectoryData trajectory;
        public BehaviourData behaviour;

        //subprojectile
        [System.NonSerialized]
        public SubWeaponData subweapon;

        //cosmetic
        //public CosmeticData? hit_cosmetic;
        //public CosmeticData? move_cosmetic;
    }

    [System.Serializable]
    public struct ShieldData
    {
        [Range(0, 1f)]
        public float size;
        [Range(0, 100f)]
        public float health;
        //ShieldShape shape;
    }

    [System.Serializable]
    public struct ExplosionData
    {
        public float range;
        public float duration;//Ignored if explosion is activated on a moving projectile?
        public EffectData effect;
    }

    [System.Serializable]
    public struct ActivationData
    {
        public bool onContact;//The explosion, turret, or self effect activates on hitting a collidable entity
        public bool onTimeout;//The explosion, turret, or self effect activates on hitting a collidable entity
        public bool onTrigger;//The explosion, turret or self effect activates on pressing associated trigger
        public bool onStartup;//The explosion, turret, or self effect activates as the startupTimer ends

        [Range(0f, 1f)]
        public float setupTime;//Once whatever trigger is activated, the turret will wait for this as well

        public float activationTime;
        //How long will the subweapon be enabled
        //If 0f, the weapon will fire a single shot

        public bool immortal;
        //Requires the parent weapon to have a limited concurent children
        //Still dies if no more ammo

        public bool dieOnProjectileDeath;
    }

    [System.Serializable]
    public struct ShotData
    {
        [Range(0, 100)]
        public int count;//number of projectiles, strong cost multiplier
        [Range(0f, 360f)]
        public float cone;//angle of firing cone
        [Range(0f, 60f)]
        public float velocity;//muzzle velocity
        [Range(0f, 1f)]
        public float inheritedVelocity;//Percent of player velocity added to muzzle velocity
        [Range(0.1f, 100f)]
        public float rate;//Bullets per second
    }

    [System.Serializable]
    public enum AimType
    {
        Auto,//Can only fire if target has at least one element
        Lock,//Same. Children might inherit target.
        Front,
        Back,
        Up,
    }

    [System.Serializable]
    public struct AimData
    {
        public AimType aimType;
        //public float aquiring_delay = 1f;
        public float detection_range;//if auto-targeting
        //public float detection_margin;//allows to keep firing beyond detection range, but not detect
        public int maxTargets;//allows to attack multiple ennemies
        public bool waitForFleeingTarget;//Once target is in range, wait for distance to increase to trigger
    }

    [System.Serializable]
    public struct ResourceData {

        [Range(0, 1000000)]
        public int resourceMax; //all-time ammo capacity of the weapon. 0 is infinite ammo
        [Range(0, 1000000)]
        public int resourceCap; //limit of ammunition in the magazine. 0 is no magazine management
        //public int shot_cost; //cost of ammunition per shot
        //int abs_count; //limit of ammunition between pickup.
        public float resourcePerSec; //ammunition regeneration, 0 if no regen

        [Range(0f, 60f)]
        public float timePerReload;//time to reload gun
        [Range(0f, 60f)]
        public float timePerBullet;//time for each ammunition reloaded. allows partial reload

        [Range(0f, 60f)]
        public float timePerEmptyClip;//if resource goes below 0, time before reload or gain works //TODO clarify

        [Range(0, 100)]
        public int maxProjectiles;//max number of projectiles in existance, 0 if no limit, must be 1 for some systems to work
        public bool firingReplaceOld;//if firing should replace the oldest projectile, useful for turrets
    }

    [System.Serializable]
    public struct RecoilData
    {
        public Vector2 constant;
        public Vector2 random;
    }

    [System.Serializable]
    public struct CosmeticData
    {
        /*public bool line_renderer;
        public bool particles;
        public float line_thickness;
        public float line_length;
        public float particle_amount;
        public Gradient line_colors;
        public Gradient particle_colors;
        public string sound_effect;
        [Range(0f, 1f)]
        public float sound_volume;*/
    }

    [System.Serializable]
    public struct EffectData
    {
        [Range(0, 1000)]
        public int buildup;//number of hit required for damage to be applied. damage will be multiplied by this number. if <2 ignored

        public float damage;
        [Range(0f, 600f)]
        public float damageDuration;
        //public float headshot_multiplier;
        public float freeze;
        [Range(0f, 600f)]
        public float freezeDuration;

        public float blindness;
        [Range(0f, 600f)]
        public float blindnessDuration;

        public float knockback;//force applied from effect source to outside, can be negative.
    }

    [System.Serializable]
    public struct TrajectoryData
    {
        //public float spin_angle_increment;
        //public float spin_force;
        //public float spin_starting_time;
        [Range(-10f, 10f)]
        public float gravity;
        [Range(0.1f, 1000f)]
        public float lifetime;
        [Range(0.1f, 1000f)]
        public float lifetimeVariation;
        [Range(0, 100)]
        public int bounceCount;
        //public float acceleration;
        //public bool follow_player;
        //public MovementType move_type;
    }

    [System.Serializable]
    public struct BehaviourData
    {
        //public WallBehaviour wall_hit;
        //public PlayerBehaviour player_hit;
        //public HitboxBehaviour hitbox_hit;
        //public HitboxType hitbox_type;
        //public float trigger_collision_force;
        //public float trigger_collision_angle;

        public enum SeekType
        {
            None,
            Player,
            Missiles,
        }

        public enum WallBehaviour
        {
            Destruction,//destroyed on impact, explodes if explode on death
            Stop,
            Slide,//will attempt to continue on trajectory
            Ignore
        }

        public enum PlayerBehaviour
        {
            Destruction = 0,
            IgnoreFriend = 1,
            IgnoreEnemy = 2,
        }
    }
}

