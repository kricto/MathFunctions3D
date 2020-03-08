using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform pointPrefab;
    [Range(10, 100)]
    public int resolution = 10;

    public FunctionType functionType = FunctionType.Sine;

    private List<Transform> points = new List<Transform>();

    public delegate Vector3 GraphFunction(float u, float v, float t);

    private const float pi = Mathf.PI;

    private GraphFunction[] functions =
    {
        SineFunction,
        MultiSineFunction,
        Sine2DFunction,
        MultiSine2DFunction,
        RippleFunction,
        CylinderFunction,
        SphereFunction,
        TorusFunction,
    };

    public enum FunctionType
    {
        Sine,
        MultiSine,
        Sine2D,
        MultiSine2D,
        Ripple,
        Cylinder,
        Sphere,
        Torus,
    }

    private void Awake()
    {
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;

        for (int i = 0; i < resolution * resolution; i++)
        {
            Transform point = Instantiate(pointPrefab);
            
            point.localScale = scale;
            point.SetParent(transform, false);
            
            points.Add(point);
        }
    }

    private async void Update()
    {
        float time = Time.time;
        GraphFunction f = functions[(int)functionType];

        float step = 2f / resolution;

        await LoopAsync(resolution, step, f, time);
    }

    public async Task LoopAsync(int resolution, float step, GraphFunction f, float time)
    {
        List<Task> listOfTasks = new List<Task>();

        for (int i = 0, z = 0; z < resolution; z++)
        {
            float v = (z + 0.5f) * step - 1f;
            for (int x = 0; x < resolution; x++, i++)
            {
                float u = (x + 0.5f) * step - 1f;
                listOfTasks.Add(DoAsync(i, f, u, v, time));
            }
        }

        await Task.WhenAll(listOfTasks);
    }

    public Task DoAsync(int i, GraphFunction f, float u, float v, float time)
    {
        points[i].localPosition = f(u, v, time);

        return Task.CompletedTask;
    }

    public static float Sin(float x, float t = .0f, float pi = pi)
    {
        return Mathf.Sin(pi * (x + t));
    }

    public static float Cos(float x, float t = .0f, float pi = pi)
    {
        return Mathf.Cos(pi * (x + t));
    }

    public static Vector3 SineFunction(float x, float z, float t)
    {
        Vector3 p;

        p.x = x;
        p.y = Sin(x, t);
        p.z = z;

        return p;
    }

    public static Vector3 MultiSineFunction(float x, float z, float t)
    {
        Vector3 p;
        
        p.x = x;
        p.y = Sin(x, t);
        p.y += Sin(x, 2f * t, 2f * pi) / 2f;
        p.y *= 2f / 3f;
        p.z = z;

        return p;
    }

    public static Vector3 Sine2DFunction(float x, float z, float t)
    {
        Vector3 p;

        p.x = x;
        p.y = Sin(x, t);
        p.y += Sin(z, t);
        p.y *= 0.5f;
        p.z = z;
        
        return p;
    }

    public static Vector3 MultiSine2DFunction(float x, float z, float t)
    {
        Vector3 p;

        p.x = x;
        p.y = 4f * Sin(x + z, t / 2f);
        p.y += Sin(x, t);
        p.y += Sin(z, 2f * t, 2f * pi) * 0.5f;
        p.y *= 1f / 5.5f;
        p.z = z;
        
        return p;
    }

    public static Vector3 RippleFunction(float x, float z, float t)
    {
        Vector3 p;
        
        float d = Mathf.Sqrt(x * x + z * z);

        p.x = x;
        p.y = Sin(4f * d, -t);
        p.y /= 1f + 10f * d;
        p.z = z;

        return p;
    }

    public static Vector3 CylinderFunction(float u, float v, float t)
    {
        Vector3 p;

        float r = .8f + Sin(6f * u + 2f * v, t) * .2f;

        p.x = r * Sin(u);
        p.y = v;
        p.z = r * Cos(u);

        return p;
    }

    public static Vector3 SphereFunction(float u, float v, float t)
    {
        Vector3 point;

        float radius = .8f + Sin(6f * u, t) * .1f;
        radius += Sin(4f * v, t) * .1f;

        float s = radius * Cos(.5f * v);
        
        point.x = s * Sin(u);
        point.y = radius * Sin(.5f * v);
        point.z = s * Cos(u);

        return point;
    }

    public static Vector3 TorusFunction(float u, float v, float t)
    {
        Vector3 point;

        //float r1 = 1f; //Simple torus radiuses
        //float r2 = .5f;

        float r1 = .65f + Sin(6f * u, t) * .1f;
        float r2 = .2f + Sin(4f * v, t) * .05f;

        float s = r2 * Cos(v) + r1;

        point.x = s * Sin(u);
        point.y = r2 * Sin(v);
        point.z = s * Cos(u);

        return point;
    }

    public void SwitchFunction(string type)
    {
        Enum.TryParse(type, out FunctionType function);
        functionType =  function;
    }
}
