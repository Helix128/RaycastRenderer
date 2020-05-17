using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RaycastRenderer : MonoBehaviour
{
    public RawImage Target;
    public int width;
    public int height;
    public Light Sun;

    public bool ShadeBounce;
    public bool ReflectionBounce;
 
    public float ReflectionRayLength;
    Vector3 SunDir;
    int pixelCount;
    public bool Realtime;
    public int i;
    public int h;
    public Vector3 TracingOffset;
    float Timer;
    Texture2D Final;
    Vector3 pos;
    Vector3 euler;

    // Start is called before the first frame update
    void Start()
    {
        RaycastRender();

        Final = new Texture2D(width, height);
        Final.filterMode = FilterMode.Point;
    }

    // Update is called once per frame
    void Update()
    {
        SunDir = Sun.transform.forward;
        if (Mathf.Abs(pos.magnitude - transform.position.magnitude)>0.01f||Mathf.Abs(euler.magnitude-transform.eulerAngles.magnitude)>0.01f)
        {
            i = 0;
            h = 0;
            Final = new Texture2D(width, height);
            Final.filterMode = FilterMode.Point;
            Final.Apply();  
        }
        if (Realtime)
        {
            RaycastRender();
        }   
    }
    private void FixedUpdate()
    {
        pos = transform.position;
        euler = transform.eulerAngles;
     
    }
    void RaycastRender()
    {
      Ray CamRay = Camera.main.ScreenPointToRay(transform.position);
        i += 1;
        RaycastHit hit;
        RaycastHit shit;
        RaycastHit ghit;
        RaycastHit rhit;
        Debug.DrawRay(TracingOffset+CamRay.origin+transform.up*(h-width/2)/ Camera.main.fieldOfView, CamRay.direction + transform.right * i * Camera.main.fieldOfView / 2000 + transform.up * .01f + transform.up * (h - height / 4)/height, Color.red,8f);

        if (Physics.Raycast(TracingOffset + CamRay.origin + transform.up * (h - width / 2) / Camera.main.fieldOfView, CamRay.direction + transform.right * i * Camera.main.fieldOfView / 2000 + transform.up * .01f + transform.up * (h - height / 4) / height, out hit))
        {

            Final.SetPixel(i, h, hit.collider.GetComponent<MeshRenderer>().sharedMaterial.color);


            Final.Apply();
       
            if (ShadeBounce)
            {
                Debug.DrawRay(hit.point + hit.normal * 0.02f, -SunDir, Color.yellow);
                if (Physics.Raycast(hit.point + hit.normal * 0.02f, -SunDir, out shit,Mathf.Infinity))
                {
                    Final.SetPixel(i, h,Final.GetPixel(i,h)*RenderSettings.ambientLight);
                    Final.Apply();
                }
                else
                {
                    if (ReflectionBounce)
                    {
                        Debug.DrawRay(hit.point + hit.normal * 0.02f, hit.normal, Color.yellow);
                        if (Physics.Raycast(hit.point + hit.normal * 0.02f, hit.normal, out ghit,ReflectionRayLength))
                        {
                            Final.SetPixel(i, h,Final.GetPixel(i, h)*(ghit.collider.GetComponent<MeshRenderer>().sharedMaterial.color));
                            Final.Apply();
                        }
                    }
                }
            }
            else
            {
                if (ReflectionBounce)
                {
                    Debug.DrawRay(hit.point + hit.normal * 0.02f, hit.normal, Color.yellow);
                    if (Physics.Raycast(hit.point + hit.normal * 0.02f, hit.normal, out rhit, ReflectionRayLength))
                    {
                        Final.SetPixel(i, h, Final.GetPixel(i, h) * (rhit.collider.GetComponent<MeshRenderer>().sharedMaterial.color));
                        Final.Apply();
                    }
                }
            }
          
        }
  
        
        if (i>width)
        {
            i = 0;
            h++;

        }
        if (h > height)
        {
            h = 0;
        }

        Target.texture = Final;
    }
     
        
       
     

  

    }

