
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        private void Start()
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor)
            {
                NetworkManager.Singleton.StartClient();
            }
            else
            {
                NetworkManager.Singleton.StartHost();
            }

            //NetworkManager.Singleton.StartClient();
        }

    }
}