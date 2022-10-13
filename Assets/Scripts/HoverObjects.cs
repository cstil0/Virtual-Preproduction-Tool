using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverObjects : MonoBehaviour
{
    bool alreadyTriggered;
    GameObject currentCollider;

    // recursive function that iterates through all materials of the tree and changes their color
    private void changeColorMaterials(GameObject currentParent, Color color)
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
                    material.color = color;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }

            // recursive call to check also for childs
            if (currChild.transform.childCount > 0)
                changeColorMaterials(currChild, color);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // change the color only to the first object that collided with the controller, only if it is an item
        if (!alreadyTriggered && other.gameObject.layer == 10)
        {
            FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
            bool isSelected = false;
            if (followPath != null)
            {
                followPath.triggerOn = true;
                isSelected = followPath.isSelected;
            }

            if (!isSelected)
            {
                alreadyTriggered = true;
                currentCollider = other.gameObject;
                changeColorMaterials(currentCollider, Color.blue);

                // if the object has a limit rotation script mark it as selected
                try
                {
                    currentCollider.GetComponent<LimitRotation>().objectSelected(gameObject);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // selecting the object to define a path can happen whenever the hand is triggering it that's why we check it here
        if (other.gameObject.layer == 10)
        {
            FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
            Color color = followPath.isSelected ? new Color(0.5176471f, 0.7504352f, 0.8078431f) : Color.blue;
            if (followPath != null)
            {
                changeColorMaterials(currentCollider, color);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // change the color to white tho the first collider
        if (other.gameObject == currentCollider)
        {
            FollowPath followPath = other.gameObject.GetComponent<FollowPath>();
            bool isSelected = false;
            if (followPath != null)
            {
                followPath.triggerOn = false;
                isSelected = followPath.isSelected;
                alreadyTriggered = false;
            }
            if (!isSelected)
            {
                alreadyTriggered = false;
                currentCollider = other.gameObject;
                changeColorMaterials(currentCollider, Color.white);
            }

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
