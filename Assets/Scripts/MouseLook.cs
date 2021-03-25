using UnityEngine;
using System.Collections;
public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityHor;
    public float sensitivityVert;
    public float minimumVert;
    public float maximumVert;
    private float _rotationX;
    private float cameraDistance_;
    void Start()
    {
        cameraDistance_ = (transform.position - transform.parent.position).magnitude;
        _rotationX = transform.rotation.eulerAngles.x;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
            body.freezeRotation = true;
    }

    private void VerticalOrbit(float angle)
    {
        Vector3 pivot = Quaternion.Euler(angle, 0, 0) * new Vector3(0,
            0,
            -cameraDistance_);
        pivot += transform.parent.position;
        transform.position = pivot;
    }
    private void HorizontalOrbit(float angle)
    {
        Vector3 pivot = Quaternion.Euler(0, angle, 0) * new Vector3(transform.position.x - transform.parent.position.x,
           0,
           transform.position.z - transform.parent.position.z);

        pivot.x += transform.parent.position.x;
        pivot.y = transform.position.y;
        pivot.z += transform.parent.position.z;
        transform.position = pivot;
    }

    void Update()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
        _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);
        float rotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityHor;
        transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
        
        VerticalOrbit(_rotationX);
        HorizontalOrbit(rotationY);
    }
}