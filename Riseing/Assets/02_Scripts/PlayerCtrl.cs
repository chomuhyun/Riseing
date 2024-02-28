using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerCtrl : MonoBehaviour
{
    float gravity = 9.8f;

    [Header("Com")]
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
        Attack();
    }

    void Move()
    {
        Vector2 moveinput = new Vector2(Input.GetAxis("Horizontal") * Time.deltaTime * 1.5f, Input.GetAxis("Vertical") * Time.deltaTime * 1.5f);
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

            if(Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.Space))
            {
                animator.SetTrigger("DF");
            }
        }
    }

    void Attack()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("ATTACK");
        }
    }
}
