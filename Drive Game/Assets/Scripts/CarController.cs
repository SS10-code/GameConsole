using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;
    public WheelColliders wheelColliders;
    public WheelMeshes wheelMeshes;
    public WheelParticles wheelParticles;  
    public float accelInput;
    public float brakeInput;
    public float steerInput;
    public GameObject smokePrefab;

    public float motorPower;
    public float brakePower;
    private float slipAngle;

    public float speed;
    public AnimationCurve steeringCurve;
    // Start is called before the first frame update
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Start()
    {
        InstaniateSmoke();
    }

    void InstaniateSmoke()
    {
        wheelParticles.frontLeft = Instantiate(smokePrefab, wheelColliders.frontLeft.transform.position-Vector3.up*wheelColliders.frontLeft.radius, Quaternion.identity, wheelColliders.frontLeft.transform).GetComponent<ParticleSystem>();
        wheelParticles.frontRight = Instantiate(smokePrefab, wheelColliders.frontRight.transform.position - Vector3.up * wheelColliders.frontLeft.radius, Quaternion.identity, wheelColliders.frontRight.transform).GetComponent<ParticleSystem>();
        wheelParticles.rearLeft = Instantiate(smokePrefab, wheelColliders.rearLeft.transform.position - Vector3.up * wheelColliders.frontLeft.radius, Quaternion.identity, wheelColliders.rearLeft.transform).GetComponent<ParticleSystem>();
        wheelParticles.rearRight = Instantiate(smokePrefab, wheelColliders.rearRight.transform.position - Vector3.up * wheelColliders.frontLeft.radius, Quaternion.identity, wheelColliders.rearRight.transform).GetComponent<ParticleSystem>();
    }

    void CheckInput()
    {
        speed = rb.velocity.magnitude;
        accelInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        slipAngle = Vector3.Angle(transform.forward, rb.velocity);

        if (slipAngle < 120f)
        {
            if(accelInput<0)
            {
                brakeInput = -accelInput;
                accelInput = 0;
            }
        }
        else
        {
            brakeInput = 0;
        }
    }

    void ApplyBrake()
    {
        wheelColliders.frontLeft.brakeTorque = brakeInput * brakePower *0.7f;

        wheelColliders.frontRight.brakeTorque = brakeInput * brakePower*0.7f;

        wheelColliders.rearLeft.brakeTorque = brakeInput * brakePower*0.3f;

        wheelColliders.rearRight.brakeTorque = brakeInput * brakePower * 0.3f;
    }

    void ApplyMotor()
    {
        wheelColliders.rearLeft.motorTorque = accelInput * motorPower;
        wheelColliders.rearRight.motorTorque = accelInput * motorPower;
    }

    void ApplySteering()
    {
        float steeringAngle = steerInput * steeringCurve.Evaluate(speed);
        steeringAngle+=Vector3.SignedAngle(transform.forward, rb.velocity+transform.forward, Vector3.up);
        Mathf.Clamp(steeringAngle, -90f, 90f);
        wheelColliders.frontLeft.steerAngle = steeringAngle;
        wheelColliders.frontRight.steerAngle = steeringAngle;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckInput();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        CheckParticles();
        ApplyWheelPositions();
        
    }

    void ApplyWheelPositions()
    {
        UpdateWheel(wheelColliders.frontLeft, wheelMeshes.frontLeft);
        UpdateWheel(wheelColliders.frontRight, wheelMeshes.frontRight);
        UpdateWheel(wheelColliders.rearLeft, wheelMeshes.rearLeft);
        UpdateWheel(wheelColliders.rearRight, wheelMeshes.rearRight);
    }

    void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];

        wheelColliders.frontLeft.GetGroundHit(out wheelHits[0]);
        wheelColliders.frontRight.GetGroundHit(out wheelHits[1]);
        wheelColliders.rearLeft.GetGroundHit(out wheelHits[2]);
        wheelColliders.rearRight.GetGroundHit(out wheelHits[3]);

        float slipThreshold = 0.5f;

        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipThreshold))
            wheelParticles.frontLeft.Play();
        else
            wheelParticles.frontLeft.Stop();

        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipThreshold))
            wheelParticles.frontRight.Play();
        else
            wheelParticles.frontRight.Stop();

        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipThreshold))
            wheelParticles.rearLeft.Play();
        else
            wheelParticles.rearLeft.Stop();

        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipThreshold))
            wheelParticles.rearRight.Play();
        else
            wheelParticles.rearRight.Stop();

    }

    void UpdateWheel(WheelCollider wheelCollider, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 pos;

        wheelCollider.GetWorldPose(out pos, out quat);

        wheelMesh.transform.position = pos;
        wheelMesh.transform.rotation = quat;
    }
}
[System.Serializable]
public class WheelColliders
{
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;
}

[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer frontLeft;
    public MeshRenderer frontRight;
    public MeshRenderer rearLeft;
    public MeshRenderer rearRight;
}
[System.Serializable]
public class WheelParticles
{
    public ParticleSystem frontLeft;
    public ParticleSystem frontRight;
    public ParticleSystem rearLeft;
    public ParticleSystem rearRight;
}
