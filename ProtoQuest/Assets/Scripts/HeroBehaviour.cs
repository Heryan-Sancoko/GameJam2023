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
            Debug.LogError("wat");
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
            shovelGravity = (!IsFlying) ? localDown.forward * 10 : Vector3.zero;

            rbody.useGravity = false;

            if (pushingObject != null)
            {
                rbody.velocity = Vector3.zero;

                if (pushingObject.isRoutineNull == true)
                    pushingObject = null;

                //movementInput = Vector3.zero;
                return;
            }
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

                bool wallInFront = false;

                //crawl up a wall in front of you
                if (Physics.Raycast(transform.position, (antennaeObj.transform.position - transform.position).normalized, out RaycastHit hit, Vector3.Distance(transform.position, antennaeObj.position), groundedMask))
                {
                    //transform.position = hit.point + (hit.normal * 0.5f);
                    localDown.transform.position = hit.point + hit.normal;
                    localDown.transform.LookAt(hit.point, localDown.transform.up);
                    wallInFront = true;
                }

                //check if there is floor under your feet
                if (Physics.SphereCast(transform.position + (localDown.forward * -0.5f), groundCastRadius, localDown.forward, out RaycastHit hitinfo, groundCastDist, groundedMask))
                {
                    lastGroundedPosition = transform.position;
                    isGrounded = true;
                }
                //if not, then check for a cliff face to cling to
                else if (Physics.SphereCast(transform.position + (localDown.forward * 0.5f) + ((antennaeObj.position - transform.position).normalized), groundCastRadius, (transform.position - antennaeObj.position).normalized, out RaycastHit hitinfoTwo, groundCastDist * 3, groundedMask))
                {

                    //if (Physics.OverlapSphere(hitinfoTwo.point + (hitinfoTwo.normal * 0.5f), 0.01f).Length == 0)
                    //{
                    //    rbody.velocity = Vector3.zero;
                    //    transform.position = hitinfoTwo.point + (hitinfoTwo.normal * 0.5f);
                    //    localDown.transform.position = hitinfoTwo.point + hitinfoTwo.normal;
                    //    localDown.transform.LookAt(hitinfoTwo.point, localDown.transform.up);
                    //}
                    ////else if you are nowhere near a wall, just fall down.
                    //else
                    if (!wallInFront)
                    {
                        Vector3[] cardinalDirections = { localDown.forward, Vector3.up, (Vector3.up + Vector3.left).normalized, Vector3.left, (Vector3.left + Vector3.down).normalized, Vector3.down, (Vector3.down + Vector3.right).normalized, Vector3.right, (Vector3.right + Vector3.up).normalized };

                        foreach (Vector3 direction in cardinalDirections)
                        {
                            if (Physics.Raycast(transform.position, direction, out RaycastHit emergencyRay, 1.5f, groundedMask))
                            {
                                transform.position = emergencyRay.point + (emergencyRay.normal * 0.5f);
                                rbody.velocity = Vector3.zero;
                                localDown.transform.position = emergencyRay.point + emergencyRay.normal;
                                localDown.transform.LookAt(emergencyRay.point, localDown.transform.up);
                                break;
                            }
                        }

                        Debug.LogError("Last grounded pos: " + lastGroundedPosition);
                        transform.position = lastGroundedPosition;
                    }

                }
                else
                {
                    Debug.LogError("Last grounded pos: " + lastGroundedPosition);
                    if (lastGroundedPosition != Vector3.zero)
                        transform.position = lastGroundedPosition;
                }

                if (CheckIfInDirt(transform.position))
                {
                    IsFlying = true;
                }

            }
            else
            {
                if (!isAntennaeInDirt)
                {

                    lastGroundedPosition = transform.position;
                    if (Physics.Raycast(antennaeObj.position, (transform.position - antennaeObj.position).normalized, out RaycastHit hitinfo, Vector3.Distance(transform.position, antennaeObj.position), groundedMask))
                    {
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
                            localDown.transform.position = hitinfo.point + hitinfo.normal;
                            localDown.transform.LookAt(hitinfo.point, localDown.transform.up);
                            isGrounded = true;
                            IsFlying = false;
                            Physics.IgnoreLayerCollision(Constants.Layers.GroundLayer, Constants.Layers.ShovelLayer, false);
                            Physics.IgnoreLayerCollision(Constants.Layers.VinesLayer, Constants.Layers.ShovelLayer, false);
                        }
                    }
                }


                //if (Physics.SphereCast(antennaeObj.transform.position, groundCastRadius, (transform.position - antennaeObj.transform.position).normalized, out RaycastHit hitinfo, groundCastDist, groundedMask))
                //{
                //    if (!isAntennaeInDirt)
                //    {
                //            localDown.transform.position = hitinfo.point + hitinfo.normal;
                //            localDown.transform.LookAt(hitinfo.point, localDown.transform.up);
                //            isGrounded = true;
                //            isFlying = false;
                //            transform.position = antennaeObj.transform.position;
                //            Physics.IgnoreLayerCollision(Constants.Layers.GroundLayer, Constants.Layers.ShovelLayer, false);
                //    }
                //}
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
                        antennaeObj.transform.position = transform.position;
                        transform.position = transform.position + localDown.forward;
                        IsFlying = true;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (myHerotype == heroType.shovel)
        {
            if (collision.gameObject.layer == Constants.Layers.PushBlockLayer)
            {
                if (collision.gameObject.TryGetComponent(out PushBlock pushblock))
                {
                    pushblock.PushThisBlock((collision.transform.position - lastGroundedPosition).normalized);
                    pushingObject = pushblock;
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
    }
}