using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    Player player;
    Rigidbody2D rig;

    int groundID;
    int hangingID;
    int crouchID;
    int speedID;
    int fallID;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
        rig = GetComponent<Rigidbody2D>();

        groundID = Animator.StringToHash("isOnGround");
        hangingID = Animator.StringToHash("isHangin");
        crouchID = Animator.StringToHash("isCrouching");
        speedID = Animator.StringToHash("speed");
        fallID = Animator.StringToHash("verticalVelocity");

    }


    void Update()
    {
        anim.SetFloat(speedID, Mathf.Abs(player.Xvelocity));
        //anim.SetBool("isOnGround", player.isOnGround);
        anim.SetBool(groundID, player.isOnGround);
        anim.SetBool(hangingID, player.isHanging);
        anim.SetBool(crouchID, player.isCrouch);
        anim.SetFloat(fallID, rig.velocity.y);
    }
}
