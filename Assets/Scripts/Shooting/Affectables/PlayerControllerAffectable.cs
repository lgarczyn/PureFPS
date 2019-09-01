using UnityEngine;
public class PlayerControllerAffectable : RigidBodyAffectable, IAffectable
{
    public ADisplayRatio healthbar;
    public PlayerController controller;
    new public void Update()
    {
        base.Update();
        controller.speedMultiplier = this.speedMult;
        healthbar.SetRatio((this.health + this.shield) / 100f);
    }
}
