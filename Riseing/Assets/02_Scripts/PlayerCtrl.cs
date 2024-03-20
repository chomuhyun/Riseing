using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerCtrl : MonoBehaviour
{
    float gravity = 9.8f;
    public float atkCoolDownTime = 2f;
    float nextAtkTime = 0f;
    public static int noAtks = 0;
    float lastClickTime = 0f;
    float maxComboDelay = 1f;
    private Transform targetEnemy;
    public GameObject weapons;
    public bool battle;

    [Header("Com")]
    public VariableJoystick joy;
    CharacterController characterController;
    Animator animator;
    public CinemachineVirtualCamera cvc;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cvc = GameObject.FindGameObjectWithTag("CVC").GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit1"))
        {
            animator.SetBool("hit1", false);
        }
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit2"))
        {
            animator.SetBool("hit2", false);
        }
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit3"))
        {
            animator.SetBool("hit3", false);
        }
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit4"))
        {
            animator.SetBool("hit4", false);
            noAtks = 0;
        }
        if(Time.time - lastClickTime > maxComboDelay)
        {
            noAtks = 0;
        }
        if(battle)
        {
            DetectNearestEnemy();
            if (targetEnemy != null)
            {
                Vector3 directionToEnemy = targetEnemy.position - transform.position;
                directionToEnemy.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
            }
            StartCoroutine(BattleOut());
        }
    }
    IEnumerator BattleOut()
    {
        yield return new WaitForSeconds(3f);
        battle = false;
    }
    

    void Move()
    {
        Vector2 moveinput = new Vector2(joy.Horizontal * Time.deltaTime * 1.5f, joy.Vertical * Time.deltaTime * 1.5f);
        bool ismove = moveinput.magnitude != 0;
        animator.SetBool("isRun", ismove);
        if (ismove)
        {
            Vector3 lookForward = new Vector3(cvc.transform.forward.x, 0f, cvc.transform.forward.z).normalized;
            Vector3 lookRight = new Vector3(cvc.transform.right.x, 0f, cvc.transform.right.z).normalized;
            Vector3 moveDir = lookForward * moveinput.y + lookRight * moveinput.x;

            transform.forward = moveDir;
            transform.position += moveDir * Time.deltaTime * 0.01f;
            characterController.Move(moveDir * 0.5f);

            if (!characterController.isGrounded)
            {
                Vector3 gravityVector = Vector3.down * gravity * Time.deltaTime;
            characterController.Move(gravityVector);
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                transform.position += moveDir * Time.deltaTime * 1f;
                characterController.Move(moveDir * 70f);
                animator.SetTrigger("DF");
            }
            else
            {
                characterController.Move(moveDir * 0.5f);
            } 
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("JUMP");
        }
    }

    public void Attack()
    {
        lastClickTime = Time.time;
        noAtks++;
        if(noAtks == 1)
        {
            animator.SetBool("hit1", true);
        }
        noAtks = Mathf.Clamp(noAtks, 0, 4);

        if(noAtks >=2 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit1"))
        {
            animator.SetBool("hit1", false);
            animator.SetBool("hit2", true);
        }
        if (noAtks >= 3 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit2"))
        {
            animator.SetBool("hit2", false);
            animator.SetBool("hit3", true);
        }
        if (noAtks >= 4 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit3"))
        {
            animator.SetBool("hit3", false);
            animator.SetBool("hit4", true);
        }
    }
    private void DetectNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearestDistanceSqr = Mathf.Infinity;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            Vector3 directionToEnemy = enemy.transform.position - transform.position;
            float distanceSqr = directionToEnemy.sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }
        }
        if (nearestEnemy != null)
        {
            targetEnemy = nearestEnemy.transform;
        }
    }
}
