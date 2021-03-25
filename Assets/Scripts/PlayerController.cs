using UnityEngine;
using System;
using System.Collections;
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 6.0f; 
    public float jumpForce = 15.0f;
    public float terminalVelocity = -10.0f;
    public float minFall = 15.0f;
    public new Camera camera;
    public float gravity = -9.8f;
    
    private float gravityHiddenScale_ = 5.0f;
    private CharacterController charController_;
    private float vertSpeed_;
    private bool needRespawn_ = false;
    private float fireOrbLifetime = 2.0f;
    private float fireballLifetime = 2.0f;
    private float fireballLaunchDelay = 0.25f;
    private Vector3 respawnPoint_ = new Vector3(0, 6, 3);
    private float fireballSlowScale_ = 0.4f;
    private float playerWeaponOffsetRevertScale_ = 20.0f;
    private float laserConstractionPerFrame_ = 0.005f;
    private float fireOrbVectForceScale_ = 1.2f;

    void Start()
    {
        vertSpeed_ = minFall;
        charController_ = GetComponent<CharacterController>();
    }

    void Update()
    {
        //Orb launch [begin]
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ShootFireOrb());
        }
        //Orb launch [end]


        //Projectile shoot [begin]
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ShootFireball());
        }
        //Projectile shoot [end]


        //Ray shoot [begin]
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Vector3 point = -camera.gameObject.transform.localPosition;
            StartCoroutine(ShootLaser(point));
        }
        //Ray shoot [end]

        //WASD and Space [begin]
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, speed);

        movement = Quaternion.Euler(0, camera.transform.localEulerAngles.y, 0) * movement;
        
        if (charController_.isGrounded)
            if (Input.GetKeyDown(KeyCode.Space))
                vertSpeed_ = jumpForce;
            else
                vertSpeed_ = minFall;
        else
        {
            vertSpeed_ += gravity * gravityHiddenScale_ * Time.deltaTime;
            if (vertSpeed_ < terminalVelocity)
            {
                vertSpeed_ = terminalVelocity;
            }
        }

        movement.y = vertSpeed_;
        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        
        charController_.Move(movement);
        //WASD and Space [end]
    }

    private IEnumerator ShootFireOrb()
    {
        GameObject fireOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        fireOrb.transform.position = transform.position - camera.gameObject.transform.localPosition / playerWeaponOffsetRevertScale_;
        
        fireOrb.AddComponent<Rigidbody>();
        fireOrb.GetComponent<Rigidbody>().AddForce(-camera.gameObject.transform.localPosition, ForceMode.Impulse);
        fireOrb.GetComponent<Rigidbody>().AddForce(-gravity * fireOrbVectForceScale_ *  Vector3.up, ForceMode.Impulse);
        fireOrb.GetComponent<Rigidbody>().mass = 5.0f;

        yield return new WaitForSeconds(fireOrbLifetime);
        
        Destroy(fireOrb);
    }

    private IEnumerator CreateFireball()
    {
        GameObject profectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        Destroy(profectile.GetComponent<SphereCollider>());
        profectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        profectile.transform.position = transform.position - camera.gameObject.transform.localPosition / playerWeaponOffsetRevertScale_;
        //profectile.transform.localPosition += Quaternion.Euler(0, camera.transform.localEulerAngles.y, 0) * Vector3.right;

        profectile.AddComponent<Rigidbody>();
        profectile.GetComponent<Rigidbody>().useGravity = false;
        profectile.GetComponent<Rigidbody>().AddForce(fireballSlowScale_ * (-camera.gameObject.transform.localPosition), ForceMode.Impulse);

        yield return new WaitForSeconds(fireballLifetime);

        Destroy(profectile);
    }

    private IEnumerator ShootFireball()
    {
        StartCoroutine(CreateFireball());
        yield return new WaitForSeconds(fireballLaunchDelay);
        StartCoroutine(CreateFireball());
    }

    private IEnumerator ShootLaser(Vector3 point)
    {
        GameObject laserCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        
        Destroy(laserCylinder.GetComponent<CapsuleCollider>());
        
        laserCylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, -camera.gameObject.transform.localPosition);
        laserCylinder.transform.localPosition = transform.position + point / 2;
        laserCylinder.transform.position -= camera.gameObject.transform.localPosition / playerWeaponOffsetRevertScale_;
        laserCylinder.transform.localScale = new Vector3(0.5f, point.magnitude / 2, 0.5f);
        
        while (laserCylinder.transform.localScale.x > 0)
        {
            laserCylinder.transform.localScale = new Vector3(laserCylinder.transform.localScale.x - laserConstractionPerFrame_,
                laserCylinder.transform.localScale.y,
                laserCylinder.transform.localScale.z - laserConstractionPerFrame_);
            yield return null;
        }
        
        Destroy(laserCylinder);
    }

    /*
    private IEnumerator SphereIndicator(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(sphere.GetComponent<SphereCollider>());
        sphere.transform.position = pos;
        yield return new WaitForSeconds(1);
        Destroy(sphere);
    }
    */
    
    void LateUpdate()
    {
        if (needRespawn_)
        {
            charController_.enabled = false;
            transform.position = respawnPoint_;
            charController_.enabled = true;
            needRespawn_ = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Abyss"))
        {
            needRespawn_ = true;
        }
    }
}