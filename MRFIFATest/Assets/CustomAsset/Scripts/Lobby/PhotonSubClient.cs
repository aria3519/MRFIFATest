using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class PhotonSubClient : IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks
{
    public LoadBalancingClient loadBalancingClient;

    public PhotonSubClient()
    {
        loadBalancingClient = new LoadBalancingClient();
        loadBalancingClient.AddCallbackTarget(this);
    }

    ~PhotonSubClient()
    {
        loadBalancingClient.RemoveCallbackTarget(this);
    }

    public void ServiceClient()
    {
        if (loadBalancingClient.IsConnected)
        {
            loadBalancingClient.Service();
        }
    }

    public void Connect()
    {
#if SERVER_CHINA
        loadBalancingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
        loadBalancingClient.AppVersion = PhotonNetwork.NetworkingClient.AppVersion;
        loadBalancingClient.MasterServerAddress = "47.94.234.45:5055";
        loadBalancingClient.ConnectToMasterServer();
        //loadBalancingClient.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);    
#else
        loadBalancingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
        loadBalancingClient.AppVersion = PhotonNetwork.NetworkingClient.AppVersion;
        //client.EnableLobbyStatistics = true;
        loadBalancingClient.ConnectToRegionMaster(PhotonNetwork.CloudRegion);
        //PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = PhotonNetwork.CloudRegion;
        //loadBalancingClient.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);    
#endif     
    }

    //public ClientState GetState()
    //{
    //    return loadBalancingClient.State;
    //}

    public bool IsConnect()
    {
        return loadBalancingClient.IsConnected;
    }

    public void Disconnect()
    {
        if (loadBalancingClient.IsConnected)
        {
            loadBalancingClient.Disconnect();
        }
    }

    void IConnectionCallbacks.OnConnected()
    {

    }

    void IConnectionCallbacks.OnConnectedToMaster()
    {
        loadBalancingClient.OpJoinLobby(null);
    }

    void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected: " + cause.ToString());
        // client is now discconnected from Photon Master Server
    }

    void IConnectionCallbacks.OnRegionListReceived(RegionHandler handler)
    {
        Debug.Log("Region List : " + handler.BestRegion);
        // client is now discconnected from Photon Master Server
    }

    void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> response)
    {

    }

    void IConnectionCallbacks.OnCustomAuthenticationFailed(string fail)
    {

    }

    void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
    {
        
    }

    void IMatchmakingCallbacks.OnCreatedRoom()
    {
        Debug.Log(this.loadBalancingClient.NickName + " On Created Room ! RoomName : " + "<color=yellow>" + this.loadBalancingClient.CurrentRoom.Name + "</color>");
    }

    void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(this.loadBalancingClient.NickName + " Create Room Fail: " + message);
    }

    void IMatchmakingCallbacks.OnJoinedRoom()
    {
        Debug.Log(this.loadBalancingClient.NickName + " On Joined Room ! RoomName : " + "<color=yellow>" + this.loadBalancingClient.CurrentRoom.Name + "</color>");
    }

    void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
    {

    }    

    void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
    {

    }    

    void IMatchmakingCallbacks.OnLeftRoom()
    {
       
    }

    void ILobbyCallbacks.OnJoinedLobby()
    {

    }

    void ILobbyCallbacks.OnLeftLobby()
    {        

    }

    void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
    {
        LobbyPhotonManager.GetInstance.RoomListUpdate(roomList);
    }

    void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        
    }    
}
