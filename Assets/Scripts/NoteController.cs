using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
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

    public void SetProperties(int start, int end)
    {
        int width = end - start + 1;
        transform.position = new Vector3(Panel0x + start * 0.5f, -3f + 7.5f * HiSpeed, transform.position.z);
        transform.localScale = new Vector3(0.5f * width, 0.1f, 1f);

    }
}
