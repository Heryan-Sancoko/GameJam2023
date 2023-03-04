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

        if (myHerotype == heroType.shovel)
        {
            rbody.useGravity = !isFlying;
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
        //}

    }

    private void GroundedCheck()
    {
        if (hMan.SelectedHero != myHerotype)
            return;

        groundTestSphere.transform.localScale = Vector3.one * groundCastRadius;
        if (movementInput != Vector3.zero)
        {
            //Vector3 upVec = (myHerotype == heroType.shovel && IsFlying) ? movementInput : Vector3.up;
            Vector3 upVec = movementInput;
            originTestSphere.transform.position = transform.position + (upVec);

            if (myHerotype == heroType.shovel)
            {
                if (IsFlying)
                {
                    downDir = (transform.position - originTestSphere.transform.position).normalized;
                }
                else
                    downDir = Vector3.down;
            }


            if (Physics.SphereCast(transform.position + (upVec), groundCastRadius, downDir, out RaycastHit hitinfo, groundCastDist, groundedMask))
            {
                burrowPoint = hitinfo.point - (hitinfo.normal);
                if (burrowPointSphere!=null)
                burrowPointSphere.transform.position = burrowPoint;
                groundTestSphere.transform.position = hitinfo.point + (upVec * (groundCastRadius * 0.5f));
                isGrounded = true;
                IsFlying = false;
            }
            else
            {
                groundTestSphere.transform.position = transform.position + (downDir * groundCastDist);
                isGrounded = false;
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
                        transform.position = burrowPoint;
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