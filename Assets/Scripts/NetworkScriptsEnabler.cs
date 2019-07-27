using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class NetworkScriptsEnabler : NetworkBehaviour
{
    NetworkIdentity networkIdentity;
    [SerializeField]
    Behaviour[] objectsToEnableIfPlayer;
    public bool isLocalPlayer = false;

    void Start()
    {
        Cursor.visible = false;
        networkIdentity = GetComponent<NetworkIdentity>();
        if (networkIdentity.isLocalPlayer)
        {
            isLocalPlayer = true;
            foreach (Behaviour b in objectsToEnableIfPlayer)
            {
                b.enabled = true;
            }
        }

        int thisPlayerID = int.Parse(networkIdentity.netId.ToString());

        if (thisPlayerID == -1)
            SetColor(Color.red);
        if (thisPlayerID == 2)
            SetColor(Color.black);
        if (thisPlayerID == 3)
            SetColor(Color.green);
        if (thisPlayerID == 4)
            SetColor(Color.yellow);
        if (thisPlayerID == 5)
            SetColor(Color.cyan);
        if (thisPlayerID >= 6)
            SetColor(Color.blue);
    }

    void SetColor(Color colorToUse)
    {
        this.GetComponentInChildren<SpriteRenderer>().color = colorToUse;
    }

    /*private void Update()
    {
        print(networkIdentity.playerControllerId + ">" + networkIdentity.isClient);
    }*/
}
