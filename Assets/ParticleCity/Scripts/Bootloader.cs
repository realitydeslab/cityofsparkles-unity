using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class Bootloader : MonoBehaviour
{
    private AsyncOperation citySceneLoadOp;

    public static string SceneToLoad = "new_york_opening";

	void Start () {
        //AkSoundEngine.StopAll();

        Debug.Log("Loaded XR Device: " + XRSettings.loadedDeviceName);

	    if (XRSettings.loadedDeviceName == "Oculus")
	    {
	        SceneManager.LoadScene("oculus_rift_components");
	    }
        else if (XRSettings.loadedDeviceName == "OpenVR")
	    {
            SceneManager.LoadScene("steam_vr_components");
	    }

	    citySceneLoadOp = SceneManager.LoadSceneAsync(SceneToLoad, LoadSceneMode.Additive);
	    citySceneLoadOp.allowSceneActivation = true;
	}
	
	void Update () {
	    if (citySceneLoadOp.isDone)
	    {
	        SceneManager.UnloadSceneAsync("bootloader");
	    }
	}
}
