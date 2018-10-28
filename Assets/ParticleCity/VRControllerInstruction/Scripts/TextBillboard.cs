using UnityEngine;
using WanderUtils;

public class TextBillboard : MonoBehaviour {

	void Start () {
	
	}

    void LateUpdate() {
        transform.rotation = Quaternion.LookRotation(InputManager.Instance.CenterCamera.transform.forward, InputManager.Instance.CenterCamera.transform.up);
    }
}
