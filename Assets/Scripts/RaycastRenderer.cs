using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RaycastRenderer : MonoBehaviour
{
    public RawImage Target;
    public int width;
    public int height;
    public bool AutoStepSize;
    public float stepSizeX = 1;
    public float stepSizeY = 1;
    public Light Sun;
    public Color Background;
    public bool ShadeBounce;
    public bool ReflectionBounce;
    public float ReflectionRayLength;
    Vector3 SunDir;

    public bool Render;
    public int i;
    public int h;
    int tracedPixels;
    public Vector3 TracingOffset;
    float Timer;
    Texture2D Final;
    Vector3 pos;
    Vector3 euler;

    // Start is called before the first frame update
    void Start()
    {
        tracedPixels = 0;
        //Setup the target texture
        Final = new Texture2D(width, height);
        Final.filterMode = FilterMode.Point;

        //Init rendering
        RaycastRender();
    }

    // Update is called once per frame
    void Update()
    {
        Target.GetComponent<AspectRatioFitter>().aspectRatio = width / height;
        if (AutoStepSize)
        {
            stepSizeX = width / 64;
            stepSizeY = height / 64;

        }
        SunDir = Sun.transform.forward;
        if (Mathf.Abs(pos.magnitude - transform.position.magnitude)>0.0001f||Mathf.Abs(euler.magnitude-transform.eulerAngles.magnitude)>0.0001f)
        { 
            i = 0;
            h = 0;
            Final = new Texture2D(width, height);
            Final.filterMode = FilterMode.Point;
            Final.Apply();
            tracedPixels = 0;
            Render = true;
        }
      
    }
    private void FixedUpdate()
    {
        Timer -= Time.fixedDeltaTime;
        if (Timer < 0)
        {
            Timer = 1f;
            pos = transform.position;
            euler = transform.eulerAngles;
         
        }
        if (Render)
        {
            RaycastRender();
        }
    }
    void RaycastRender()
    {
      Ray CamRay = Camera.main.ScreenPointToRay(transform.position);
        i += 1;
        RaycastHit hit;
        RaycastHit shit;
        RaycastHit ghit;
        RaycastHit rhit;
        Debug.DrawRay(TracingOffset+CamRay.origin+(transform.up*(h-width/2)/ Camera.main.fieldOfView)/stepSizeY, CamRay.direction + (transform.right * i)/stepSizeX * Camera.main.fieldOfView / 2000 + ( transform.up * .01f)/stepSizeY + (transform.up * (h - height / 4)/height)/stepSizeY, Color.red,8f);
        tracedPixels++;
        if (Physics.Raycast(TracingOffset + CamRay.origin + (transform.up * (h - width / 2) / Camera.main.fieldOfView) / stepSizeY, CamRay.direction + (transform.right * i) / stepSizeX * Camera.main.fieldOfView / 2000 + (transform.up *stepSizeX/5) / stepSizeY + (transform.up * (h - height / 4) / height) / stepSizeY, out hit))
        {
            MeshRenderer rend = hit.collider.GetComponent<MeshRenderer>();
            Texture2D texture2D = rend.sharedMaterial.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= texture2D.width;
            pixelUV.y *= texture2D.height;
            Vector2 tiling = rend.sharedMaterial.mainTextureScale;
            if (texture2D != null && texture2D.isReadable)
            {
                Final.SetPixel(i, h, texture2D.GetPixel((int)pixelUV.x*(int)tiling.x,(int)pixelUV.y*(int)tiling.y) * rend.sharedMaterial.color);
                Final.Apply();

            }
            else
            {
                Final.SetPixel(i, h, rend.sharedMaterial.color);
                Final.Apply();

            }




            if (ShadeBounce)
            {
                Debug.DrawRay(hit.point + hit.normal * 0.02f, -SunDir*100, Color.yellow);
                if (Physics.Raycast(hit.point + hit.normal * 0.02f, -SunDir, out shit,Mathf.Infinity))
                {
                    Final.SetPixel(i, h,Final.GetPixel(i,h)*RenderSettings.ambientLight);
                    Final.Apply();
                }
                else
                {
                    if (ReflectionBounce)
                    {
                        Debug.DrawRay(hit.point + hit.normal * 0.02f, hit.normal*100, Color.yellow);
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
                    Debug.DrawRay(hit.point + hit.normal * 0.02f, hit.normal*100, Color.yellow);
                    if (Physics.Raycast(hit.point + hit.normal * 0.02f, hit.normal, out rhit, ReflectionRayLength))
                    {
                        Final.SetPixel(i, h, Final.GetPixel(i, h) * (rhit.collider.GetComponent<MeshRenderer>().sharedMaterial.color));
                        Final.Apply();
                    }
                }
            }
          
        }
        else
        {
            Final.SetPixel(i, h,Background);
            Final.Apply();
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
        if (tracedPixels >= width * height)
        {

            Render = false;

        }
        Target.texture = Final;
    }
     
        
       
     

  

    }

