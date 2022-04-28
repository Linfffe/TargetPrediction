using UnityEngine;

public class ShellScript : MonoBehaviour
{
	public float initVelocity;
	public float G;

	private Rigidbody myRigidbody;
	private float doomTime = 5;
    // Start is called before the first frame update
    void Start()
    {
		if (G == 0) G = -9.8f;
		myRigidbody = GetComponent<Rigidbody>();
		myRigidbody.velocity =transform.forward * initVelocity;
    }

	private void Update() {
		doomTime -= Time.deltaTime;
		if (doomTime < 0) Destroy(this.gameObject);
	}

}
