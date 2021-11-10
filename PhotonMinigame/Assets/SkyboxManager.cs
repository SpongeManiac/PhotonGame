using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SkyboxManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public int skybox
    {
        get
        {
            return _skybox;
        }
        set
        {
            SetSkybox();
            _skybox = value;
        }
    }
    int _skybox;

    public Material[] skyboxes;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(skybox);
        }
        else
        {
            this.skybox = (int)stream.ReceiveNext();
        }
    }

    public void SetSkybox()
    {
        RenderSettings.skybox = skyboxes[skybox];
    }
}
