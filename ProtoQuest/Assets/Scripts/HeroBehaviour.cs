using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class HeroBehaviour : MonoBehaviour
{

    public Vector3 movementInput;
    public enum heroType {sword, shovel};
    public heroType myHerotype;
    public float moveSpeed;
    public float jumpSpeed;
    public float throwSpeed;
    public GameObject heldObject;
    public PushBlock pushingObject;
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
                Physics.IgnoreLayerCollision(Constants.Layers.VinesLayer, Constants.Layers.ShovelLayer, value);
            }
        }
    }

    [SerializeField]
    private Rigidbody rbody;
    [SerializeField]
    private LayerMask groundedMask;
    [SerializeField]
    private LayerMask walkableSurfaceMask;
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
    private Vector3 lastGroundedPosition;

    public bool isjumping = false;

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
            rbody.useGravity = !IsFlying;

            if (pushingObject != null)
            {
                rbody.velocity = Vector3.zero;

                if (pushingObject.isRoutineNull == true)
                    pushingObject = null;

                return;
            }
        }

        if (!isjumping)
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
                Vector3 flatvel = movementInput.normalized * (moveSpeed * 0.5f);
                flatvel.y = rbody.velocity.y;

                rbody.velocity = flatvel;
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

            if (IsFlying)
            {
                if (!CheckIfInDirt(antennaeObj.position))
                {

                    lastGroundedPosition = transform.position;
                    bool isThereRock = false;

                    List<Collider> colliders1 = Physics.OverlapSphere(antennaeObj.position, 0.05f).ToList();

                    if (colliders1.Count > 0)
                    {
                        foreach (Collider col in colliders1)
                        {
                            if (col.gameObject.layer == Constants.Layers.WallLayer ||
                                col.gameObject.layer == Constants.Layers.GroundLayer ||
                                col.gameObject.layer == Constants.Layers.VinesLayer)
                            {
                                isThereRock = true;
                            }
                        }
                    }


                    if (!isThereRock)
                    {
                        transform.position = antennaeObj.transform.position;
                        isGrounded = true;
                        IsFlying = false;
                        Physics.IgnoreLayerCollision(Constants.Layers.GroundLayer, Constants.Layers.ShovelLayer, false);
                        Physics.IgnoreLayerCollision(Constants.Layers.VinesLayer, Constants.Layers.ShovelLayer, false);
                    }
                }
            }
            else
            {
                if (Physics.SphereCast(transform.position + (Vector3.up * 0.5f), groundCastRadius, Vector3.down, out RaycastHit hit, groundCastDist, walkableSurfaceMask))
                {
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }
            }

            isAntennaeInDirt = CheckIfInDirt(antennaeObj.position);
        }
    }

    public bool CheckIfInDirt( Vector3 pos)
    {
        Collider[] colliders1;

        colliders1 = Physics.OverlapSphere(pos, 0.05f, groundedMask);

        if (colliders1.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
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

                Vector3 upvel = Vector3.up;

                if (myHerotype == heroType.shovel)
                {
                    rbody.velocity = localDown.forward * (-jumpSpeed);
                    StartCoroutine(JumpRoutine());
                }

                if (myHerotype == heroType.sword)
                    rbody.velocity = flatvel + (upvel * jumpSpeed);
                else
                    rbody.velocity = rbody.velocity + (localDown.up * jumpSpeed);
            }
            else
            {
                if (myHerotype == heroType.sword)
                IsFlying = !IsFlying;
            }
        }
    }

    public IEnumerator JumpRoutine()
    {
        isjumping = true;
        float jumptimer = 0.5f;
        while (jumptimer > 0)
        {
            jumptimer -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        isjumping = false;
        yield return null;
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
                        if (isAntennaeInDirt)
                        {
                            transform.position = antennaeObj.position;
                            antennaeObj.position = transform.position;
                            IsFlying = true;
                        }
                    }
                    else
                    {
                        //return to closest point outside the wall
                        //IsFlying = false;
                    }
                    break;
                case heroType.sword:
                    break;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (myHerotype == heroType.shovel)
        {
            if (collision.gameObject.layer == Constants.Layers.PushBlockLayer)
            {
                if (Vector3.Dot(collision.contacts[0].normal, movementInput) < -0.9f)
                {
                    if (collision.gameObject.TryGetComponent(out PushBlock pushblock))
                    {
                        pushblock.PushThisBlock(movementInput.normalized);
                        pushingObject = pushblock;
                    }
                }
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
        public static int WallLayer = 7;
        public static int SwordLayer = 8;
        public static int ShovelLayer = 9;
        public static int VinesLayer = 10;
        public static int PushBlockLayer = 11;
        public static int ExitBlockLayer = 12;
        public static int BreakableBlockLayer = 13;
    }
}