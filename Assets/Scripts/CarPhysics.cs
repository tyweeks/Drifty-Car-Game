using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    public float speed;
    public float turnSpeed;
    public LayerMask myLayerMask;
    public bool isPlayer = false;
    public GameObject checkPoints;

    private Rigidbody rb;
    private float positionY;
    private direction carDirection;
    private bool initialized = false;
    private NeuralNetwork net;
    private int currentCheckPoint;
    private int currentLap;

    private enum direction
    {
        Forward = 1,
        Backward = -1
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(8, 8);
        currentCheckPoint = 0;
        currentLap = 0;
    }

    void FixedUpdate()
    {
        if (initialized == true)
        {
            float[] inputs = new float[8];

            RaycastHit hit;
            float drawDistance = 10f;

            Ray leftRay = new Ray(transform.position, transform.forward);
            Ray leftForwardRay = new Ray(transform.position, (transform.forward + transform.right).normalized);
            Ray leftForwardForwardRay = new Ray(transform.position, ((transform.forward + transform.right).normalized + transform.right).normalized);
            Ray forwardRay = new Ray(transform.position, transform.right);
            Ray rightForwardForwardRay = new Ray(transform.position, (transform.right + (transform.right + -transform.forward).normalized).normalized);
            Ray rightForwardRay = new Ray(transform.position, (transform.right + -transform.forward).normalized);
            Ray rightRay = new Ray(transform.position, -transform.forward);
            Ray backRay = new Ray(transform.position, -transform.right);
            

            /* Draw Rays
            Debug.DrawRay(transform.position, transform.forward, Color.cyan, 1);
            Debug.DrawRay(transform.position, (transform.forward + transform.right).normalized, Color.green, 1);
            Debug.DrawRay(transform.position, transform.right, Color.magenta, 1);
            Debug.DrawRay(transform.position, (transform.right + -transform.forward).normalized, Color.red, 1);
            Debug.DrawRay(transform.position, -transform.forward, Color.yellow, 1);
            */

            if (Physics.Raycast(leftRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < drawDistance)
                {
                    Debug.DrawLine(transform.position, hit.point);
                }
                inputs[0] = hit.distance;
            }
            if (Physics.Raycast(leftForwardRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < drawDistance)
                {
                    Debug.DrawLine(transform.position, hit.point);
                }
                inputs[1] = hit.distance;
            }
            if (Physics.Raycast(forwardRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < drawDistance)
                {
                    Debug.DrawLine(transform.position, hit.point);
                }
                inputs[2] = hit.distance;
            }
            if (Physics.Raycast(rightForwardRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < drawDistance)
                {
                    Debug.DrawLine(transform.position, hit.point);
                }
                inputs[3] = hit.distance;
            }
            if (Physics.Raycast(rightRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < drawDistance)
                {
                    Debug.DrawLine(transform.position, hit.point);
                }
                inputs[4] = hit.distance;
            }
            /*
            if (Physics.Raycast(backRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < closestHitDistance)
                {
                    closestHitDistance = hit.distance;
                    closestHit = hit.point;
                }
                //if (hit.distance < drawDistance)
                //{
                //    Debug.DrawLine(transform.position, hit.point);
                //}
                inputs[5] = hit.distance;
            }
            */
            if (Physics.Raycast(rightForwardForwardRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < drawDistance)
                {
                    Debug.DrawLine(transform.position, hit.point);
                }
                inputs[6] = hit.distance;
            }
            if (Physics.Raycast(leftForwardForwardRay, out hit, 1000.0f, myLayerMask))
            {
                if (hit.distance < drawDistance)
                {
                    Debug.DrawLine(transform.position, hit.point);
                }
                inputs[7] = hit.distance;
            }

            inputs[5] = rb.velocity.magnitude;

            float[] output = net.FeedForward(inputs);

            if (output[0] > 0)
            {
                Forward();
            }
            if (output[1] > 0)
            {
                Backward();
            }
            if (output[2] > output[3])
            {
                TurnLeft();
            }
            else
            {
                TurnRight();
            }

        }
        if (isPlayer)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                Forward();
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                Backward();
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                TurnLeft();
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                TurnRight();
            }
        }
    }

    void Forward()
    {
        rb.AddRelativeForce(speed, 0.0f, 0.0f);
    }

    void Backward()
    {
        rb.AddRelativeForce(-speed * 0.6f, 0.0f, 0.0f);
    }

    void TurnLeft()
    {
        rb.AddRelativeTorque(0.0f, -turnSpeed * rb.velocity.magnitude  * (float)carDirection, 0.0f);
    }
    void TurnRight()
    {
        rb.AddRelativeTorque(0.0f, turnSpeed * rb.velocity.magnitude * (float)carDirection, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        SetDirection();
    }

    private void SetDirection()
    {
        if (transform.InverseTransformDirection(rb.velocity).x >= 0)
        {
            carDirection = direction.Forward;
        }
        else
        {
            carDirection = direction.Backward;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            if (initialized)
            {
                net.AddFitness(-1f);
                //Destroy(this.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Checkpoint")// && carDirection == direction.Forward)
        {
            if (collider.transform.name == checkPoints.transform.GetChild(currentCheckPoint).name)
            {
                if (currentCheckPoint + 1 < checkPoints.transform.childCount)
                {
                    currentCheckPoint++;
                }
                else
                {
                    currentCheckPoint = 0;
                    currentLap++;
                }

                if (initialized)
                {
                    net.AddFitness(1f);
                    UpdateCarColor(net.GetFitness());
                }
            }
            else // not hitting the correct checkpoint. deduct fitness
            {
                if (initialized)
                {
                    net.AddFitness(-10f);
                    UpdateCarColor(net.GetFitness());
                }
            }
        }
    }

    public void Init(NeuralNetwork net)
    {
        this.net = net;
        initialized = true;
    }

    private void UpdateCarColor(float i)
    {
        foreach(Transform child in transform)
        {
            if(child.name == "Body")
            {
                if (i > 0)
                {
                    child.GetComponent<Renderer>().material.color = new Color(1 - (i / 100), 1, 1 - (i / 100), 1.0f);
                }
            }
        }
    }
}
