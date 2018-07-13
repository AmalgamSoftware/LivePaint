using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform player;
	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position;
		transform.LookAt (player.position);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = player.position + offset;
	}



}
