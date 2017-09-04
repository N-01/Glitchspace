using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongQuad : MonoBehaviour {
	public Camera cam;
	public MeshRenderer mesh;
	public RenderTexture[] renderTextures = new RenderTexture[2];
	public Material mat;

	// Use this for initialization
	void Awake () {
		cam = GetComponent<Camera>();
		mat = mesh.material;
		mat.SetTexture("_MainTex", renderTextures[0]);
		cam.targetTexture = renderTextures[1];
	}

	// Update is called once per frame
	public int currentFrame = 0;

	public void Swap()
	{
		mat.SetTexture("_MainTex", renderTextures[1 - currentFrame]);
		cam.targetTexture = renderTextures[currentFrame];

		currentFrame = 1 - currentFrame;
	}
}
