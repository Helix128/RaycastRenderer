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
    public float AOFalloff;
    public float AORayDistance;
    public float AOPower;
   //reflections look bad valve pls fix
    public bool ReflectionBounce;
   public float ReflectionRayLength;
    Vector3 RefOffset;

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
        Target.GetComponent<AspectRatioFitter>().aspectRatio = width*1.5f / height;
        if (AutoStepSize)
        {
            stepSizeX = (float)width / 128;
            stepSizeY = (float)height / 128;

        }
        SunDir = Sun.transform.forward;
        if (Input.GetAxisRaw("Vertical")+Input.GetAxisRaw("Horizontal")>0.01f||Input.GetKey(KeyCode.Mouse1))
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
        Ray CamRay = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2));

        RaycastHit hit;
        RaycastHit shadehit;
        RaycastHit rhit;
        Debug.DrawRay(TracingOffset + CamRay.origin + (transform.up * ((h - height) / height) / Camera.main.fieldOfView) / stepSizeY, CamRay.direction + (transform.right * (i - width / 2)) / stepSizeX * Camera.main.fieldOfView / 2000 + (transform.up * .01f) / stepSizeY + (transform.up * (h - (height / 2)) * 0.01f) / stepSizeY, pixelColor, 2f);
        tracedPixels++;
        if (Physics.Raycast(TracingOffset + CamRay.origin + (transform.up * ((h - height) / height) / Camera.main.fieldOfView) / stepSizeY, CamRay.direction + (transform.right * (i - width / 2)) / stepSizeX * Camera.main.fieldOfView / 2000 + (transform.up * stepSizeX / 5) / stepSizeY + (transform.up * ((h - height / 2)) * 0.01f) / stepSizeY, out hit))
        {



            pixelColor = RayToColor(hit);
            Final.SetPixel(i, h, pixelColor*Sun.color);
            Final.Apply();



            if (ReflectionBounce)
            {
                for (int ao = 0; ao < AOBounces; ao++)
                {
                    Debug.DrawRay(hit.point + hit.normal * 0.01f, Vector3.Reflect(CamRay.direction, hit.normal), Color.blue);
                    if (Physics.Raycast(hit.point + hit.normal * 0.01f, Vector3.Reflect(CamRay.direction, hit.normal), out rhit,ReflectionRayLength))
                    {
                        pixelColor = RayToColor(rhit);
                        Final.SetPixel(i, h, (Final.GetPixel(i, h) + ColorDivide(pixelColor,new Color(hit.distance*10,hit.distance*10,hit.distance*10))+new Color(0,0,0,1)));
                        Final.Apply();

                    }
                }
            }



            if (ShadeBounce)
            {
                Debug.DrawRay(hit.point + hit.normal * 0.001f, -SunDir * 100, Color.yellow);
                if (Physics.Raycast(hit.point + hit.normal * 0.001f, -SunDir, out shadehit, Mathf.Infinity))
                {
                    Final.SetPixel(i, h, Final.GetPixel(i, h) * RenderSettings.ambientLight);
                    Final.Apply();

                    for (int ao = 0; ao < AOBounces; ao++)
                    {
                        Debug.DrawRay(hit.point + hit.normal * 0.002f, Vector3.Reflect(transform.forward, hit.normal), Color.black);
                        if (Physics.Raycast(hit.point + hit.normal * 0.002f, Vector3.Reflect(transform.forward, hit.normal), out shadehit, AORayDistance))
                        {
                            Final.SetPixel(i, h, Final.GetPixel(i, h) - ColorClamp((((new Color(1, 1, 1, 0) / AOBounces) / (shadehit.distance * AOFalloff*100))) * AOPower    ));
                            Final.Apply();

                        }
                    }
                }

            }


        }
        else
        {
            
            Final.SetPixel(i, h, Background);
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
        Final.filterMode = filter;
        Target.texture = Final;
    }
    public Color ColorClamp(Color input)
    {
        return new Color(Mathf.Clamp01(input.r), Mathf.Clamp01(input.g), Mathf.Clamp01(input.b), Mathf.Clamp01(input.a));

    }
    public Color ColorDivide(Color d1,Color d2)
    {

        return new Color(d1.r / d2.r, d1.g / d2.g, d1.b / d2.b, d1.a / d2.a);
    }

    public Vector3 Reflect(Vector3 input, Vector3 n)
    {

        return input - 2 * Vector3.Dot(input, n) * n;
    }

    public Color RayToColor(RaycastHit hit)
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
                return pixelColor;
            }
            else
            {
                pixelColor = pixelColor = rend.sharedMaterial.color;
                return pixelColor;
            }
        }
        else
        {
            pixelColor = rend.sharedMaterial.color;
            return pixelColor;
        }

    }
    public Color SingleColor(float value)
    {
        return new Color(value, value, value, 1);
    }
}

//Unused old reflection code. Reflections were too ugly this way.
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

