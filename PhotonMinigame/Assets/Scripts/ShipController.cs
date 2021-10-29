using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public float speed = 20f;
    public float baseSpeed = 20f;

    //if the ship is being piloted by an ai or a player (default is false)
    public bool ai_pilot = false;

    ////astronaut team
    public TeamMaterial teamMat;

    //the teamID of this ship
    //team 0 is neutral
    public int teamID
    {
        get => teamMat.teamID;
    }
    int oldID = 0;

    //whether or not the spacecraft is landed
    public bool landed = true;

    //reference to ship animator
    public Animator animator;

    //reference to ship renderer
    public MeshRenderer shipBodyRenderer;

    //reference to ship rigidbody
    public Rigidbody shipBody;

    //list of forces added to ship
    public List<KeyControl> pressedKeys = new List<KeyControl>();

    //ray position
    public Transform rayOrigin;
    //distance sensor on bottom of ship. For landing
    public FloorSensor floorSensor
    {
        get
        {
            return new FloorSensor(new Ray(rayOrigin.position, LocalDir(transform, Vector3.down)), 5f);
        }
    }

    //final force to be added to ship
    public Vector3 desiredForce = Vector3.zero;
    //final torque to be added to ship
    public Vector3 desiredTorque = Vector3.zero;
    public bool canLand
    {
        get
        {
            return !landed && floorSensor.distance < 1;
        }
    }

    //list of bound keys
    List<KeyControl> boundKeys = new List<KeyControl>();

    //dictionary key to code
    Dictionary<KeyCode, KeyControl> key2Control = new Dictionary<KeyCode, KeyControl>();

    public static Vector3 LocalDir(Transform transform, Vector3 worldDir)
    {
        return transform.rotation * worldDir;
    }

    KeyControl forward;
    KeyControl back;
    KeyControl left;
    KeyControl right;
    KeyControl up;
    KeyControl down;
    KeyControl pitchUp;
    KeyControl pitchDown;
    KeyControl rollLeft;
    KeyControl rollRight;
    KeyControl yawLeft;
    KeyControl yawRight;
    KeyControl breaks;
    KeyControl takeoff;
    KeyControl turbo;

    public Transform resetPosition;

    private void Awake()
    {
        //set bound keys
        //directions
        forward = new KeyControl(KeyCode.W, () => AddForce(Vector3.forward), () => AddForce(-Vector3.forward));
        back = new KeyControl(KeyCode.S, () => AddForce(Vector3.back), () => AddForce(-Vector3.back));
        left = new KeyControl(KeyCode.A, () => AddForce(Vector3.left), () => AddForce(-Vector3.left));
        right = new KeyControl(KeyCode.D, () => AddForce(Vector3.right), () => AddForce(-Vector3.right));
        down = new KeyControl(KeyCode.LeftControl, () => AddForce(Vector3.down), () => AddForce(-Vector3.down));
        up = new KeyControl(KeyCode.Space, () => AddForce(Vector3.up), () => AddForce(-Vector3.up));
        pitchUp = new KeyControl(KeyCode.DownArrow, () => AddForce(Vector3.left, true), () => AddForce(-Vector3.left, true));
        pitchDown = new KeyControl(KeyCode.UpArrow, () => AddForce(Vector3.right, true), () => AddForce(-Vector3.right, true));
        rollLeft = new KeyControl(KeyCode.Q, () => AddForce(Vector3.forward, true), () => AddForce(-Vector3.forward, true));
        rollRight = new KeyControl(KeyCode.E, () => AddForce(Vector3.back, true), () => AddForce(-Vector3.back, true));
        yawLeft = new KeyControl(KeyCode.LeftArrow, () => AddForce(Vector3.down, true), () => AddForce(-Vector3.down, true));
        yawRight = new KeyControl(KeyCode.RightArrow, () => AddForce(Vector3.up, true), () => AddForce(-Vector3.up, true));
        //breaks = new KeyControl(KeyCode.R, () => Breaks(), () => { });
        takeoff = new KeyControl(KeyCode.F, () => ToggleTakeOff(), () => { });
        turbo = new KeyControl(KeyCode.LeftShift, () => speed += 10, () => speed -= 10);

        boundKeys = new List<KeyControl> {
            forward,
            back,
            left,
            right,
            up,
            down,
            pitchUp,
            pitchDown,
            rollLeft,
            rollRight,
            yawLeft,
            yawRight,
            //breaks,
            takeoff,
            turbo
        };

        //set colors
        SetColors();

    }

    // Start is called before the first frame update
    void Start()
    {
        //ship starts landed
        shipBody.constraints = RigidbodyConstraints.FreezeAll;

        oldID = teamID;
    }

    // Update is called once per frame
    void Update()
    {
        //check if team has changed
        if (oldID != teamID)
        {
            //team change
            //change colors
            SetColors();
        }

        //check if there is a player
        if (!ai_pilot)
        {
            //get player input
            foreach (var key in boundKeys)
            {
                //Debug.Log("Checking: "+key);
                //if key is pressed and is not already pressed, add key to pressed keys
                if (Input.GetKeyDown(key.key) && !pressedKeys.Contains(key))
                {
                    //Debug.Log("Keydown: "+key.key);
                    //execute key down action
                    key.keyDown.Invoke();
                    //add key to list of pressed keys
                    pressedKeys.Add(key);
                }

                if (Input.GetKeyUp(key.key) && pressedKeys.Contains(key))
                {
                    //Debug.Log("Keyup: "+key.key);
                    //execute key up action
                    key.keyUp.Invoke();
                    //remove key from list of pressed keys
                    pressedKeys.Remove(key);
                }
            }
        }
    }

    private void FixedUpdate()
    {

        //apply forces to rigidbody
        shipBody.velocity = Vector3.Lerp(shipBody.velocity, LocalDir(transform, desiredForce) * speed, .2f);
        shipBody.angularVelocity = Vector3.Lerp(shipBody.angularVelocity, LocalDir(transform, desiredTorque) * 3, .2f);
    }

    void AddForce(Vector3 force, bool rotation = false)
    {
        if (!landed)
        {
            if (!rotation)
            {
                desiredForce += force;
            }
            else
            {
                desiredTorque += force;
            }
        }
    }

    async void Breaks()
    {
        await Task.Run(() => BreaksAsync());
    }

    void BreaksAsync()
    {
        while (pressedKeys.Contains(breaks))
        {
            Debug.Log("Breaking");
            desiredForce = Vector3.zero;
            desiredTorque = Vector3.zero;
        }
    }

    public void ToggleTakeOff()
    {
        //check if not landed and can land
        if (!landed && canLand)
        {
            Debug.Log("Landing!");
            landed = true;
            //lock ship position
            shipBody.constraints = RigidbodyConstraints.FreezeAll;
            //play landing animation
            PlayAnimation("Land");
        }
        else if (landed)
        {
            Debug.Log("Taking off!");
            landed = false;
            //unlock ship position
            shipBody.constraints = RigidbodyConstraints.None;
            //play takeoff animation
            PlayAnimation("Takeoff");
        }
    }

    void PlayAnimation(string animName, int layer = -1, float normalizedTime = 0)
    {
        animator.Play(animName, layer, normalizedTime);
    }

    void SetAnimationSpeed(float speed)
    {
        //change animation speed
        animator.speed = speed;
    }

    public void ResetShip()
    {
        //remove all forces
        shipBody.velocity = Vector3.zero;
        //make ship landed
        PlayAnimation("landed");
        landed = true;
        //place ship back at beginning
        transform.position = resetPosition.position;
        transform.rotation = resetPosition.rotation;
        shipBody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void SetColors()
    {
        shipBodyRenderer.materials[0].SetColor("TeamColor", TeamManager.instance.teams[teamID]);
        oldID = teamID;
    }

    //ai Dictionary
    Dictionary<int, Vector3> out2force = new Dictionary<int, Vector3>();

    //a function the AI will use to control the ship instead of key-binds
    public void ControlShip(float[] vectorAction)
    {
        //move ship
        Vector3 desiredDirection = Vector3.zero;
        float x = vectorAction[0];
        float y = vectorAction[0];
        float z = vectorAction[0];
    }
}
[System.Serializable]
public class FloorSensor
{
    public Ray ray;
    public float maxDist;
    public float distance
    {
        get
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDist))
            {
                return hit.distance;
            }
            else
            {
                return maxDist;
            }
        }
    }

    public FloorSensor(Ray ray, float maxDist)
    {
        this.ray = ray;
        this.maxDist = maxDist;
    }
}