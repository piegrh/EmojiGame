using UnityEngine;

public class Roll : MonoBehaviour
{
    public float anagle = 45f;
    public float speed = 10f;

    Transform _transform;
    public void Awake()
    {
        _transform = transform;
    }

    void Update()
    {
        _transform.rotation = Quaternion.Euler(0, 0, anagle * Mathf.Sin(Time.time * speed));
    }

    private void OnDisable()
    {
        _transform.rotation = Quaternion.identity;
    }
}
