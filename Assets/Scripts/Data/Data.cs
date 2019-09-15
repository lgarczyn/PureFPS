using UnityEngine;
using System.Collections.Generic;

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
        Shield,//Deploy shield on Player, or on subweapon
    }

    [System.Serializable]
    public class WeaponData
    {
        public WeaponType type;

        [ConditionalField("type", true, WeaponType.Shield)]
        public EffectData effect;

        //Ignored unless type == shield
        [ConditionalField("type", false, WeaponType.Shield)]
        public ShieldData shield;

        //Ignored unless type == explosion
        [ConditionalField("type", false, WeaponType.Explosion)]
        public ExplosionData explosion;

        //Ignored unless type == gun
        [ConditionalField("type", false, WeaponType.Gun)]
        public AimData aim;
        [ConditionalField("type", false, WeaponType.Gun)]
        public ShotData shot;
        [ConditionalField("type", false, WeaponType.Gun)]
        public RecoilData recoil;
        [ConditionalField("type", false, WeaponType.Gun)]
        public ProjectileData projectile;

        public ResourceData resource;
    }

    [System.Serializable]
    public class SubweaponData : WeaponData
    {
        public ActivationData activation;
    }

    public enum TrajectoryType
    {
        Parabolic,
        Orbital,
    }

    [System.Serializable]
    public struct ProjectileData
    {
        [Range(0f, 1f)]
        public float width;
        [Range(0f, 1000f)]
        public float velocity;
        [Range(0.1f, 1000f)]
        public float lifetime;
        [Range(0.1f, 1000f)]
        public float lifetimeVariation;

        public TrajectoryType trajectoryType;

        [ConditionalField("trajectoryType", false, TrajectoryType.Parabolic)]
        public ParabolicTrajectoryData parabolicTrajectory;
        [ConditionalField("trajectoryType", false, TrajectoryType.Orbital)]
        public OrbitalTrajectoryData orbitalTrajectory;
        public BehaviourData behaviour;

        //subprojectile
        [System.NonSerialized]
        public List<SubweaponData> subweapons;

        //cosmetic
        //public CosmeticData? hit_cosmetic;
        //public CosmeticData? move_cosmetic;
    }

    [System.Serializable]
    public enum ShieldShape
    {
        Sphere,
        LionHeart,
        Aegis,
    }

    [System.Serializable]
    public struct ShieldData
    {
        [Range(1, 10f)]
        public float size;
        public ShieldShape shape;
    }

    [System.Serializable]
    public struct ExplosionData
    {
        [Range(0.01f, 100f)]
        public float range;
        public float duration;//Ignored if explosion is activated on a moving projectile?
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
        [Range(1, 100)]
        public int count;//number of projectiles, strong cost multiplier
        //[Range(0f, 1000f)]
        //public float velocity;//muzzle velocity
        //[Range(0f, 1f)]
        //public float inheritedVelocity;//Percent of player velocity added to muzzle velocity
        [Range(0.1f, 500f)]
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
    public struct ResourceData
    {

        [Range(0, 1000)]
        public int resourceMax; //all-time ammo capacity of the weapon. 0 is infinite ammo
        [Range(0, 1000)]
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
    public struct Effect
    {
        public float value;
        public float duration;
    }

    [System.Serializable]
    public struct EffectData
    {
        [Range(0, 1000)]
        public int buildup;//number of hit required for damage to be applied. damage will be multiplied by this number. if <2 ignored

        //health based effects
        //TODO damage information (enemy multiplier, friendly-fire multiplier, shield multiplier)
        public Effect damage;
        //shielf based effect
        public Effect shield;
        public Effect blindness;
        //multiplier based effects
        public Effect speed;
        public Effect rof;


        public float knockback;//force applied from effect source to outside, can be negative.
    }

    [System.Serializable]
    public struct ParabolicTrajectoryData
    {
        [Range(0f, 180f)]
        public float cone;
        [Range(-10f, 10f)]
        public float gravity;
        [Range(0, 100)]
        public int bounceCount;
    }

    [System.Serializable]
    public struct OrbitalTrajectoryData
    {
        //0 is vertical, -90 is left to right, 90 is right to left
        [Range(-180f, 180f)]
        public float tilt;
        [Range(0f, 180f)]
        public float tiltVariation;
        [Range(1, 10)]
        public float range;

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

