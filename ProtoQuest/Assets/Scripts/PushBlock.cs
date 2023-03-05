using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBlock : MonoBehaviour
{
    private Coroutine pushRoutine;

    public bool isRoutineNull;

    private Rigidbody rbody;
    private HeroManager hMan;
    [SerializeField]
    private LayerMask vineLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        hMan = HeroManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PushThisBlock(Vector3 pushDir)
    {
        Debug.LogError("PUSHING BLOCK");

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
            rbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            pushRoutine = StartCoroutine(PushRoutine(closestDirection));
        }
    }

    private IEnumerator PushRoutine(Vector3 pushDir)
    {
        isRoutineNull = false;
        rbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rbody.isKinematic = true;
        rbody.useGravity = false;
        Vector3 newPos = transform.position + (pushDir * 2);
        while (transform.position != newPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, newPos, 0.1f);
            if (Vector3.Distance(transform.position, newPos) < 0.05f)
                transform.position = newPos;
            yield return new WaitForFixedUpdate();
        }
        rbody.isKinematic = false;
        rbody.useGravity = true;

        RaycastHit[] hitColliders = Physics.BoxCastAll(transform.position + (Vector3.forward*-1), Vector3.one, Vector3.forward, Quaternion.identity, 2, vineLayerMask);

        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Debug.LogError("VINES DETECTED");
                //rbody.isKinematic = true;
                rbody.useGravity = false;
                rbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            }
        }
        else
            rbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;


        rbody.velocity = Vector3.zero;

        Vector3 roundedPos = transform.position;
        roundedPos.x = Mathf.RoundToInt(roundedPos.x);
        roundedPos.y = Mathf.RoundToInt(roundedPos.y);
        roundedPos.z = 0;

        transform.position = roundedPos;
        HeroManager.instance.shovelBehaviour.pushingObject = null;
        isRoutineNull = true;
        pushRoutine = null;
        yield return null;
    }

}
