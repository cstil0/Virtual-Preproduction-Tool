
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        public GameObject HarryPrefab;
        private void Start()
        {

            //NetworkManager.Singleton.StartClient();
            NetworkManager.Singleton.StartHost();


            RotationScale rotationScale = HarryPrefab.GetComponentInChildren<RotationScale>();
            Vector3 scale = new Vector3(rotationScale.scale, rotationScale.scale, rotationScale.scale);

            GameObject objectInstance = Instantiate(HarryPrefab);
            objectInstance.transform.position = new Vector3(0.0f, 0.0f, -10f);
            objectInstance.GetComponent<NetworkObject>().Spawn();
        }

    }
}