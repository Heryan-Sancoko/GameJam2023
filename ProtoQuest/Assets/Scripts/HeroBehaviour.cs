using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeroBehaviour : MonoBehaviour
{

    public Vector3 movementInput;
    public enum heroType {sword, shovel};
    public heroType myHerotype;
    public float moveSpeed;
    public float jumpSpeed;
    public float throwSpeed;
    public GameObject heldObject;
    [SerializeField]
    private bool isFlying;
    public bool IsFlying
    {
        get { return isFlying; }
        set
        {
            isFlying = value;
            if (myHerotype == heroType.shovel)
            {
                Physics.IgnoreLayerCollision(Constants.Layers.GroundLayer, Constants.Layers.ShovelLayer, value);
            }
        }
    }

    [SerializeField]
    private Rigidbody rbody;
    [SerializeField]
    private LayerMask groundedMask;
    [SerializeField]
    private float groundCastRadius;
    [SerializeField]
    private float groundCastDist;
    [SerializeField]
    private GameObject groundTestSphere;
    [SerializeField]
    private GameObject originTestSphere;
    [SerializeField]
    private GameObject burrowPointSphere;

    public bool isGrounded = true;
    private HeroManager hMan;

    private Vector3 burrowPoint;

    private Vector3 downDir;

    private Vector3 shovelGravity;

    [SerializeField]
    private Transform localDown;

    public bool isAntennaeInDirt;
    public Transform antennaeObj;

    // Start is called before the first frame update
    void Start()
    {
        downDir = Vector3.down;
        hMan = HeroManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (hMan.SelectedHero != myHerotype)
            return;

        Vector3 zeroForward = transform.position;
        zeroForward.z = 0;
        transform.position = zeroForward;



        if (myHerotype == heroType.shovel)
        {
            shovelGravity = (!isFlying) ? localDown.forward * 10 : Vector3.zero;

            rbody.useGravity = false;
        }

        GroundedCheck();


        //move character
        //if (movementInput != Vector3.zero)
        //{
        if (IsFlying)
        {
            if (myHerotype == heroType.shovel)
                rbody.useGravity = false;
            rbody.velocity = movementInput.normalized * (moveSpeed * 0.5f);
        }
        else
        {
            if (myHerotype != heroType.shovel)
            {
                rbody.useGravity = true;
                Vector3 flatvel = movementInput.normalized * (moveSpeed * 0.5f);
                flatvel.y = 0;

                if (isGrounded && rbody.velocity.y <= 0)
                    rbody.velocity = flatvel;
                else
                {
                    Vector3 fallingVel = Vector3.up * rbody.velocity.y;
                    rbody.velocity = flatvel + fallingVel;
                }
            }
            else
            {
                if (Mathf.Abs(Vector3.Dot(movementInput.normalized, localDown.right)) > 0.9f)
                {

                    Vector3 freeVel = movementInput.normalized * (moveSpeed);
                    freeVel += shovelGravity;
                    rbody.velocity = freeVel;
                }
                else
                {
                    rbody.velocity = shovelGravity;
                }
            }
        }
        //}

    }

    private void GroundedCheck()
    {
        if (hMan.SelectedHero != myHerotype)
            return;

        if (myHerotype == heroType.shovel)
        {
            if (movementInput != Vector3.zero)
            antennaeObj.transform.position = transform.position + movementInput;

            if (!IsFlying)
            {

                if (Physics.Raycast(transform.position, movementInput, out RaycastHit hit, Vector3.Distance(transform.position, antennaeObj.position), groundedMask))
                {
                    //transform.position = hit.point + (hit.normal * 0.5f);
                    localDown.transform.position = hit.point + hit.normal;
                    localDown.transform.LookAt(hit.point, localDown.transform.up);
                }

                if (Physics.SphereCast(transform.position + (localDown.forward *-0.5f) + (transform.position - antennaeObj.position).normalized, groundCastRadius, localDown.forward, out RaycastHit hitinfo, groundCastDist, groundedMask))
                {
                    isGrounded = true;
                }
                else if (Physics.SphereCast(transform.position + (localDown.forward*0.5f) + ((antennaeObj.position - transform.position).normalized*0.5f), groundCastRadius, (transform.position - antennaeObj.position).normalized, out RaycastHit hitinfoTwo, groundCastDist*2, groundedMask))
                {
                    Debug.LogError("UNDETECTED GROUND");
                    
                    if (Physics.OverlapSphere(hitinfoTwo.point + (hitinfoTwo.normal * 0.5f), 0.01f).Length == 0)
                    {
                        transform.position = hitinfoTwo.point + (hitinfoTwo.normal * 0.5f);
                        localDown.transform.position = hitinfoTwo.point + hitinfoTwo.normal;
                        localDown.transform.LookAt(hitinfoTwo.point, localDown.transform.up);
                    }

                }
            }
            else
            {
                if (Physics.SphereCast(antennaeObj.transform.position, groundCastRadius, (transform.position - antennaeObj.transform.position).normalized, out RaycastHit hitinfo, groundCastDist, groundedMask))
                {
                    if (!isAntennaeInDirt)
                    {
                        Collider[] colliders;

                        colliders = Physics.OverlapSphere(transform.position, 0.01f);

                        if (colliders.Length <=1)
                        {
                            Debug.LogError("WHY");
                            localDown.transform.position = hitinfo.point + hitinfo.normal;
                            localDown.transform.LookAt(hitinfo.point, localDown.transform.up);
                            isGrounded = true;
                            isFlying = false;
                            transform.position = antennaeObj.transform.position;
                            Physics.IgnoreLayerCollision(Constants.Layers.GroundLayer, Constants.Layers.ShovelLayer, false);
                        }
                    }
                }
            }
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();

    }

    public void GrabObject(InputAction.CallbackContext context)
    {
        if (myHerotype == heroType.sword)
        {
            
        }
    }

    public void SwapCharacters(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            hMan.SwapCharacters();
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (hMan.SelectedHero != myHerotype)
            return;

        if (context.phase == InputActionPhase.Started)
        {
            if (isGrounded)
            {
                Debug.Log("JUMP!");
                Vector3 flatvel = rbody.velocity;
                flatvel.y = 0;
                rbody.velocity = flatvel + (Vector3.up * jumpSpeed);
            }
            else
            {
                if (myHerotype == heroType.sword)
                IsFlying = !IsFlying;
            }
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (hMan.SelectedHero != myHerotype)
            return;

        if (context.phase == InputActionPhase.Started)
        {
            switch (myHerotype)
            {
                case heroType.shovel:
                    //dig into the dirt
                    if (!IsFlying)
                    {
                        transform.position = transform.position + localDown.forward;
                        IsFlying = true;
                    }
                    else
                    {
                        //return to closest point outside the wall
                        IsFlying = false;
                    }
                    break;
                case heroType.sword:
                    break;
            }
        }
    }

}

public static class Constants
{
    public struct Layers
    {
        public static int HeroLayer = 3;
        public static int GroundLayer = 6;
        public static int SwordLayer = 8;
        public static int ShovelLayer = 9;
    }
}