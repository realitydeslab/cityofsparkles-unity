using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class Bootloader : MonoBehaviour
{
    private AsyncOperation citySceneLoadOp;

	void Start () {
        Debug.Log("Loaded XR Device: " + XRSettings.loadedDeviceName);

	    if (XRSettings.loadedDeviceName == "Oculus")
	    {
	        SceneManager.LoadScene("oculus_rift_components");
	    }
        else if (XRSettings.loadedDeviceName == "OpenVR")
	    {
            SceneManager.LoadScene("steam_vr_components");
	    }

	    citySceneLoadOp = SceneManager.LoadSceneAsync("new_york_scene", LoadSceneMode.Additive);
	    citySceneLoadOp.allowSceneActivation = true;
	}
	
	void Update () {
	    if (citySceneLoadOp.isDone)
	    {
	        SceneManager.UnloadSceneAsync("bootloader");
	    }
	}
}
