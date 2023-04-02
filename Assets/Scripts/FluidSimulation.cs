using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public class FluidSimulation : MonoBehaviour
{
    private class Fluid
    {
        private int width;
        private int height;
        private float verticesDistance;
        private float timeSlice;
        private float wavePropagationVelocity;
        private float viscosity;

        public Vector3[] buffer1;
        public Vector3[] buffer2;
        public int renderBuffer;

        private Vector3[] normal;
        private Vector3[] tangent;

        private float k1, k2, k3;

        public Fluid(int width, int height, float verticesDistance, float timeSlice, float wavePropagationVelocity, float viscosity)
        {
            this.width = width;
            this.height = height;
            this.verticesDistance = verticesDistance;
            this.timeSlice = timeSlice;
            this.wavePropagationVelocity = wavePropagationVelocity;
            this.viscosity = viscosity;
            int count = width * height;

            buffer1 = new Vector3[count];
            buffer2 = new Vector3[count];
            renderBuffer = 0;

            normal = new Vector3[count];
            tangent = new Vector3[count];

            ComputeCoefficients();
            InitializeBuffers();
        }

        public void Evaluate()
        {
            DisplaceVertices();
            SwapBuffers();
        }

        public void SetTimeSlice(float timeSlice)
        {
            this.timeSlice = timeSlice;
            ComputeCoefficients();
        }

        public void SetViscosity(float viscosity)
        {
            this.viscosity = viscosity;
            ComputeCoefficients();
        }

        public void SetWavePropagationVelocity(float wavePropagationVelocity)
        {
            this.wavePropagationVelocity = wavePropagationVelocity;
            ComputeCoefficients();
        }

        public void Turbate(float x, float y, float force, int size)
        {
            int vert = (int)(y * height * width) + (int)(x * width);
            if (vert > 0 && vert < buffer1.Length)
            {
                buffer1[vert].z += force;
                buffer2[vert].z += force;
            }

            for (int i = 1; i <= size; i++)
            {
                float midForce = force / (i + 1);

                int adjacent = vert + i;
                if (adjacent > 0 && adjacent < buffer1.Length)
                {
                    buffer1[adjacent].z += midForce;
                    buffer2[adjacent].z += midForce;
                }

                adjacent = vert - i;
                if (adjacent > 0 && adjacent < buffer1.Length)
                {
                    buffer1[adjacent].z += midForce;
                    buffer2[adjacent].z += midForce;
                }

                adjacent = vert - width * i;
                if (adjacent > 0 && adjacent < buffer1.Length)
                {
                    buffer1[adjacent].z += midForce;
                    buffer2[adjacent].z += midForce;
                }

                adjacent = vert + width * i;
                if (adjacent > 0 && adjacent < buffer1.Length)
                {
                    buffer1[adjacent].z += midForce;
                    buffer2[adjacent].z += midForce;
                }

                for (int j = 1; j < i + 1; j++)
                {
                    float lowForce = force / (i + 2);

                    adjacent = vert - width * i + j;
                    if (adjacent > 0 && adjacent < buffer1.Length)
                    {
                        buffer1[adjacent].z += lowForce;
                        buffer2[adjacent].z += lowForce;
                    }

                    adjacent = vert - width * i - j;
                    if (adjacent > 0 && adjacent < buffer1.Length)
                    {
                        buffer1[adjacent].z += lowForce;
                        buffer2[adjacent].z += lowForce;
                    }

                    adjacent = vert + width * i + j;
                    if (adjacent > 0 && adjacent < buffer1.Length)
                    {
                        buffer1[adjacent].z += lowForce;
                        buffer2[adjacent].z += lowForce;
                    }

                    adjacent = vert + width * i - j;
                    if (adjacent > 0 && adjacent < buffer1.Length)
                    {
                        buffer1[adjacent].z += lowForce;
                        buffer2[adjacent].z += lowForce;
                    }
                }
            }
        }

        public float GetMaxWavePropagationVelocity()
        {
            return verticesDistance * Mathf.Sqrt(viscosity * timeSlice + 2) / (2 * timeSlice);
        }

        public float GetMaxTimeSlice()
        {
            return (viscosity + Mathf.Sqrt(viscosity * viscosity + 32 * viscosity * viscosity / (verticesDistance * verticesDistance))) / (8 * wavePropagationVelocity * wavePropagationVelocity / (verticesDistance * verticesDistance));
        }

        private void InitializeBuffers()
        {
            int a = 0;
            for (int j = 0; j < height; j++)
            {
                float y = verticesDistance * j;
                for (int i = 0; i < width; i++)
                {
                    buffer1[a].Set(verticesDistance * i, y, 0);
                    buffer2[a] = buffer1[a];
                    normal[a].Set(0, 0, 2 * verticesDistance);
                    tangent[a].Set(2 * verticesDistance, 0, 0);
                    a++;
                }
            }
        }

        private void ComputeCoefficients()
        {
            float f1 = wavePropagationVelocity * wavePropagationVelocity * timeSlice * timeSlice / (verticesDistance * verticesDistance);
            float f2 = 1 / (viscosity * timeSlice + 2);
            k1 = (4 - 8 * f1) * f2;
            k2 = (viscosity * timeSlice - 2) * f2;
            k3 = 2 * f1 * f2;
        }

        private void DisplaceVertices()
        {
            Vector3[] currentBuffer = renderBuffer == 0 ? buffer1 : buffer2;
            Vector3[] previousBuffer = renderBuffer == 1 ? buffer1 : buffer2;

            for (int j = 1; j < height - 1; j++)
                for (int i = 1; i < width - 1; i++)
                {
                    previousBuffer[i + j * width].z = k1 * currentBuffer[i + j * width].z + k2 * previousBuffer[i + j * width].z +
                    k3 * (currentBuffer[i + 1 + j * width].z + currentBuffer[i - 1 + j * width].z +
                    currentBuffer[i + width + j * width].z + currentBuffer[i - width + j * width].z);
                }
        }

        private void SwapBuffers()
        {
            renderBuffer = 1 - renderBuffer;
        }

        private void CalculateNormalsAndTangents()
        {
            Vector3[] nextBuffer = renderBuffer == 0 ? buffer1 : buffer2;

            for (int j = 1; j < height - 1; j++)
                for (int i = 1; i < width - 1; i++)
                {
                    normal[i + j * width].x = nextBuffer[i - 1 + j * width].z - nextBuffer[i + 1 + j * width].z;
                    normal[i + j * width].y = nextBuffer[i - width + j * width].z - nextBuffer[i + width + j * width].z;
                    tangent[i + j * width].z = nextBuffer[i + 1 + j * width].z - nextBuffer[i - 1 + j * width].z;
                }
        }
    }

    public int widthAndHeight;
    public float distance;
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

    public bool isRaining;

    public bool drawGizmos;

    private Fluid fluid;

    public Texture2D heightMap;
    private MeshRenderer planeRenderer;
    private Material planeMaterial;

    void Awake()
    {
        planeRenderer = GetComponent<MeshRenderer>();
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        Vector3[] currentBuffer = fluid.renderBuffer == 0 ? fluid.buffer1 : fluid.buffer2;
        for (int i = 0; i < currentBuffer.Length; i++)
        {
            Gizmos.color = new Color((currentBuffer[i].x) / 10f, (currentBuffer[i].y) / 10f, 0);
            Gizmos.DrawSphere(new Vector3(currentBuffer[i].x - 5, 0, currentBuffer[i].y - 5), 0.1f);
        }
    }

    void OnValidate()
    {
        if (fluid == null)
            return;

        if (timeSlice <= 0)
        {
            timeSlice = 0.01f;
            fluid.SetTimeSlice(timeSlice);
        }

        if (wavePropagationVelocity <= 0)
        {
            wavePropagationVelocity = 0.01f;
            fluid.SetWavePropagationVelocity(wavePropagationVelocity);
        }

        if (timeSlice >= fluid.GetMaxTimeSlice())
        {
            timeSlice = fluid.GetMaxTimeSlice();
            fluid.SetTimeSlice(timeSlice);
        }

        if (wavePropagationVelocity >= fluid.GetMaxWavePropagationVelocity())
        {
            wavePropagationVelocity = fluid.GetMaxWavePropagationVelocity();
            fluid.SetWavePropagationVelocity(wavePropagationVelocity);
        }
    }

    void Start()
    {
        fluid = new Fluid(widthAndHeight, widthAndHeight, distance, timeSlice, wavePropagationVelocity, viscosity);
        heightMap = new Texture2D(textureResolution, textureResolution, TextureFormat.R8, false);
        heightMap.wrapMode = TextureWrapMode.Clamp;
        heightMap.filterMode = FilterMode.Trilinear;
        planeMaterial = planeRenderer.material;
    }

    void Update()
    {
        if(isRaining)
        {
            float randomX = Random.Range(0f, 1f);
            float randomY = Random.Range(0f, 1f);
            fluid.Turbate(randomX, randomY, rainForce, 2);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            float randomX = Random.Range(0f, 1f);
            float randomY = Random.Range(0f, 1f);
            fluid.Turbate(randomX, randomY, objectEnterForce, 2);
        }
    }

    void FixedUpdate()
    {
        fluid.Evaluate();

        Vector3[] currentBuffer = fluid.renderBuffer == 0 ? fluid.buffer1 : fluid.buffer2;
        Color32[] colors = new Color32[textureResolution * textureResolution];

        for (int i = 1; i < textureResolution - 1; i++)
        {
            float nearestX = Mathf.Lerp(0, (widthAndHeight - 1), Mathf.InverseLerp(0, textureResolution, i));
            int nearestXUp = Mathf.CeilToInt(nearestX);
            int nearestXDown = (int)nearestX;
            float nearestXInterpolator = Mathf.Repeat(nearestX, 1f);

            for (int j = 1; j < textureResolution - 1; j++)
            {
                float nearestY = Mathf.Lerp(0, (widthAndHeight - 1), Mathf.InverseLerp(0, textureResolution, j));
                int nearestYUp = Mathf.CeilToInt(nearestY);
                int nearestYDown = (int)nearestY;
                float nearestYInterpolator = Mathf.Repeat(nearestY, 1f);

                Vector3 currentVertex1 = currentBuffer[nearestXDown * widthAndHeight + nearestYDown];
                Vector3 currentVertex2 = currentBuffer[nearestXDown * widthAndHeight + nearestYUp];
                Vector3 currentVertex3 = currentBuffer[nearestXUp * widthAndHeight + nearestYUp];
                Vector3 currentVertex4 = currentBuffer[nearestXUp * widthAndHeight + nearestYDown];

                float color = Mathf.InverseLerp(-0.2f, 0.2f, Mathf.Lerp(Mathf.Lerp(currentVertex1.z, currentVertex4.z, nearestXInterpolator), Mathf.Lerp(currentVertex2.z, currentVertex3.z, nearestXInterpolator), nearestYInterpolator));
                colors[i * textureResolution + j] = new Color(color, color, color);
            }
        }

        heightMap.SetPixels32(colors);
        heightMap.Apply();
        planeMaterial.mainTexture = heightMap;
    }


    void OnTriggerEnter(Collider other)
    {
        Vector2 fluidCoord = WorldToFluidSpace(other.transform.position.x, other.transform.position.z);
        fluid.Turbate(fluidCoord.x, fluidCoord.y, objectEnterForce, 2);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 fluidCoord = WorldToFluidSpace(other.transform.position.x, other.transform.position.z);
            fluid.Turbate(fluidCoord.x, fluidCoord.y, playerMovingForce, 2);
        }
    }


    private Vector2 WorldToFluidSpace(float worldX, float worldY)
    {
        return new Vector2(
            Mathf.InverseLerp(minPlaneX, maxPlaneX, Mathf.Round((worldX - transform.position.x) / distance) * distance),
            Mathf.InverseLerp(minPlaneY, maxPlaneY, Mathf.Round((worldY - transform.position.z) / distance) * distance));
    }
}
