using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestSceneLogic : NetworkBehaviour
{
    public PlayerController controller;
    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.StartHost();
        StartCoroutine(GameStart());
    }

    private IEnumerator GameStart()
    {
        yield return StartCoroutine(MapGenerator.instance.PreGenerateMap());
        CameraViewManager.instance.CameraInitialize(controller.transform);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
