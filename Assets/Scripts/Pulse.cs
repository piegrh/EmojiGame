using UnityEngine;

public class Pulse : MonoBehaviour
{
    public Vector3 amp;
    public Vector3 freq;
    public Vector3 offset;
    [HideInInspector] public Vector3 scale;
    Transform _transform;
    
    void Start()
    {
        _transform = transform;
        scale = _transform.localScale;
    }

    void Update()
    {
        _transform.localScale = new Vector3(
            scale.x + PulseFunc(Time.time, freq.x, amp.x, offset.x),
            scale.y + PulseFunc(Time.time, freq.y, amp.y, offset.y), 
            scale.z + PulseFunc(Time.time, freq.z, amp.z, offset.z));
    }

    protected static float PulseFunc(float time, float freq, float amp, float offset)
    {
        return (amp * (1 + Mathf.Sin(((1f * Mathf.PI * freq * time) + offset))));
    }

    private void OnDisable()
    {
        _transform.localScale = scale;
    }
}
