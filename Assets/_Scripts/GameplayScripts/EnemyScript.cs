﻿using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class EnemyScript : HumanBaseScript {

    public enum EnumEnemyStates
    {
        NEUTRAL = 0,
        AGGRESS,
    }

    public EnumEnemyStates CurrentEnemyState = EnumEnemyStates.NEUTRAL;

    public Transform AggressTarget;
    private float aggressionTimer;
    private float aggressionTimerLimit;
    private const float aggressionTimerLimitMin = 1f;
    private const float aggressionTimerLimitMax = 5f;
    private float pursueRate;
    private const float pursueRateMin = 0.03f;
    private const float pursueRateMax = 0.06f;

    private float deathTimer;
    private float deathTimeLimit;
    private const float deathTimeLimitMin = 2;
    private const float deathTimeLimitMax = 4;

    //audio variables
    public AudioClip Clip_Attack1;
    public AudioClip Clip_Attack2;
    public AudioClip Clip_Attack3;
    public AudioClip Clip_Hurt;
    public AudioClip Clip_Death1;
    public AudioClip Clip_Death2;
    public AudioClip Clip_Death3;
    public AudioClip Clip_Aggress1;
    public AudioClip Clip_Aggress2;
    public AudioClip Clip_Aggress3;

    [SerializeField]
    float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others


    // Use this for initialization
    void Start () {
        audio = GetComponent<AudioSource>();

        m_Animator = GetComponent<Animator>();
        m_RigidBody = GetComponent<Rigidbody>();

        m_RigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        pursueRate = Random.Range(pursueRateMin, pursueRateMax);
             
        aggressionTimerLimit = Random.Range(aggressionTimerLimitMin, aggressionTimerLimitMax);
        aggressionTimer = Time.time;

        deathTimeLimit = Random.Range(deathTimeLimitMin, deathTimeLimitMax);

    }

    // Update is called once per frame
    void Update () {
        if (CurrentHealthState == EnumHealthState.ALIVE)
            HandleAggressionBehavior();
        else
        {
            if (Time.time - deathTimer > deathTimeLimit)
                Destroy(this.gameObject);
        }

        UpdateEnemyAnimator();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //attack player
            int rand = Random.Range(0, 4);
            switch (rand)
            {
                case 0:
                    if (SceneManager.IncrementSFX())
                        audio.PlayOneShot(Clip_Attack1, 1);
                    break;
                case 1:
                    if (SceneManager.IncrementSFX())
                        audio.PlayOneShot(Clip_Attack2, 1);
                    break;
                case 2:
                    if (SceneManager.IncrementSFX())
                        audio.PlayOneShot(Clip_Attack3, 1);
                    break;
                default:
                    //no audio
                    break;

            }
        }
    }

    void HandleAggressionBehavior()
    {
        switch (CurrentEnemyState)
        {
            case EnumEnemyStates.NEUTRAL:
                if (Time.time - aggressionTimer > aggressionTimerLimit)
                {
                    //aggress player
                    CurrentEnemyState = EnumEnemyStates.AGGRESS;

                    int rand = Random.Range(0, 4);
                    switch (rand)
                    {
                        case 0:
                            if (SceneManager.IncrementSFX())
                                audio.PlayOneShot(Clip_Aggress1, 1);
                            break;
                        case 1:
                            if (SceneManager.IncrementSFX())
                                audio.PlayOneShot(Clip_Aggress2, 1);
                            break;
                        case 2:
                            if (SceneManager.IncrementSFX())
                                audio.PlayOneShot(Clip_Aggress3, 1);
                            break;
                        default:
                            //no audio
                            break;
                    }
                }
                break;
            case EnumEnemyStates.AGGRESS:
                transform.LookAt(AggressTarget);
                transform.Rotate(Vector3.up, 90f);
                transform.position = Vector3.MoveTowards(transform.position, AggressTarget.position, pursueRate);
                break;
        }
    }


    public void ReceiveDamage(float damageValue, Vector3 orginPosition)
    {
        health -= damageValue;

        health = 0;    //TODO: watch out for this.  handle if we want enemies to handle some damage (like bosses);

        if (health <= 0)
        {
            health = 0;
            CurrentHealthState = EnumHealthState.DEAD;
            m_RigidBody.constraints = RigidbodyConstraints.None;
            m_Dead = true;
            deathTimer = Time.time;
            int rand = Random.Range(0, 4);
            switch (rand)
            {
                case 0:
                    if(SceneManager.IncrementSFX())
                        audio.PlayOneShot(Clip_Death1, 1);
                    break;
                case 1:
                    if (SceneManager.IncrementSFX())
                        audio.PlayOneShot(Clip_Death2, 1);
                    break;
                case 2:
                    if (SceneManager.IncrementSFX())
                        audio.PlayOneShot(Clip_Death3, 1);
                    break;
                default:
                    //no audio
                    break;
            }
            GetComponent<Rigidbody>().AddForceAtPosition(Vector3.Normalize(transform.position - orginPosition) * damageValue, orginPosition);
            tag = "Untagged";
        }
    }

    void UpdateEnemyAnimator()
    {
        // update the animator parameters
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        m_Animator.SetBool("Damaged", m_Damaged);
        m_Animator.SetBool("Dead", m_Dead);
        m_Animator.SetBool("Attacking", m_Attacking);

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);

        // don't use that while airborne
        m_Animator.speed = 1;
    }

}
