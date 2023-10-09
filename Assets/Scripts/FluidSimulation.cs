using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public class FluidSimulation : MonoBehaviour
{
    //private class Fluid
    //{
    //    private void ComputeCoefficients()
    //    {
    //        float f1 = wavePropagationVelocity * wavePropagationVelocity * timeSlice * timeSlice / (verticesDistance * verticesDistance);
    //        float f2 = 1 / (viscosity * timeSlice + 2);
    //        k1 = (4 - 8 * f1) * f2;
    //        k2 = (viscosity * timeSlice - 2) * f2;
    //        k3 = 2 * f1 * f2;
    //    }
    //}

    public int planeDefaultLength;
    public float timeSlice = 0.2f;
    public float wavePropagationVelocity = 0.15f;
    public float viscosity = 0.15f;

    public int textureResolution;

    public float minPlaneX;
    public float maxPlaneX;
    public float minPlaneY;
    public float maxPlaneY;

    public float playerMovingForce = -0.1f;
    public float rainForce = -0.05f;
    public float objectEnterForce = -1.5f;

    public Material waterSimulationRTMaterial;

    private MeshRenderer planeRenderer;
    private Material planeMaterial;
    public CustomRenderTexture waterSimulationRT;

    private float verticesDistance;

    void Awake()
    {
        planeRenderer = GetComponent<MeshRenderer>();
        planeMaterial = planeRenderer.material;

        ComputeCoefficients(out float k1, out float k2, out float k3);

        waterSimulationRTMaterial.SetFloat("k1", k1);
        waterSimulationRTMaterial.SetFloat("k2", k2);
        waterSimulationRTMaterial.SetFloat("k3", k3);

        waterSimulationRT = new CustomRenderTexture(textureResolution, textureResolution, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32_SFloat);
        waterSimulationRT.material = waterSimulationRTMaterial;
        waterSimulationRT.updateMode = CustomRenderTextureUpdateMode.OnDemand;
        waterSimulationRT.initializationColor = Color.black;
        waterSimulationRT.doubleBuffered = true;
        waterSimulationRT.depth = 0;
        waterSimulationRT.useMipMap = true;
        waterSimulationRT.wrapMode = TextureWrapMode.Mirror;

        planeMaterial.mainTexture = waterSimulationRT;

        //waterSimulationRT.Update();
    }

    private void ComputeCoefficients(out float k1, out float k2, out float k3)
    {
        verticesDistance = planeDefaultLength * transform.localScale.x / (textureResolution * textureResolution);
        float f1 = wavePropagationVelocity * wavePropagationVelocity * timeSlice * timeSlice / (verticesDistance * verticesDistance);
        float f2 = 1 / (viscosity * timeSlice + 2);
        k1 = (4 - 8 * f1) * f2;
        k2 = (viscosity * timeSlice - 2) * f2;
        k3 = 2 * f1 * f2;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            var clickZone = new CustomRenderTextureUpdateZone();
            clickZone.needSwap = true;
            clickZone.passIndex = 2;
            clickZone.rotation = 0f;
            clickZone.updateZoneCenter = new Vector2(0.5f, 0.5f);
            clickZone.updateZoneSize = new Vector2(0.1f, 0.1f);

            waterSimulationRT.SetUpdateZones(new CustomRenderTextureUpdateZone[] { clickZone });
            waterSimulationRT.Update();
        }
    }

    void FixedUpdate()
    {
        var defaultZone = new CustomRenderTextureUpdateZone();
        defaultZone.needSwap = true;
        defaultZone.passIndex = 0;
        defaultZone.rotation = 0f;
        defaultZone.updateZoneCenter = new Vector2(0.5f, 0.5f);
        defaultZone.updateZoneSize = new Vector2(1f, 1f);
        waterSimulationRT.ClearUpdateZones();
        waterSimulationRT.SetUpdateZones(new CustomRenderTextureUpdateZone[] { defaultZone });
        waterSimulationRT.Update(2);
    }

    void OnTriggerEnter(Collider other)
    {
        var defaultZone = new CustomRenderTextureUpdateZone();
        defaultZone.needSwap = true;
        defaultZone.passIndex = 0;
        defaultZone.rotation = 0f;
        defaultZone.updateZoneCenter = new Vector2(0.5f, 0.5f);
        defaultZone.updateZoneSize = new Vector2(1f, 1f);

        Vector2 fluidCoord = WorldToFluidSpace(other.transform.position.x, other.transform.position.z);
        var clickZone = new CustomRenderTextureUpdateZone();
        clickZone.needSwap = true;
        clickZone.passIndex = 1;
        clickZone.rotation = 0f;
        clickZone.updateZoneCenter = fluidCoord;
        clickZone.updateZoneSize = new Vector2(0.1f, 0.1f);

        waterSimulationRTMaterial.SetFloat("currentForce", 0.1f);

        waterSimulationRT.ClearUpdateZones();
        waterSimulationRT.SetUpdateZones(new CustomRenderTextureUpdateZone[] { clickZone, defaultZone });
        waterSimulationRT.Update(2);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var defaultZone = new CustomRenderTextureUpdateZone();
            defaultZone.needSwap = true;
            defaultZone.passIndex = 0;
            defaultZone.rotation = 0f;
            defaultZone.updateZoneCenter = new Vector2(0.5f, 0.5f);
            defaultZone.updateZoneSize = new Vector2(1f, 1f);

            Vector2 fluidCoord = WorldToFluidSpace(other.transform.position.x, other.transform.position.z);
            var clickZone = new CustomRenderTextureUpdateZone();
            clickZone.needSwap = true;
            clickZone.passIndex = 1;
            clickZone.rotation = 0f;
            clickZone.updateZoneCenter = fluidCoord;
            clickZone.updateZoneSize = new Vector2(0.1f, 0.1f);

            waterSimulationRTMaterial.SetFloat("currentForce", 0.025f);

            waterSimulationRT.ClearUpdateZones();
            waterSimulationRT.SetUpdateZones(new CustomRenderTextureUpdateZone[] { clickZone, defaultZone });
            waterSimulationRT.Update();
        }
    }

    private Vector2 WorldToFluidSpace(float worldX, float worldY)
    {
        return new Vector2(
            Mathf.InverseLerp(minPlaneX, maxPlaneX, worldX),
            1-Mathf.InverseLerp(minPlaneY, maxPlaneY, worldY));
    }
}
