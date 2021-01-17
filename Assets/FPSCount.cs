using UnityEngine;
using System.Collections;

public class FPSCount : MonoBehaviour
{
    public static FPSCount active;

    float startTime = 0f;
    float updateTime = 0f;

    [HideInInspector] public float fps = 0.0f;
    public float refreshTime = 1.0f;

    int frameCount = 0;

    IEnumerator CalculateFPS()
    {
        while (true)
        {
            fps = frameCount / (updateTime - startTime);
            startTime = Time.time;
            frameCount = 0;
            yield return new WaitForSeconds(refreshTime);
        }
    }

    void Awake()
    {
        active = this;
    }

    void Start()
    {
        startTime = Time.time;
        updateTime = startTime;

        StartCoroutine(CalculateFPS());
    }

    void Update()
    {
        updateTime = Time.time;
        frameCount++;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.9f, 500f, 20f), "FPS: " + fps);
    }
}
