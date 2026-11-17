using UnityEngine;
using System.Collections.Generic;

public class GameManagerScr : MonoBehaviour
{
    public float tickRate = 15f;
    public float tickSpeed = 1f;
    public List<Chapter> chapters;
    private void Tick()
    {
        for (int i = 0; i < chapters.Count; i++)
        {
            chapters[i].Tick();
        }
    }
    void Update()
    {
        if (tickRate>0)
        {
            tickRate -= Time.deltaTime * tickSpeed;
        }
        else
        {
            Tick();
            tickRate = 15f;
        }
    }
}
