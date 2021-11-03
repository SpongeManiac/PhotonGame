using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject escMenu;
    public PhotonAnimatorView playerAnimator;

    public static GameObject LocalPlayerInstance;
    public Rigidbody localPlayerBody
    {
        get
        {
            return LocalPlayerInstance.GetComponentInChildren<Rigidbody>();
        }
    }
    public Camera camera;

    public Vector3 camPosOffset;
    public Vector3 camLookOffset;

    public float speed = 20f;
    public float baseSpeed = 20f;

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
    KeyControl breaks;
    KeyControl engineToggle;
    KeyControl turbo;
    KeyControl esc;

    List<KeyControl> boundKeys = new List<KeyControl>();
    List<KeyControl> pressedKeys = new List<KeyControl>();

    public Rigidbody playerBody;

    public GameObject player;

    private Vector2 turn;
    private float sensitivity = 2f;

    private void Awake()
    {
        forward = new KeyControl(KeyCode.W, () => AddForce(Vector3.forward), () => AddForce(-Vector3.forward));
        back = new KeyControl(KeyCode.S, () => AddForce(Vector3.back), () => AddForce(-Vector3.back));
        left = new KeyControl(KeyCode.A, () => AddForce(Vector3.left), () => AddForce(-Vector3.left));
        right = new KeyControl(KeyCode.D, () => AddForce(Vector3.right), () => AddForce(-Vector3.right));
        //down = new KeyControl(KeyCode.LeftControl, () => AddForce(Vector3.down), () => AddForce(-Vector3.down));
        //up = new KeyControl(KeyCode.Space, () => AddForce(Vector3.up), () => AddForce(-Vector3.up));
        pitchUp = new KeyControl(KeyCode.DownArrow, () => AddForce(Vector3.left, true), () => AddForce(-Vector3.left, true));
        pitchDown = new KeyControl(KeyCode.UpArrow, () => AddForce(Vector3.right, true), () => AddForce(-Vector3.right, true));
        rollLeft = new KeyControl(KeyCode.Q, () => AddForce(Vector3.forward, true), () => AddForce(-Vector3.forward, true));
        rollRight = new KeyControl(KeyCode.E, () => AddForce(Vector3.back, true), () => AddForce(-Vector3.back, true));
        yawLeft = new KeyControl(KeyCode.LeftArrow, () => AddForce(Vector3.down, true), () => AddForce(-Vector3.down, true));
        yawRight = new KeyControl(KeyCode.RightArrow, () => AddForce(Vector3.up, true), () => AddForce(-Vector3.up, true));
        //breaks = new KeyControl(KeyCode.R, () => Breaks(), () => { });
        engineToggle = new KeyControl(KeyCode.T, () => { }, () => { });
        turbo = new KeyControl(KeyCode.LeftShift, () => speed += 10, () => speed -= 10);
        esc = new KeyControl(KeyCode.Escape, () => { }, () => { escMenu.SetActive(!escMenu.activeInHierarchy);});

        boundKeys = new List<KeyControl> {
            forward,
            back,
            left,
            right,
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

        if (photonView.IsMine)
        {
            PlayerManager.LocalPlayerInstance = gameObject;
            camera = Camera.main;
            escMenu = GameObject.FindGameObjectWithTag("EscMenu");
        }

        DontDestroyOnLoad(gameObject);

        Cursor.visible = false;
    }

    

    // Start is called before the first frame update
    void Start()
    {
        //move camera to player
        //lerp towards target position

    }

    void LateUpdate()
    {
        if (photonView.IsMine)
        {
            
        }
        
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

        //Player rotate with mouse
        //Move this to PhotonView.IsMine condition
        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        player.transform.localRotation = Quaternion.Euler(-turn.y, turn.x, 0);
        //camera.transform.localRotation = Quaternion.Euler(-turn.y, turn.x, 0);
    }


    private Vector3 velocity = Vector3.zero;

    private void FixedUpdate()
    {

        //apply forces to rigidbody
        playerBody.velocity = Vector3.Lerp(playerBody.velocity, LocalDir(transform, desiredForce) * speed, .2f);
        //playerBody.angularVelocity = Vector3.Lerp(playerBody.angularVelocity, LocalDir(transform, desiredTorque) * 3, .2f);

        float desiredAngle = player.transform.eulerAngles.y;
        float desiredAngleX = player.transform.eulerAngles.x;
        Quaternion rotation = Quaternion.Euler(desiredAngleX, desiredAngle, 0);

        var newPos = Vector3.Lerp(camera.transform.position, localPlayerBody.transform.position + camPosOffset, .1f);
        camera.transform.position = newPos + (rotation * camPosOffset);


        //look at target
        camera.transform.LookAt(localPlayerBody.position + camLookOffset, Vector3.up);
        camera.transform.rotation = player.transform.rotation;
        
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
