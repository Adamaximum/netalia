﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingEnemyScript : MonoBehaviour
{
    private Rigidbody2D rb;
    
    //raycasting
    public float detectionRange;
    private Vector2 dir;
    
    //charging
    public float chargeSpeed;
    private float retreatSpeed;
    public float chargeDist;
    private Vector2 endCoords;
    private Vector2 startCoords;
    private bool charging;
    private bool retreating;
    
    void Start()
    {
        dir = gameObject.GetComponent<SpriteRenderer>().flipX ? Vector2.right : Vector2.left;
        rb = gameObject.GetComponent<Rigidbody2D>();

        startCoords = transform.position;

        if (!gameObject.GetComponent<SpriteRenderer>().flipX)
        {
            endCoords = new Vector2(startCoords.x - chargeDist, startCoords.y);
        }
        else
        {
            endCoords = new Vector2(startCoords.x + chargeDist, startCoords.y);
        }

        retreatSpeed = chargeSpeed / 4;
    }

    private void Update()
    {
        if (charging)
            CheckRetreat();
        
        if (retreating)
            CheckStop();
        
        Debug.Log("charging: " + charging);
        Debug.Log("retreating: " + retreating);
    }


    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(startCoords, dir, detectionRange, LayerMask.GetMask("Player"));
        Debug.DrawRay(startCoords, dir*detectionRange, Color.red);

        if (hit != null && hit.collider.tag == "Player")
        {
            rb.velocity = dir * chargeSpeed;
            Debug.Log("dir: " + dir);
            Debug.Log("chargeSpeed: " + chargeSpeed);
            charging = true;
        }
    }

    void CheckRetreat()
    {
        if (dir == Vector2.right && gameObject.transform.position.x > endCoords.x
            || dir == Vector2.left && gameObject.transform.position.x < endCoords.x)
        {
            rb.velocity = -dir * chargeDist;
            charging = false;
            retreating = true;
        }
    }

    void CheckStop()
    {
        if (dir == Vector2.right && gameObject.transform.position.x < startCoords.x
            || dir == Vector2.left && gameObject.transform.position.x > startCoords.x)
        {
            rb.velocity = new Vector2(0, 0);
            retreating = false;
        }
    }
}