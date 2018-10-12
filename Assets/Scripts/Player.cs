using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

public class Player : PlayerBehavior
{
    public Vector3 ThrusterForceTranslate;
    public Vector3 ThrusterForceRotate;
    private bool AutoStabilize;
    public float Mass;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = Mass;
        AutoStabilize = false;
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();

        if (!networkObject.IsOwner)
        {
            //Don't need cameras or audiolisteners for other players
            //GetComponent<Camera>().enabled = false;
            //GetComponent<AudioListener>().enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Stabilize"))
        {
            AutoStabilize = AutoStabilize == true ? false : true;
        }
    }

    private void FixedUpdate()
    {
        if (!networkObject.IsOwner)
        {
            transform.position = networkObject.Position;
            transform.rotation = networkObject.Rotation;
            return;
        }

        //transform.Translate(translation * Time.deltaTime * maneuverSpeed);
        Vector3 totalForce = new Vector3
            (
                Input.GetAxis("Sway"),
                Input.GetAxis("Heave"),
                Input.GetAxis("Surge")
            );
        totalForce.Scale(ThrusterForceTranslate);

        rb.AddRelativeForce(totalForce, ForceMode.Acceleration);

        Vector3 totalTorque = new Vector3
            (
                Input.GetAxis("Pitch"),
                Input.GetAxis("Roll"),
                Input.GetAxis("Yaw")
            );
        totalTorque.Scale(ThrusterForceRotate);

        if (AutoStabilize)
        {
            // I have no idea why unity doesn't provide this in relative....
            Vector3 LocalAngularvelocity = transform.InverseTransformVector(rb.angularVelocity);
            totalTorque += new Vector3
                (
                    CalculateNumberSign(LocalAngularvelocity.x, ThrusterForceRotate.x),
                    CalculateNumberSign(LocalAngularvelocity.y, ThrusterForceRotate.y),
                    CalculateNumberSign(LocalAngularvelocity.z, ThrusterForceRotate.z)
                );
            //Debug.Log(rb.angularVelocity.ToString() + " " + totalTorque.ToString());
        };

        rb.AddRelativeTorque(totalTorque, ForceMode.Acceleration);

        // Update network folk
        networkObject.Position = transform.position;
        networkObject.Rotation = transform.rotation;
    }

    // When stabilizing Rigidbody.angularVelocity, we need to add a Vector3 to the torque.
    // This produces something that can be += to an existing Vector3 force (or 0,0,0)
    // to produce a modified force as if thrusters were firing to undo your momentum.
    private float CalculateNumberSign(float number,  float thrust)
    {
        float smaller = Mathf.Abs(number) >= thrust ? thrust : number;
        // This performs something like 400% better than float.CompareTo(0) in testing
        if (number > 0)
            return  smaller * -1f;
        else if (number < 0)
            return Mathf.Abs(smaller);
        else
            return 0;
    }
}
