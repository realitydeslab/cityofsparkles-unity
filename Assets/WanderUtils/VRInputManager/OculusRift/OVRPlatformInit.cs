using UnityEngine;
using Oculus.Avatar;
using Oculus.Platform;
using Oculus.Platform.Models;
using System.Collections;

public class OVRPlatformInit : MonoBehaviour {

    public OvrAvatar myAvatar;

    void Start () {
        Oculus.Platform.Core.Initialize();
        Oculus.Platform.Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        Oculus.Platform.Request.RunCallbacks();  //avoids race condition with OvrAvatar.cs Start().
    }

    private void GetLoggedInUserCallback(Message<User> message)
    {
        if (!message.IsError) {
            myAvatar = FindObjectOfType<OvrAvatar>();
            myAvatar.oculusUserID = message.Data.ID.ToString();
            Debug.Log("Oculus logged-in user: " + message.Data.OculusID + " <" + message.Data.ID + ">");
        }
    }
}
