using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Appnori.Util
{
    public class PhotonSpeakerCtrl : MonoBehaviour
    {
        PhotonView photonView;
        private void Awake()
        {
            photonView = transform.GetComponent<PhotonView>();
            DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            if (photonView.CreatorActorNr != photonView.OwnerActorNr)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
