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
    public FilterMode filter;
    public Light Sun;
    public Color Background;
    public bool ShadeBounce;
    public int AOBounces;
    public float AORayDistance;
    public float AOPower;
    //public bool ReflectionBounce;

    //public float ReflectionRayLength;
    //public Vector3 RefOffset;
  
    Vector3 SunDir;

    public bool Render;
    bool debugBounce;
    public int i;
    public int h;
    int tracedPixels;
    public Vector3 TracingOffset;
    float Timer;
    Texture2D Final;
    Vector3 pos;
    Vector3 euler;
    Color pixelColor;

    // Start is called before the first frame update
    void Start()
    {
        tracedPixels = 0;
        //Setup the target texture
        Final = new Texture2D(width, height);
        Final.filterMode = filter;

        //Init rendering
        RaycastRender();
    }

    // Update is called once per frame
    void Update()
    {
        Target.GetComponent<AspectRatioFitter>().aspectRatio = width*2 / height;
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
            Final.filterMode = filter;
            Final.Apply();
            tracedPixels = 0;
            Render = true;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            i = 0;
            h = 0;
            Final = new Texture2D(width, height);
            Final.filterMode = filter;
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

        RaycastHit hit;
        RaycastHit shadehit;
  
        Debug.DrawRay(TracingOffset+CamRay.origin+(transform.up*(h-width/2)/ Camera.main.fieldOfView)/stepSizeY, CamRay.direction + (transform.right * i)/stepSizeX * Camera.main.fieldOfView / 2000 + ( transform.up * .01f)/stepSizeY + (transform.up * (h - height / 4)/height)/stepSizeY, Color.red,8f);
        tracedPixels++;
        if (Physics.Raycast(TracingOffset + CamRay.origin + (transform.up * (h - width / 2) / Camera.main.fieldOfView) / stepSizeY, CamRay.direction + (transform.right * i) / stepSizeX * Camera.main.fieldOfView / 2000 + (transform.up *stepSizeX/5) / stepSizeY + (transform.up * (h - height / 4) / height) / stepSizeY, out hit))
        {
            MeshRenderer rend = hit.collider.GetComponent<MeshRenderer>();
            Texture2D texture2D = rend.sharedMaterial.mainTexture as Texture2D;
          
            if (texture2D != null)
            {
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= texture2D.width;
                pixelUV.y *= texture2D.height;
                Vector2 tiling = rend.sharedMaterial.mainTextureScale;
               pixelColor = Color.white;
                if (texture2D.isReadable)
                {
                    pixelColor = texture2D.GetPixel((int)pixelUV.x * (int)tiling.x, (int)pixelUV.y * (int)tiling.y) * rend.sharedMaterial.color * Sun.color;
                    Final.SetPixel(i, h, texture2D.GetPixel((int)pixelUV.x * (int)tiling.x, (int)pixelUV.y * (int)tiling.y) * rend.sharedMaterial.color * Sun.color);
                    Final.Apply();
                }
            }
            else
            {
                pixelColor = rend.sharedMaterial.color;
                Final.SetPixel(i, h, rend.sharedMaterial.color);
                Final.Apply();

            }




            if (ShadeBounce)
            {
                Debug.DrawRay(hit.point+hit.normal*0.002f, -SunDir*100, Color.yellow);
                if (Physics.Raycast(hit.point+hit.normal*0.002f, -SunDir, out shadehit,Mathf.Infinity))
                {
                    Final.SetPixel(i, h,Final.GetPixel(i,h)*RenderSettings.ambientLight);
                    Final.Apply();
                  
                    for (int ao = 0; ao < AOBounces; ao++)
                    {
                        Debug.DrawRay(hit.point + hit.normal * 0.02f, Vector3.Reflect(transform.forward, hit.normal), Color.black);
                        if (Physics.Raycast(hit.point + hit.normal * 0.02f, Vector3.Reflect(transform.forward, hit.normal), out shadehit, AORayDistance))
                        {
                            Final.SetPixel(i, h, Final.GetPixel(i, h) - ColorClamp((((new Color(1, 1, 1, 0) / AOBounces) / (shadehit.distance * 100))) * AOPower));
                            Final.Apply();

                        }
                    }
                }
                
            }
         
          
        }
        else
        {
            Final.SetPixel(i, h,Background);
            Final.Apply();
        }

        if (!debugBounce)
        {
            i += 1;
            if (i > width)
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
        }
        Target.texture = Final;
    }
     public Color ColorClamp(Color input)
    {
        return new Color(Mathf.Clamp01(input.r), Mathf.Clamp01(input.g), Mathf.Clamp01(input.b), Mathf.Clamp01(input.a));

    }
        
       
     

  

    }

//Unused old reflection code. Reflections were too ugly so i removed them.
/*  if (ReflectionBounce)
                    {

                            Debug.DrawRay(hit.point + hit.normal * 0.02f, Vector3.Reflect(transform.forward, hit.normal)+RefOffset * ReflectionRayLength, Color.yellow);
                            if (Physics.Raycast(hit.point + hit.normal * 0.02f, Vector3.Reflect(transform.forward, hit.normal)+RefOffset, out rhit, ReflectionRayLength))
                            {
                                MeshRenderer render = rhit.collider.GetComponent<MeshRenderer>();
                                Texture2D texture2Dr = render.sharedMaterial.mainTexture as Texture2D;

                                if (texture2Dr != null)
                                {
                                    Vector2 pixelUV = rhit.textureCoord;
                                    pixelUV.x *= texture2Dr.width;
                                    pixelUV.y *= texture2Dr.height;
                                    Vector2 tiling = render.sharedMaterial.mainTextureScale;
                             
                                    if (texture2Dr.isReadable)
                                    {
                                        pixelColor = texture2Dr.GetPixel((int)pixelUV.x * (int)tiling.x, (int)pixelUV.y * (int)tiling.y) * render.sharedMaterial.color * Sun.color;
                                        Final.SetPixel(i, h, (Final.GetPixel(i, h) * pixelColor) * RenderSettings.ambientLight);
                                        Final.Apply();
                                    }
                                    else
                                    {
                                        pixelColor = render.sharedMaterial.color*Sun.color;
                                        Final.SetPixel(i, h, (Final.GetPixel(i, h) * pixelColor) * RenderSettings.ambientLight);
                                        Final.Apply();
                                    }
                                }
                         
                            
                        }
                    }
                    */

