using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TargetController : MonoBehaviour
{
	public float speed;
    // Start is called before the first frame update
    void Start()
    {
		GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }
}
