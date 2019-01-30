using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperScript : Photon.PunBehaviour, IPunObservable
{

    #region PUBLIC PROPERTIES
    
    //public float DirectionDampTime = .25f;
    public float maxSpeed = .5f;
    [Tooltip("How long it'd take to go from 0 to maxSpeed using a keyboard, in seconds.")]
    public float accelerateTime = 0.1f;
    public GameObject _head;
    public GameObject _jumperPlate;
    public GameObject _groundDetecter;
    public float jumpStrength = 7f;
    public float gravityMax = 10;
    public float gravityAccel = 20;
    public LayerMask groundLayer;

    #endregion

    #region PRIVATE PROPERTIES
    //private Animator animator;
    //private CharacterController _controller;
    private Rigidbody _body;
    //private Collider _groundDetecter;

    private Vector3 desiredMove;
    private Vector3 move;

    private Quaternion yRotation = Quaternion.identity;
    //private float yRotation = 0;
    private Quaternion jumperPlateStartRotation;
    private bool _isGrounded = false;

    /*
    private Vector3 lastPosition, velocity;
    private Quaternion lastRotation;
    private float turnSpeed;
    float lastTime = 0;
    */
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 oldTargetPosition;
    private Quaternion oldTargetRotation;
    private Vector3 velocity;
    private Vector3 netSyncVelocity;
    private float netTickDelta;
    private float netTickLast;
    private float oldSenderTime;
    private float senderTime;

    Camera _jumperCamera;
    #endregion


    #region MONOBEHAVIOUR CALLBACKS

    // Start is called before the first frame update
    void Start()
    {
        //_controller = GetComponent<CharacterController>();
        //if (!_controller)
        //{ Debug.LogError("Controller not found"); }
        _body = GetComponent<Rigidbody>();
        //_groundDetecter = GetComponent<SphereCollider>();
        jumperPlateStartRotation = _jumperPlate.transform.localRotation;

        _jumperCamera = GetComponentInChildren<Camera>();

        if (PhotonNetwork.connected == true && photonView.isMine == false) {
            _jumperCamera.enabled = false;
        }

        //lastPosition = transform.position;
        //lastRotation = transform.rotation;

        oldTargetPosition = targetPosition = transform.position;
        oldTargetRotation = targetRotation = transform.rotation;
        velocity = new Vector3(0, 0, 0);
        netSyncVelocity = new Vector3(0, 0, 0);
        netTickDelta = 0.1f;
}

    // Update is called once per frame
    void Update()
    {
        // Below code happens on all players machines 

        SphereCollider groundCollder = _groundDetecter.GetComponent<SphereCollider>();
        if (Physics.CheckSphere(groundCollder.transform.position + groundCollder.center, groundCollder.radius, groundLayer))
        {
            _isGrounded = true;
            //_jumperPlate.transform.rotation = jumperPlateDefault.rotation;
        }
        else
        {
            _isGrounded = false;
        }


        if (!_isGrounded) {
            _jumperPlate.transform.Rotate(new Vector3(0, 70, 0));
        } else
        {
            _jumperPlate.transform.localRotation = jumperPlateStartRotation;
            //Debug.Log(jumperPlateDefault.rotation);
        }

        //_head.transform.rotation

        //Debug.Log(_head.transform.rotation.eulerAngles);
        //Debug.Log(_head.transform.rotation);


        /////
        // Below stuff only happens on THIS client's computer, not other players!
        //if (photonView.isMine == false && PhotonNetwork.connected == true) { return; }
        if (photonView.isMine == true || PhotonNetwork.connected == false)
        {

            if (Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; }
            if (Cursor.visible) { Cursor.visible = false; }

            _head.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));


            desiredMove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) / 3;
            desiredMove = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * (desiredMove * maxSpeed);

            move = new Vector3();


            if ((desiredMove.x + desiredMove.z) % 1 != 0)
            {
                // input is from joystick, don't smooth it
                move = desiredMove;
            }
            else
            {
                // input is from keyboard (or a joystick at 100%), DO smooth it
                move = desiredMove;
                //Debug.Log(move);
            }

            //_groundDetecter.

            if (Input.GetButtonDown("Jump"))
            {
                //_body.AddForce(Vector3.up * jumpStrength, ForceMode.VelocityChange);
                //_body.velocity.Set(0, 10, 0);
                _body.velocity = new Vector3(0, jumpStrength, 0);
            }

            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
            //_head.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));

            //PhotonNetwork.ServerTimestamp
            //Debug.Log(Time.fixedTime);
        } else if (photonView.isMine == false && PhotonNetwork.connected == true)
        {
            //if ((Time.time - netTickLast) < netTickTime)
            //{
            //velocity += netSyncVelocity * netTickTime;
            //transform.position += velocity;
            //transform.position = oldTargetPosition + netSyncVelocity * ((Time.time - netTickLast) / netTickTime);
            //transform.position = oldTargetPosition + netSyncVelocity * Time.deltaTime;
            transform.position += netSyncVelocity * Time.deltaTime;
            //} else
            //{
            //    transform.position = targetPosition;
            //    transform.rotation = targetRotation;
            //}
        }
    }

    private void FixedUpdate()
    {
        if (photonView.isMine == false && PhotonNetwork.connected == true) { return; }

        _body.MovePosition(_body.position + move);

        // Gravity
        if (_body.velocity.y > -gravityMax)
        {
            _body.AddForce(-Vector3.up * gravityAccel, ForceMode.Acceleration);
        }

        /*
        if (photonView.isMine == true && PhotonNetwork.connected == true)
        {
            // Note: This is inconsistently jerky, but seems to handle fast changes better
            // I think it can work if I can just get the god damn network update ticks, to time it better...!
            velocity = (transform.position - lastPosition) / Time.fixedDeltaTime; //Time.deltaTime;
            turnSpeed = Quaternion.Angle(transform.rotation, lastRotation) / Time.fixedDeltaTime; //Time.deltaTime;
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            // TODO: Save reference beforehand
            // https://doc-api.photonengine.com/en/pun/current/class_photon_transform_view.html#a914782b6d7ec46386636fa9fbaaa8f1f
            gameObject.GetComponent<PhotonTransformView>().SetSynchronizedValues(velocity, turnSpeed);
            //Debug.Log("lastPosition " + velocity + " lastRotation " + turnSpeed);
        }
        */
    }

    //on

    /*
private void OnTriggerEnter(Collider other)
{
        //Debug.Log(other.tag);
}
private void OnCollisionEnter(Collision collision)
{
    Debug.Log(collision.contactCount);
    Debug.Log(collision.contacts[0].thisCollider);
}
*/

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            netTickDelta = Time.time - netTickLast;
            netTickLast = Time.time;

            //velocity = (transform.position - oldTargetPosition) / (Time.time - netTickLast);
            //oldTargetPosition = transform.position;
            //netTickLast = Time.time;

            velocity = (transform.position - oldTargetPosition) / netTickDelta;
            oldTargetPosition = transform.position;

            // We own this player: send the others our data
            //stream.SendNext(IsFiring);
            //stream.SendNext(Health);
            //stream.SendNext(_head.transform.rotation.eulerAngles);
            stream.SendNext(_head.transform.rotation);
            //stream.SendNext(Time.fixedTime);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(velocity);
            stream.SendNext(Time.time);
            //Debug.Log("Wrote net stream! Velocity " + velocity);
        }
        else
        {
            netTickDelta = Time.time - netTickLast;
            netTickLast = Time.time;
            oldSenderTime = senderTime;

            //transform.position = targetPosition;    // update to old value first
            //oldTargetPosition = targetPosition;
            //transform.rotation = targetRotation;
            //oldTargetRotation = targetRotation;
            //oldTargetPosition = transform.position;

            Vector3 positionError = targetPosition - transform.position;


            // Network player, receive data
            //this.IsFiring = (bool)stream.ReceiveNext();
            //this.Health = (float)stream.ReceiveNext();
            //Debug.Log("this.IsFiring " + this.IsFiring + " this.Health " + this.Health);
            //_head.transform.rotation.eulerAngles = (Vector3)stream.ReceiveNext();
            _head.transform.rotation = (Quaternion)stream.ReceiveNext();
            //float lastTimeNew = (float)stream.ReceiveNext();
            //if (lastTimeNew != lastTime)
            //{
            // network data received
            //lastTimeNew = lastTime;

            //}
            //transform.position = (Vector3)stream.ReceiveNext();
            //transform.rotation = (Quaternion)stream.ReceiveNext();
            targetPosition = (Vector3)stream.ReceiveNext();
            targetRotation = (Quaternion)stream.ReceiveNext();
            netSyncVelocity = (Vector3)stream.ReceiveNext();
            senderTime = (float)stream.ReceiveNext();
            //Debug.Log("Tick! " + targetPosition);

            netSyncVelocity += positionError / 0.1f;

            //float netUpdateDelay

            //netTickTime = 0.1f;
            //netSyncVelocity = (targetPosition - (transform.position + (velocity * netTickTime)));
            //netSyncVelocity = (targetPosition - (transform.position )) / netTickTime;
            //velocity = new Vector3(0, 0, 0);

            Debug.Log("Tick! targetPosition " + targetPosition + " current postion is " + transform.position + " netSyncVelocityDelta " + netSyncVelocity);
            //Debug.Log("update time " + (senderTime - oldSenderTime));
            Debug.Log("senderTime - oldSenderTime " + (senderTime - oldSenderTime) + " netTickDelta " + netTickDelta);
        }

        //Debug.Log("Network data received.");
    }

    #endregion

}

