using UnityEngine;

public class TextBillboard : MonoBehaviour {

	void Start () {
	
	}

    void Update() {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
    }
}
