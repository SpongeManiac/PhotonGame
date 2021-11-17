using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool team;

    public PhotonAnimatorView playerAnimator;
    
    public Rigidbody localPlayerBody
    {
        get
        {
            return GameManager.LocalPlayerInstance.GetComponentInChildren<Rigidbody>();
        }
    }

    public Transform camPosOffset;
    public Transform camLookOffset;

    public float speed = 40f;
    public float baseSpeed = 40f;

    public GameObject bullet;

    public float shootForce, upwardForce;
    public int magazineSize, bulletsPerTap;
    public float spread, timeBetweenShooting, timeBetweenShots, reloadTime;
    public bool allowButtonHold;

    public Transform attackPoint;

    public bool allowInvoke = true;

    public Text magazineText;
    public Text healthText;
    public int maxHealth = 100;
    public int health = 100;

    int bulletsLeft, bulletsShot;

    bool shooting, readyToShoot, reloading;


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

            fire = new KeyControl(KeyCode.Space, () => shooting = true, () => shooting = false);
            engineToggle = new KeyControl(KeyCode.T, () => { }, () => { });
            turbo = new KeyControl(KeyCode.LeftShift, () => speed += 10, () => speed -= 10);
            esc = new KeyControl(KeyCode.Escape, () => { }, () => { GameManager.escMenu.SetActive(!GameManager.escMenu.activeInHierarchy);});

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
            esc,
            fire
            };
            GameManager.LocalPlayerInstance = gameObject;
            bulletsLeft = magazineSize;
            readyToShoot = true;
            magazineText = GameObject.FindGameObjectWithTag("Magazine").GetComponent<Text>();
            healthText = GameObject.FindGameObjectWithTag("Health").GetComponent<Text>();
            magazineText.text = "100/100";
            healthText.text = "100/100";

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

            //check if player is shooting
            if (shooting)
            {
                //Reload automatically when trying to shoot without ammo
                if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

                //Shooting
                if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
                {
                    bulletsShot = 0;
                    Shoot();
                }
            }

            magazineText.text = $"{bulletsLeft}/{magazineSize}";


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
            var newPos = Vector3.Lerp(Camera.main.transform.position, camPosOffset.position, .1f);
            //Debug.Log("before: " + camera.transform.position);
            Camera.main.transform.position = newPos;
            //Debug.Log("after: " + camera.transform.position);
            //look at target
            Camera.main.transform.LookAt(camLookOffset, LocalDir(transform, Vector3.up));
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
        else
        {
            
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        //Find the exact hit position using a raycast
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        //Check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point - new Vector3(0f,5f,0);
        else
            targetPoint = ray.GetPoint(75);

        //Calculate direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        //Calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Calculate new direction with spread;
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        //Instantiate bullet/projectile
        GameObject currentBullet = PhotonNetwork.Instantiate(bullet.name, attackPoint.position, transform.rotation);
        currentBullet.transform.forward = directionWithSpread.normalized;

        //Add force to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(Camera.main.transform.up * upwardForce, ForceMode.Impulse);

        bulletsLeft--;
        bulletsShot++;

        //Invoke resetShot function (if not already invoked)
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        //if more than one bulletsPerTap
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
        {
            if (collision.gameObject.tag == "bullet")
            {
                health -= 10;
                healthText.text = $"{health}/100";
                Debug.Log(health);
            }
        }
    }
}
