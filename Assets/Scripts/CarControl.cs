using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json.Serialization.Metadata;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask driveable;
    [SerializeField] private GameObject[] wheelVisuals;
    [SerializeField] private GameObject[] TireParents = new GameObject[2];
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private InputActionReference throttleAction;
    [SerializeField] private InputActionReference steeringAction;
    [SerializeField] private float visualSteeringSpeed = 90f;
    [SerializeField] private float tireRotation = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;
    [SerializeField] private TrailRenderer[] BlackStuff = new TrailRenderer[2];
    [SerializeField] private ParticleSystem[] Smoke = new ParticleSystem[2];
    [SerializeField] private InputActionReference brakeAction;
    [SerializeField] private float brakeStrength = 5f;
    private float currentSteeringAngle;

    [Header("Suspension")]
    [SerializeField] private float RestSpringDistance;
    [SerializeField] private float SpringStiffness;
    [SerializeField] private float SpringOffset;
    [SerializeField] private float wheelRadius = 0.441f;
    [SerializeField] private float DamperStiffness;

    [Header("Car")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float MaxSpeed = 100f;
    [SerializeField] private float steeringStrength = 15f;
    [SerializeField] AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1.0f;
    [SerializeField] private float MinimumSideVelocitySmoke = 10f;

    [Header("Audio")]
    [SerializeField] AudioSource EngineSound;
    [SerializeField] AudioSource DriftSound;
    [SerializeField] [Range(0, 1)] private float minPitch;
    [SerializeField] [Range(1,5)] private float maxPitch;

    private Vector3 CurrentCarVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    private float throttleInput;
    private float steeringInput;
    private float brakeInput;

    private bool[] wheelIsGrounded;
    private bool isAllGrounded = false;
    private bool carStopped = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheelIsGrounded = new bool[rayPoints.Length];
    }

    private void OnEnable()
    {
        throttleAction.action.Enable();
        steeringAction.action.Enable();
        brakeAction.action.Enable();
    }

    private void OnDisable()
    {
        throttleAction.action.Disable();
        steeringAction.action.Disable();
        brakeAction.action.Disable();
    }

    private void Update()
    {
        if (carStopped)
        {
            throttleInput = 0;
            steeringInput = 0;
            return;
        }

        throttleInput = throttleAction.action.ReadValue<float>();
        steeringInput = steeringAction.action.ReadValue<float>();
        brakeInput = brakeAction.action.ReadValue<float>();
    }
    private void FixedUpdate()
    {
        Suspension();

        if (!controlsEnabled) return;

        GroundCheck();
        CalculateVelocity();

        if (isAllGrounded && !carStopped)
        {
            Aceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
            Brake();
        }

        RotateWheels();
        VFX();
        EnginePitch();
    }

    private void GroundCheck()
    {
        int numGroundedWheels = 0;

        foreach (bool t in wheelIsGrounded)
        {
            if (t)
            {
                numGroundedWheels++;
            }
        }

        if (numGroundedWheels > 1)
        {
            isAllGrounded = true;
        }
        else
        {
            isAllGrounded = false;
        }
    }

    private void RotateWheels()
    {
        float targetSteeringAngle = maxSteeringAngle * steeringInput;

        currentSteeringAngle = Mathf.MoveTowards(currentSteeringAngle, targetSteeringAngle, visualSteeringSpeed * Time.fixedDeltaTime);

        for (int i = 0; i < wheelVisuals.Length; i++)
        {
            if (i < 2)
            {
                wheelVisuals[i].transform.Rotate(Vector3.right, tireRotation * carVelocityRatio * Time.fixedDeltaTime, Space.Self);

                TireParents[i].transform.localEulerAngles = new Vector3(TireParents[i].transform.localEulerAngles.x, currentSteeringAngle, TireParents[i].transform.localEulerAngles.z);
            }
            else
            {
                wheelVisuals[i].transform.Rotate(Vector3.right, tireRotation * carVelocityRatio * Time.fixedDeltaTime, Space.Self);
            }
        }
    }

    private void EnginePitch()
    {
        EngineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(carVelocityRatio));
    }

    private void VFX()
    {
        bool drifting = Mathf.Abs(CurrentCarVelocity.x) > MinimumSideVelocitySmoke;
        bool braking = brakeInput > 0 && Mathf.Abs(CurrentCarVelocity.z) > 2f;

        if (isAllGrounded && (drifting || braking))
        {
            DriftSound.mute = false;

            foreach (TrailRenderer trail in BlackStuff)
            {
                trail.emitting = true;
            }

            foreach (ParticleSystem particle in Smoke)
            {
                particle.Play();
            }
        }
        else
        {
            DriftSound.mute = true;

            foreach (TrailRenderer trail in BlackStuff)
            {
                trail.emitting = false;
            }

            foreach (ParticleSystem particle in Smoke)
            {
                particle.Stop();
            }
        }
    }

    private void CalculateVelocity()
    {
        CurrentCarVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        carVelocityRatio = CurrentCarVelocity.z / MaxSpeed;
    }

    private void Aceleration()
    {
        if (throttleInput > 0 && CurrentCarVelocity.z < MaxSpeed)
        {
            rb.AddForceAtPosition(acceleration * throttleInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
        }
    }

    private void Deceleration()
    {
        if (throttleInput < 0)
        {
            rb.AddForceAtPosition(deceleration * -throttleInput * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
        }
    }

    private void Turn()
    {
        rb.AddTorque(steeringStrength * steeringInput * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        float CurrentSidewaysSpeed = CurrentCarVelocity.x;

        rb.AddForceAtPosition(transform.right * -CurrentSidewaysSpeed * dragCoefficient, rb.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float SpringMaxLength = RestSpringDistance + SpringOffset;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, SpringMaxLength + wheelRadius, driveable))
            {
                wheelIsGrounded[i] = true;

                float CurrSpringDistance = hit.distance - wheelRadius;
                float CurrentStiffness = RestSpringDistance - CurrSpringDistance;

                float HowMuchCompressed = CurrentStiffness / SpringOffset;

                float SpringForce = HowMuchCompressed * SpringStiffness;

                float SpringVelocity = Vector3.Dot(rayPoints[i].up, rb.GetPointVelocity(rayPoints[i].position));
                float DampForce = SpringVelocity * DamperStiffness;

                wheelVisuals[i].transform.position = hit.point + rayPoints[i].up * wheelRadius;

                rb.AddForceAtPosition((SpringForce - DampForce) * rayPoints[i].up, rayPoints[i].position);
            }
            else
            {
                wheelIsGrounded[i] = false;
                wheelVisuals[i].transform.position = hit.point - rayPoints[i].up * SpringMaxLength;
            }
        }
    }

    public void StopCar()
    {
        carStopped = true;

        throttleInput = 0;
        steeringInput = 0;

        DriftSound.mute = true;

        foreach (TrailRenderer trail in BlackStuff)
        {
            trail.emitting = false;
        }

        foreach (ParticleSystem particle in Smoke)
        {
            particle.Stop();
        }
    }
    private bool controlsEnabled = true;

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
    }

    private void Brake()
    {
        if (brakeInput > 0)
        {
            rb.AddForceAtPosition(-transform.forward * CurrentCarVelocity.z * brakeStrength * brakeInput, accelerationPoint.position, ForceMode.Acceleration);
        }
    }
}