﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Movement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    //public bool wallSlide;
    public bool isDashing;

    [Space]

    private bool groundTouch;
    private bool hasDashed;

    public int side = 1;

    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rb.gravityScale);

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(xRaw, yRaw);

        //Walk(dir);
        if (!canMove)
            return;

        //if (wallGrab)
        //    return;

        

        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        if (coll.onWall && Input.GetButton("Jump") && canMove)
        {
            if (coll.wallSide == 1 && Input.GetAxis("Horizontal") < 0 || coll.wallSide == -1 && Input.GetAxis("Horizontal") > 0)
            {
                if (side != coll.wallSide)
                    anim.Flip(side * -1);
                wallGrab = true;
                //wallSlide = false;
            }
        }

        if (!coll.onWall || !canMove)
        {
            wallGrab = false;
            //wallSlide = false;
        }

        //if (Input.GetButton("Jump") && coll.onWall)
        //{
            
        //}

        if (coll.onGround)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }

        if (Input.GetButton("Jump") && rb.gravityScale == 0)
        {
            if (coll.wallSide == 1 && Input.GetAxis("Horizontal") > 0 || coll.wallSide == -1 && Input.GetAxis("Horizontal") < 0)
            {
                if (coll.onWall && !coll.onGround)
                    WallJump();
            }
        }

        if (!wallJumped)
        {
            Vector2 test = new Vector2(dir.x * speed, rb.velocity.y);
            rb.velocity = test;
        }
        else
        {
            //Debug.Log(rb.velocity.y);
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }

        if (wallGrab)
        {
            rb.gravityScale = 0;
            if(x > .2f || x < -.2f)
            rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;
            if (coll.wallSide == 1 && Input.GetAxis("Horizontal") > 0 || coll.wallSide == -1 && Input.GetAxis("Horizontal") < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
            }
            else
            {
                rb.velocity = new Vector2(0, y * (speed * speedModifier));
            }
            
        }
        else
        {
            rb.gravityScale = 3;
        }

        if (coll.onWall && !coll.onGround)
        {
            //if (x != 0 && !wallGrab)
            //{
            //    wallSlide = true;
            //    WallSlide();
            //}
        }

        //if (!coll.onWall || coll.onGround)
            //wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            anim.SetTrigger("jump");

            if (coll.onGround)
                Jump(Vector2.up, false);
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed)
        {
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallGrab || !canMove)
            return;

        if(x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }


    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    private void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;
        Vector2 jumpDir = (((Vector2.up*10f) / 1.5f) + wallDir / 1.5f);
        //Vector2 tempDir = new Vector2(wallDir.x, 20);
        Jump(wallDir + Vector2.up, true);
        //Jump(tempDir, true);

        wallJumped = true;

        Debug.Log("WallJump");
    }

    private void WallSlide()
    {
        if(coll.wallSide != side)
         anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    //private void Walk(Vector2 dir)
    //{
    //    if (!canMove)
    //        return;

    //    if (wallGrab)
    //        return;

    //    if (!wallJumped)
    //    {
    //        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
    //    }
    //    else
    //    {
    //        rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
    //    }
    //}

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;
        Debug.Log(dir * jumpForce);
        Debug.Log("Before = " + rb.velocity);
        //rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        rb.velocity += dir * jumpForce;
        Debug.Log("After = " + rb.velocity);
        particle.Play();
        
        
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallGrab && vertical < 0)
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
}
