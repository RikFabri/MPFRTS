using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Unity.Netcode;
using UnityEngine.UI;
using Netcode.Transports;
using System;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private int maxConnections = 2;

    //Callbacks
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public ulong CurrentLobbyId;
    private const string HostAddressKey = "HostAddress";

    public GameObject HostButton;
    public Text LobbyNameText;

    private void Start()
    {
        if (!SteamManager.Initialized) return;

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        Debug.Log("lobby created");

        NetworkManager.Singleton.StartHost();

        string data = SteamUser.GetSteamID().ToString();
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, data);
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby");

        Debug.Log($"{callback.m_ulSteamIDLobby}");

        if(data != SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey))
        {
            Debug.LogError("it fooked", this);
        }
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Everyone
        HostButton.SetActive(false);
        CurrentLobbyId = callback.m_ulSteamIDLobby;
        LobbyNameText.gameObject.SetActive(true);
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        // Clients
        if (NetworkManager.Singleton.IsHost) return;


        Debug.Log($"{callback.m_ulSteamIDLobby}");
        string id = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        Debug.Log($"Trying to connect to {id}");

        ulong parsedId = 0;

        try
        {
            parsedId = ulong.Parse(id);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message, this);
        }


        NetworkManager.Singleton.GetComponent<SteamNetworkingSocketsTransport>().ConnectToSteamID = parsedId;
        NetworkManager.Singleton.StartClient();
    }

}
