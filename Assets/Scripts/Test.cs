using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    public GameObject ThumbPrefab;


    public Transform camera;
    public float radius = 5.0f;

    private Transform target;
    private float m_RoundValue = 0;


    // Start is called before the first frame update
    void Start()
    {

        radius = 5.0f;

        for (int i=0; i<15; i++)
        {
            GameObject obj = Instantiate(ThumbPrefab,this.transform);

            float angle = i * 360/15;
            float ang = Mathf.PI/180 * angle;
            float z = radius *  Mathf.Sin(ang);
            float x = radius * Mathf.Cos(ang);
            Vector3 pos = new Vector3(x, 0, z);
            obj.transform.localPosition = pos;
            obj.transform.localRotation = Quaternion.LookRotation(pos);

            if (i == 0)
            {
                target = obj.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        float h = Input.GetAxis("Mouse X") * 2;
        float v = Input.GetAxis("Mouse Y") * 2;
        bool leftDown = Input.GetMouseButton(0);
        
        Vector3 targetPos = this.target.position - this.target.forward * 2;

        camera.position = Vector3.Lerp(camera.position, targetPos, Time.deltaTime * 5);

        //if (Input.GetMouseButtonDown(0)) {
        //    m_RoundValue = 0;
        //}

        if (leftDown) {
            Vector3 deltaRotatoin = new Vector3(-v*3,h*10,0);//  camera.up * h * 10;

            m_RoundValue += Vector3.Angle(Vector3.zero, deltaRotatoin);
            
            camera.Rotate(deltaRotatoin);
        } else
        {
            Vector3 cameraPos = camera.position + Vector3.up * 2;
            Vector3 targetDir = target.position + Vector3.up * 2f - cameraPos;
            Quaternion targetQua = Quaternion.LookRotation(targetDir);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, targetQua, Time.deltaTime * 5);


            //float angle = Quaternion.Angle(camera.localRotation, targetQua); 
            //camera.localRotation = Quaternion.RotateTowards(camera.localRotation, targetQua, 5);

            //float f = Vector3.Angle(camera.forward, targetDir);
            //Vector3.Slerp(camera.forward,)
            //camera.Rotate(camera.up * m_RoundValue);




        }
        
        
        
    }
}
