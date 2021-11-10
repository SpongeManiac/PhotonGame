using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SkyboxManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Material skyboxmat;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(skyboxmat);
        }
        else
        {
            this.skyboxmat = (Material)stream.ReceiveNext();
            RenderSettings.skybox = skyboxmat;
        }
    }
}
