using UnityEngine;

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

    void Awake()
    {
        planeRenderer = GetComponent<MeshRenderer>();

        ComputeCoefficients(out float k1, out float k2, out float k3);

        waterSimulationRTMaterial.SetFloat("k1", k1);
        waterSimulationRTMaterial.SetFloat("k2", k2);
        waterSimulationRTMaterial.SetFloat("k3", k3);

        waterSimulationRT = new CustomRenderTexture(textureResolution, textureResolution, RenderTextureFormat.R8);
        waterSimulationRT.material = waterSimulationRTMaterial;
        waterSimulationRT.updateMode = CustomRenderTextureUpdateMode.OnDemand;
        waterSimulationRT.initializationColor = Color.black;

        waterSimulationRT.Update();
    }

    private void ComputeCoefficients(out float k1, out float k2, out float k3)
    {
        float verticesDistance = planeDefaultLength * transform.localScale.x / (textureResolution * textureResolution);
        float f1 = wavePropagationVelocity * wavePropagationVelocity * timeSlice * timeSlice / (verticesDistance * verticesDistance);
        float f2 = 1 / (viscosity * timeSlice + 2);
        k1 = (4 - 8 * f1) * f2;
        k2 = (viscosity * timeSlice - 2) * f2;
        k3 = 2 * f1 * f2;
    }

    void Start()
    {
        planeMaterial = planeRenderer.material;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        //Vector2 fluidCoord = WorldToFluidSpace(other.transform.position.x, other.transform.position.z);
        //fluid.Turbate(fluidCoord.x, fluidCoord.y, objectEnterForce, 2);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Vector2 fluidCoord = WorldToFluidSpace(other.transform.position.x, other.transform.position.z);
            //fluid.Turbate(fluidCoord.x, fluidCoord.y, playerMovingForce, 2);
        }
    }

    //private Vector2 WorldToFluidSpace(float worldX, float worldY)
    //{
    //    return new Vector2(
    //        Mathf.InverseLerp(minPlaneX, maxPlaneX, Mathf.Round((worldX - transform.position.x) / distance) * distance),
    //        Mathf.InverseLerp(minPlaneY, maxPlaneY, Mathf.Round((worldY - transform.position.z) / distance) * distance));
    //}
}
