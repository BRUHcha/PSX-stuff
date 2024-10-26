using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Unity.Netcode;

public class UINetworkManager : MonoBehaviour
{
    public void StartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            gameObject.SetActive(false);
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            gameObject.SetActive(false);
        }

    }
}
