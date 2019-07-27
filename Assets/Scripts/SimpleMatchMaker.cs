using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using UnityEngine.UI;

public class SimpleMatchMaker : MonoBehaviour
{
    [SerializeField]
    GameObject loadingPanel;
    [SerializeField]
    GameObject loadingIcon;
    [SerializeField]
    Text loadingStatuts;
    [SerializeField]
    Button okButton;

    void CreateInternetMatch()
    {
        loadingStatuts.text = "Creating room...";
        CustomNetworkManager.singleton.matchMaker.CreateMatch(Application.version, 5, true, "", "", "", 0, 0, OnInternetMatchCreate);
    }

    //this method is called when your request for creating a match is returned
    private void OnInternetMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            //Debug.Log("Create match succeeded");
            loadingStatuts.text = "Entering room...";
            MatchInfo hostInfo = matchInfo;
            NetworkServer.Listen(hostInfo, 9000);

            CustomNetworkManager.singleton.StartHost(hostInfo);
        }
        else
        {
            loadingStatuts.text = "Couldn't create room";
            Debug.LogError("Create match failed");
        }
    }

    //call this method to find a match through the matchmaker
    public void FindInternetMatch()
    {
        loadingPanel.SetActive(true);
        CustomNetworkManager.singleton.StopHost();
        CustomNetworkManager.singleton.StartMatchMaker();
        loadingStatuts.text = "Looking for room...";
        CustomNetworkManager.singleton.matchMaker.ListMatches(0, 10, Application.version, true, 0, 0, OnInternetMatchList);
    }

    //this method is called when a list of matches is returned
    private void OnInternetMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        if (success)
        {
            if (matches.Count != 0)
            {
                //Debug.Log("A list of matches was returned");

                //join the last server (just in case there are two...)
                loadingStatuts.text = "Joining room...";
                CustomNetworkManager.singleton.matchMaker.JoinMatch(matches[matches.Count - 1].networkId, "", "", "", 0, 0, OnJoinInternetMatch);
            }
            else
            {
                loadingStatuts.text = "Room not found";
                CreateInternetMatch();
            }
        }
        else
        {
            Debug.LogError("Couldn't connect to match maker");
            loadingStatuts.text = "Couldn't connect";
            okButton.gameObject.SetActive(true);
            loadingIcon.SetActive(false);
        }
    }

    //this method is called when your request to join a match is returned
    private void OnJoinInternetMatch(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            //Debug.Log("Able to join a match");
            loadingStatuts.text = "Room found. Joining...";
            MatchInfo hostInfo = matchInfo;
            CustomNetworkManager.singleton.StartClient(hostInfo);
        }
        else
        {
            Debug.LogError("Join match failed");
            loadingStatuts.text = "Error joining room";
            okButton.gameObject.SetActive(true);
            loadingIcon.SetActive(false);
        }
    }
}