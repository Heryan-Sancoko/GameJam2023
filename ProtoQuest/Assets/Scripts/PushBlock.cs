using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PushBlock : MonoBehaviour
{
    private Coroutine pushRoutine;

    public bool isRoutineNull;

    private Rigidbody rbody;
    private HeroManager hMan;
    [SerializeField]
    private LayerMask vineLayerMask;
    [SerializeField]
    private LayerMask everythingBUTvinesMaks;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        hMan = HeroManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitinfo, 1.1f))
        {
            IsLandingOnBreakable(hitinfo.collider);
            Vector3 rounded = transform.position;
            rounded.y = (Mathf.RoundToInt(rounded.y*2));
            rounded.y = rounded.y * 0.5f;
            transform.position = rounded;
            rbody.isKinematic = true;
        }
        else
        {
            rbody.isKinematic = false;
        }
    }

    public void PushThisBlock(Vector3 pushDir)
    {
        Vector3[] cardinalDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        float maxDotProduct = float.MinValue;
        Vector3 closestDirection = Vector3.zero;

        foreach (Vector3 direction in cardinalDirections)
        {
            float dotProduct = Vector3.Dot(pushDir, direction);

            if (dotProduct > maxDotProduct)
            {
                maxDotProduct = dotProduct;
                closestDirection = direction;
            }
        }


        if (pushRoutine == null)
        {

            float pushMag = 2;

            if (Physics.SphereCast(transform.position,0.1f, closestDirection, out RaycastHit hit, 4, everythingBUTvinesMaks))
            {
                if (hit.collider.gameObject.layer != Constants.Layers.VinesLayer)
                {
                    float dist = Vector3.Distance(transform.position, hit.point);

                    if (dist <= 2.1f && dist >= 1.5f)
                    {
                        pushMag = 1;
                    }
                    else if (dist < 1.5f && dist >= 1f && pushMag > dist)
                    {
                        pushMag = 0.5f;

                    }
                    else if (dist < 1f && pushMag > dist)
                    {
                        pushMag = 0;
                    }

                }
            }

            if (pushMag <= 0.5f)
            {
                isRoutineNull = true;
                pushRoutine = null;
            }
            else
            {
                rbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                pushRoutine = StartCoroutine(PushRoutine(closestDirection, pushMag));
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        IsLandingOnBreakable(collision);
    }

    private bool IsLandingOnBreakable(Collision collision)
    {
        if (collision.gameObject.layer == Constants.Layers.BreakableBlockLayer || collision.gameObject.layer == Constants.Layers.ExitBlockLayer)
        {
            if (rbody.useGravity == true && transform.position.y > collision.transform.position.y && Mathf.Abs(collision.relativeVelocity.y) > 1)
            {
                if (collision.gameObject.TryGetComponent(out BreakableBlock bBlock))
                {
                    bBlock.BreakBlock();
                    return true;
                }
            }
        }
        return false;
    }


    private bool IsLandingOnBreakable(Collider collision)
    {
        if (collision.gameObject.layer == Constants.Layers.BreakableBlockLayer || collision.gameObject.layer == Constants.Layers.ExitBlockLayer)
        {
            if (rbody.useGravity == true && transform.position.y > collision.transform.position.y && rbody.velocity.y < -0.5f)
            {
                if (collision.gameObject.TryGetComponent(out BreakableBlock bBlock))
                {
                    bBlock.BreakBlock();
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator PushRoutine(Vector3 pushDir, float pushMagnitude)
    {


        isRoutineNull = false;
        rbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rbody.isKinematic = true;
        rbody.useGravity = false;
        Vector3 newPos = transform.position + (pushDir * pushMagnitude);
        while (transform.position != newPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, newPos, 0.1f);
            if (Vector3.Distance(transform.position, newPos) < 0.05f)
                transform.position = newPos;
            yield return new WaitForFixedUpdate();
        }
        rbody.isKinematic = false;
        rbody.useGravity = true;

        RaycastHit[] hitColliders = Physics.BoxCastAll(transform.position + (Vector3.forward*-1), Vector3.one*0.5f, Vector3.forward, Quaternion.identity, 2, vineLayerMask);

        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                //rbody.isKinematic = true;
                rbody.useGravity = false;
                rbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            }
        }
        else
            rbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;


        rbody.velocity = Vector3.zero;

        Vector3 roundedPos = transform.position;
        roundedPos.x = Mathf.RoundToInt(roundedPos.x*2);
        roundedPos.x /= 2;
        roundedPos.y = Mathf.RoundToInt(roundedPos.y*2);
        roundedPos.y /= 2;
        roundedPos.z = 0;

        transform.position = roundedPos;
        HeroManager.instance.shovelBehaviour.pushingObject = null;
        isRoutineNull = true;
        pushRoutine = null;
        yield return null;
    }

    

}
