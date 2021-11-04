using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool team;

    public static GameObject LocalPlayerInstance;

    public GameObject escMenu;
    public PhotonAnimatorView playerAnimator;
    
    public Rigidbody localPlayerBody
    {
        get
        {
            return LocalPlayerInstance.GetComponentInChildren<Rigidbody>();
        }
    }
    public Camera camera;

    public Transform camPosOffset;
    public Transform camLookOffset;

    public float speed = 40f;
    public float baseSpeed = 40f;

    Vector3 desiredForce = Vector3.zero;
    Vector3 desiredTorque = Vector3.zero;

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
    //new fire control
    KeyControl fire;
    //KeyControl breaks;
    KeyControl engineToggle;
    KeyControl turbo;
    KeyControl esc;

    List<KeyControl> boundKeys = new List<KeyControl>();
    List<KeyControl> pressedKeys = new List<KeyControl>();

    public Rigidbody playerBody;

    //apply thrust to plane
    bool applyThrust = false;

    private void Awake()
    {
        

        if (photonView.IsMine)
        {

            forward = new KeyControl(KeyCode.W, () => { applyThrust = true; AddForce(Vector3.forward); }, () => { AddForce(-Vector3.forward); }) ;
            //back = new KeyControl(KeyCode.S, () => AddForce(Vector3.back), () => AddForce(-Vector3.back));
            //left = new KeyControl(KeyCode.A, () => AddForce(Vector3.left), () => AddForce(-Vector3.left));
            //right = new KeyControl(KeyCode.D, () => AddForce(Vector3.right), () => AddForce(-Vector3.right));
            //down = new KeyControl(KeyCode.LeftControl, () => AddForce(Vector3.down), () => AddForce(-Vector3.down));
            //up = new KeyControl(KeyCode.Space, () => AddForce(Vector3.up), () => AddForce(-Vector3.up));
            pitchUp = new KeyControl(KeyCode.DownArrow, () => AddForce(Vector3.left, true), () => AddForce(-Vector3.left, true));
            pitchDown = new KeyControl(KeyCode.UpArrow, () => AddForce(Vector3.right, true), () => AddForce(-Vector3.right, true));
            rollLeft = new KeyControl(KeyCode.Q, () => AddForce(Vector3.forward, true), () => AddForce(-Vector3.forward, true));
            rollRight = new KeyControl(KeyCode.E, () => AddForce(Vector3.back, true), () => AddForce(-Vector3.back, true));
            yawLeft = new KeyControl(KeyCode.LeftArrow, () => AddForce(Vector3.down, true), () => AddForce(-Vector3.down, true));
            yawRight = new KeyControl(KeyCode.RightArrow, () => AddForce(Vector3.up, true), () => AddForce(-Vector3.up, true));
            
            //Leftmouse fire      //keycode        //OnDown  //OnUp
            fire = new KeyControl(KeyCode.Mouse0, () => { }, () => { });
            engineToggle = new KeyControl(KeyCode.T, () => { }, () => { });
            turbo = new KeyControl(KeyCode.LeftShift, () => speed += 10, () => speed -= 10);
            esc = new KeyControl(KeyCode.Escape, () => { }, () => { escMenu.SetActive(!escMenu.activeInHierarchy); });

            boundKeys = new List<KeyControl> {
            forward,
            //back,
            //left,
            //right,
            //up,
            //down,
            pitchUp,
            pitchDown,
            rollLeft,
            rollRight,
            yawLeft,
            yawRight,
            //breaks,
            engineToggle,
            turbo,
            esc
            };
            PlayerManager.LocalPlayerInstance = gameObject;
            camera = Camera.main;
            escMenu = GameObject.FindGameObjectWithTag("EscMenu");

        }
        else
        {
            //disable collission, physics, etc
            playerBody.isKinematic = true;
        }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void LateUpdate()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            

            //recieve player input
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
        if (photonView.IsMine)
        {
            //leave gravity alone
            var originalVelocity = playerBody.velocity;
            var localForce = LocalDir(transform, desiredForce) * speed;
            var finalVelocity = Vector3.Lerp(originalVelocity, localForce, .1f);
            //add lift based on speed
            //var x = finalVelocity.x;
            //var y = finalVelocity.z;
            //var velocity2d = new Vector2(x, y);
            //var lift = velocity2d.magnitude * .25f;
            var finalY = finalVelocity.y - 1f;
            if (finalY < -70f)
            {
                finalY = -70f;
            }
            finalVelocity.y = finalY;
            playerBody.velocity = finalVelocity;
            playerBody.angularVelocity = Vector3.Lerp(playerBody.angularVelocity, LocalDir(transform, desiredTorque) * 3, .2f);

            //Debug.Log("Updating player's camera");
            //move camera to player
            //lerp towards target position
            var newPos = Vector3.Lerp(camera.transform.position, camPosOffset.position, .1f);
            //Debug.Log("before: " + camera.transform.position);
            camera.transform.position = newPos;
            //Debug.Log("after: " + camera.transform.position);
            //look at target
            camera.transform.LookAt(camLookOffset, LocalDir(transform, Vector3.up));
        }
        
    }
    public static Vector3 LocalDir(Transform transform, Vector3 worldDir)
    {
        return transform.rotation * worldDir;
    }
    void AddForce(Vector3 force, bool rotation = false)
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //we own this player, send other players our data
            //stream.SendNext();
        }
    }
}
