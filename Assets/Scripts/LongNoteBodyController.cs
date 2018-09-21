using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongNoteBodyController : MonoBehaviour
{
    public float Panel0x;
    public float HiSpeed;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0f, -7.5f, 0f) * Time.deltaTime * HiSpeed);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="time">as second</param>
    public void SetProperties(int start, int end, float time)
    {
        int width = end - start + 1;
        transform.position = new Vector3(Panel0x + start * 0.5f, -3f + 7.5f * HiSpeed + (7.5f * time * HiSpeed) / 2f, transform.position.z);
        transform.localScale = new Vector3(0.5f * width - 0.2f, time * HiSpeed * 7.5f, 1f);

    }
}
