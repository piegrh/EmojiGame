using System.Collections.Generic;
using UnityEngine;

public class PathRenderer 
{
    public static LineRenderer[] Render(List<KeyValuePair<Vector3, Vector3>> path)
    {
        return Render(path, DefaultLineRenderer());
    }

    public static LineRenderer[] Render(List<KeyValuePair<Vector3, Vector3>> path, LineRenderer r)
    {
        List<LineRenderer> rendes = new List<LineRenderer>();
        foreach (KeyValuePair<Vector3, Vector3> kp in path)
            rendes.Add(Render(kp.Key, kp.Value,Object.Instantiate(r,r.transform.parent)));
        return rendes.ToArray();
    }

    public static LineRenderer Render(Vector3 a, Vector3 b)
    {
        LineRenderer r = DefaultLineRenderer();
        Render(a, b,r);
        return r;
    }

    public static LineRenderer Render(Vector3 a, Vector3 b, LineRenderer r)
    {
        r.positionCount = 2;
        r.SetPosition(0, a);
        r.SetPosition(1, b);
        return r;
    }

    protected static LineRenderer DefaultLineRenderer()
    {
        LineRenderer r = new GameObject().AddComponent<LineRenderer>();
        r.startWidth = 0.3f;
        r.endWidth = 0.3f;
        r.sortingOrder = 1;
        r.material = new Material(Shader.Find("Sprites/Default"));
        return r;
    }
}
