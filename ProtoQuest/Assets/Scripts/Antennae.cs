using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antennae : MonoBehaviour
{
    public HeroBehaviour mhero;

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("EXITING TRIGGER");

        Collider[] colliders;

        colliders = Physics.OverlapSphere(transform.position, 0.01f);

        if (colliders.Length > 0)
        {
            mhero.isAntennaeInDirt = true;
            Debug.Log(colliders[0].name);

            if (colliders[0].gameObject == gameObject && colliders.Length <= 1)
            {
                Debug.LogError("==============================" + colliders.Length);
                //mhero.IsFlying = false;
                mhero.isAntennaeInDirt = false;
                mhero.transform.position = mhero.transform.position;
            }
        }
        else
        {
            Debug.LogError("++++++++++++++++++++++++++++++");
            //mhero.IsFlying = false;
            mhero.isAntennaeInDirt = false;
            mhero.transform.position = transform.position;
        }
    }   
}
