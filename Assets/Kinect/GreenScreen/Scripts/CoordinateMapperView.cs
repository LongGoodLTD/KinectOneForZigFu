﻿using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class CoordinateMapperView : MonoBehaviour
{
	public GameObject CoordinateMapperManager;
	private CoordinateMapperManager _CoordinateMapperManager;

	private ComputeBuffer depthBuffer;
	private ComputeBuffer bodyIndexBuffer;

	DepthSpacePoint[] depthPoints;
	byte[] bodyIndexPoints;
	
	void Start ()
	{
		//清空緩衝器。
		ReleaseBuffers ();
		//如果讀取不到資料來源程式，跳出。
		if (CoordinateMapperManager == null)
		{
			return;
		}
		
		_CoordinateMapperManager = CoordinateMapperManager.GetComponent<CoordinateMapperManager>();
		//讀取彩色貼圖。
		Texture2D renderTexture = _CoordinateMapperManager.GetColorTexture();
		if (renderTexture != null)
		{
			gameObject.renderer.material.SetTexture("_MainTex", renderTexture);
		}
		//讀取深度資料座標。
		depthPoints = _CoordinateMapperManager.GetDepthCoordinates ();
		if (depthPoints != null)
		{
			depthBuffer = new ComputeBuffer(depthPoints.Length, sizeof(float) * 2);
			gameObject.renderer.material.SetBuffer("depthCoordinates", depthBuffer);
		}
		//讀取人體辨識資料。
		bodyIndexPoints = _CoordinateMapperManager.GetBodyIndexBuffer ();
		if (bodyIndexPoints != null)
		{
			bodyIndexBuffer = new ComputeBuffer(bodyIndexPoints.Length, sizeof(float));
			gameObject.renderer.material.SetBuffer ("bodyIndexBuffer", bodyIndexBuffer);
		}
	}

	void Update()
	{
		//TODO: fix perf on this call.
		depthBuffer.SetData(depthPoints);

		// ComputeBuffers do not accept bytes, so we need to convert to float.
		float[] buffer = new float[512 * 424];
		for (int i = 0; i < bodyIndexPoints.Length; i++)
		{
			buffer[i] = (float)bodyIndexPoints[i];
		}
		bodyIndexBuffer.SetData(buffer);
		buffer = null;
	}
	
	private void ReleaseBuffers() 
	{
		if (depthBuffer != null) depthBuffer.Release();
		depthBuffer = null;

		if (bodyIndexBuffer != null) bodyIndexBuffer.Release();
		bodyIndexBuffer = null;

		depthPoints = null;
		bodyIndexPoints = null;
	}
	
	void OnDisable() 
	{
		ReleaseBuffers ();
	}
}
