
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();

            }

            GUILayout.EndArea();
        }

        static void StartButtons()
        {
            //if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            //if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            //if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        IEnumerator StartMultiplayer()
        {
            while(NetworkManager.Singleton == null)
                yield return null;

            if (Application.platform == RuntimePlatform.WindowsPlayer)
                NetworkManager.Singleton.StartHost();
            else if (Application.isPlaying)
                NetworkManager.Singleton.StartClient();
            else
                NetworkManager.Singleton.StartClient();
        }
        void OnEnable()
        {
            StartCoroutine(StartMultiplayer());
        }
    }
}