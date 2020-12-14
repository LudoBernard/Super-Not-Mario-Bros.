﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCharacter : MonoBehaviour
{
    private enum State
    {
        None,
        Idle,
        Walk,
        Jump,
        Hurt
    }

    [SerializeField] private Animator anim_;
    [SerializeField] private SpriteRenderer spriteRenderer_;
    [SerializeField] private Rigidbody2D body_;
    [SerializeField] private PlayerFoot foot_;
    
    [FMODUnity.EventRef][SerializeField] private string jumpEvent_ = "";
    [FMODUnity.EventRef][SerializeField] private string pickUpGemEvent_ = "";
    [FMODUnity.EventRef][SerializeField] private string pickUpKeyEvent_ = "";
    [FMODUnity.EventRef][SerializeField] private string deathEvent_ = "";
    [FMODUnity.EventRef][SerializeField] private string landEvent_ = "";
    

    private bool facingRight_ = true;
    private bool jumpButtonDown_ = false;

    private State currentState_ = State.Idle;

    private const float DeadZone = 0.1f;
    private const float MoveSpeed = 2.5f;
    private const float JumpSpeed = 5.0f;
    
    private int gems_ = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        ChangeState(State.Idle);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpButtonDown_ = true;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
        float moveDir = 0.0f;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveDir -= 1.0f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveDir += 1.0f;
        }
        if (jumpButtonDown_ && foot_.FootContact > 0)
        {
            Jump();
        }
        jumpButtonDown_ = false;
        var vel = body_.velocity;
        body_.velocity = new Vector2(MoveSpeed * moveDir, vel.y);
        if (moveDir > DeadZone && !facingRight_)
        {
            Flip();
        }
        if (moveDir < -DeadZone && facingRight_)
        {
            Flip();
        }
        switch (currentState_)
        {
            case State.Idle:
                if(Mathf.Abs(moveDir) > DeadZone)
                {
                    ChangeState(State.Walk);
                }
                if(foot_.FootContact == 0)
                {
                    ChangeState(State.Jump);
                }
                break;
            case State.Walk:
                if (Mathf.Abs(moveDir) < DeadZone)
                {
                    ChangeState(State.Idle);
                }
                if(foot_.FootContact == 0)
                {
                    ChangeState(State.Jump);
                }
                break;
            case State.Jump:
                if (foot_.FootContact > 0)
                {
                    ChangeState(State.Idle);
                    FMODUnity.RuntimeManager.PlayOneShot(landEvent_, transform.position);
                }
                break;
        }
    }

    private void Jump()
    {
        var vel = body_.velocity;
        body_.velocity = new Vector2(vel.x, JumpSpeed);
        FMODUnity.RuntimeManager.PlayOneShot(jumpEvent_, transform.position);
    }

    private void Die()
    {
        Debug.Log("You died!");
        FMODUnity.RuntimeManager.PlayOneShot(deathEvent_, transform.position);
        Destroy(gameObject);
        SceneManager.LoadScene("World1-1");
    }

    void ChangeState(State state)
    {
        switch(state)
        {
            case State.Idle:
                anim_.Play("Idle");
                break;
            case State.Walk:
                anim_.Play("Walk");
                break;
            case State.Jump:
                anim_.Play("Jump");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
        currentState_ = state;
    }

    void Flip()
    {
        spriteRenderer_.flipX = !spriteRenderer_.flipX;
        facingRight_ = !facingRight_;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Spikes"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Gems"))
        {
            gems_++;
            Destroy(other.gameObject);
            FMODUnity.RuntimeManager.PlayOneShot(pickUpGemEvent_, transform.position);
        }
        
        if (other.gameObject.CompareTag("Keys"))
        {
            Destroy(other.gameObject);
            FMODUnity.RuntimeManager.PlayOneShot(pickUpKeyEvent_, transform.position);
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Die();
        }
    }
}
