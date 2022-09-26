using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverObjects : MonoBehaviour
{
    bool alreadyTriggered;
    GameObject currentCollider;

    // recursive function that iterates through all materials of the tree and changes their color
    private void changeColorMaterials(GameObject currentParent, bool triggerEnter)
    {
        for (int i = 0; i < currentParent.transform.childCount; i++)
        {
            GameObject currChild = currentParent.transform.GetChild(i).gameObject;
            
            // not all childs have materials, so this is why we need to catch errors
            try
            {
                // get all materials and change their color
                Renderer renderer = currChild.GetComponent<Renderer>();
                Material[] materials = renderer.materials;

                foreach (Material material in materials)
                {
                    material.color = triggerEnter ? Color.blue : Color.white;

                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }

            // recursive call to check also for childs
            if (currChild.transform.childCount > 0)
                changeColorMaterials(currChild, triggerEnter);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // change the color only to the first object that collided with the controller, only if it is an item
        if (!alreadyTriggered && other.gameObject.layer == 10)
        {
            alreadyTriggered = true;
            currentCollider = other.gameObject;
            changeColorMaterials(currentCollider, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // change the color to white tho the first collider
        if (other.gameObject == currentCollider)
        {
            alreadyTriggered = false;
            currentCollider = other.gameObject;
            changeColorMaterials(currentCollider, false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        alreadyTriggered = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
