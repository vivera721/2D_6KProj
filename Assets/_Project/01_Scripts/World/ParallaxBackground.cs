using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private float length, startPos;
    public float parallaxFactor;
    public GameObject mainCam;

    private void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        float temp = mainCam.transform.position.x * (1 - parallaxFactor);
        float distance = mainCam.transform.position.x * parallaxFactor;

        Vector3 newPosition = new Vector3(startPos+distance, transform.position.y, transform.position.z);
        transform.position = newPosition;

        if (temp > startPos + (length / 2)) startPos += length;
        else if(temp < startPos - (length / 2)) startPos -= length;
    }
}
