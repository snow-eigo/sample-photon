using UnityEngine;
using UnityEngine.UI;

public class SampleShowPhoton : MonoBehaviour
{
    public Text showPhotonId;


    private void Start()
    {
        showPhotonId.text = $"Photon ID: {PhotonNetwork.PhotonServerSettings.AppID}";
    }
}
