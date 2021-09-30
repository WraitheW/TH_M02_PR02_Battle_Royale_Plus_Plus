using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    private Quaternion myRotation;

    // Start is called before the first frame update
    void Start()
    {
        myRotation = this.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = myRotation;
    }
}
