using System;
using System.ComponentModel;
using DeliveryRush;
using UnityEngine;

public class Courier : MonoBehaviour
{
    public CourierState State { get; private set; }
    public bool IsOnTransport { get; private set; }
    public GameObjectType CurrentTransportType { get; private set; }

    private float _parcelPreservationStatus;
    public float ParcelPreservationStatus
    {
        get => _parcelPreservationStatus;
        private set
        {
            if (value > 0 && value <= 1)
                _parcelPreservationStatus = value;
            else
            {
                _parcelPreservationStatus = 0;
                deathMenuManager.Activate();
            }
        }
    }
    
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private Animator _animator;

    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    // [SerializeField] private float defaultSlidingTime;
    [SerializeField] private float slidingMultiplier; // ужасное название, надо как-то переименовать

    private float _transportSpeed;
    private float _timeToDriveLeft;
    // private float _timeToSlideLeft;
    
    [SerializeField] private LayerMask groundLayer;
    
    [SerializeField] public UIManager uiManager;
    [SerializeField] public MenuManager deathMenuManager;
    [SerializeField] public EndMenuManager endMenuManager;

    private static readonly Vector3 DefaultSpawnPosition = new Vector3(7.3f, 1.5f, 0);
    
    public int Money
    {
        get => PlayerPrefs.GetInt(EndMenuManager.MoneyBank);
        set
        {
            PlayerPrefs.SetInt(EndMenuManager.MoneyBank, value);
            uiManager.UpdateMoney(Money);
        }
    }

    private void Start()
    {
        uiManager.UpdateMoney(Money);
        
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        
        State = CourierState.Idle;
        SwitchToTransport(false);
        
        transform.position = DefaultSpawnPosition;
        transform.localScale = new Vector2(1, 1);
        _rigidBody.velocity = Vector2.zero; // TODO 

        // print(_rigidBody.velocity);

        speed = 10;
        jumpForce = 10;
        // defaultSlidingTime = 1f;
        ParcelPreservationStatus = 1;
        slidingMultiplier = 1.6f;
        
        _transportSpeed = 12;
        _timeToDriveLeft = Transport.DefaultRideTime;
        // _timeToSlideLeft = defaultSlidingTime;
        
        uiManager.UpdateStatusBar();
    }

    public void Reset() => Start();
    
    // TODO https://www.youtube.com/watch?v=jfFOD9TRKeQ&list=PLpj8TZGNIBNy51EtRuyix-NYGmcfkNAuH&index=25&t=1s

    private void Update()
    {
        if (!IsOnTransport)
        {
            var hDirection = Input.GetAxis("Horizontal");
            var directionDx = hDirection.CompareTo(0);
        
            if (Math.Abs(hDirection) > 0.05)
            {
                _rigidBody.velocity = State != CourierState.Sliding 
                    ? new Vector2(directionDx * speed, _rigidBody.velocity.y) 
                    : new Vector2(directionDx * speed * slidingMultiplier, _rigidBody.velocity.y);
                transform.localScale = new Vector2(directionDx, 1);
            }
            else _rigidBody.velocity = new Vector2(0, _rigidBody.velocity.y); // для того, чтобы не было скольжения

            if (Input.GetButtonDown("Jump") && _collider.IsTouchingLayers(groundLayer))
            {
                _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, jumpForce);
                State = CourierState.Jumping;
            }
        }
        else
        {
            var hDirection = Input.GetAxis("Horizontal");
            var directionDx = hDirection.CompareTo(0);

            if (hDirection != 0)
            {
                _rigidBody.velocity 
                    = new Vector2(directionDx * _transportSpeed * slidingMultiplier, _rigidBody.velocity.y);
                transform.localScale = new Vector2(directionDx, 1);
            }
        }

        UpdateTransport();
        UpdateState();
    }

    private void UpdateTransport()
    {
        if (!IsOnTransport) return;

        if (_timeToDriveLeft <= 0)
        {
            Instantiate(MapBuilder.GetGameObject(CurrentTransportType),
                transform.position, Quaternion.identity, null);
            SwitchToTransport(false);
            _timeToDriveLeft = Transport.DefaultRideTime;
        }
        
        _timeToDriveLeft -= Time.deltaTime;
    }

    private static readonly int EditorStateHash = Animator.StringToHash("State");
    private static readonly int OnTransport = Animator.StringToHash("IsOnTransport");
    private static readonly int TransportType = Animator.StringToHash("CurrentTransportType");

    private void UpdateState()
    {
        // print(_timeToSlideLeft);
        
        switch (State)
        {
            case CourierState.Sliding:
                
                /*if (_timeToSlideLeft <= 0)
                {
                    State = CourierState.Running;
                    _timeToSlideLeft = defaultSlidingTime;
                }
                else
                {
                    _timeToSlideLeft -= Time.deltaTime;
                }*/
                
                if (!Input.GetKey("left shift"))
                {
                    State = CourierState.Running;
                    // _timeToSlideLeft = defaultSlidingTime;
                }
                break;
            
            case CourierState.Jumping:
                if (_rigidBody.velocity.y < 0)
                    State = CourierState.Falling;
                break;
            
            case CourierState.Falling:
                if (_collider.IsTouchingLayers(groundLayer))
                {
                    State = CourierState.Idle;
                    if (Input.GetKey("left shift") && Mathf.Abs(_rigidBody.velocity.x) > 0)
                        State = CourierState.Sliding;
                }
                break;
            
            default:
                State = Mathf.Abs(_rigidBody.velocity.x) > 0.05 && State != CourierState.Sliding
                    ? CourierState.Running : CourierState.Idle;
                
                if (Input.GetKey("left shift") && State != CourierState.Idle
                // && _timeToSlideLeft > 0
                )
                    State = CourierState.Sliding;
                break;
        }
        
        // print("State: " + State + "; IsOnTransport: " + IsOnTransport);
        _animator.SetInteger(EditorStateHash, (int) State);
    }

    public void DamageParcel(float damage)
    {
        ParcelPreservationStatus -= damage;
        uiManager.UpdateStatusBar();
    }

    public void SwitchToTransport(bool isOnTransport, GameObjectType transportType = GameObjectType.Courier)
    {
        IsOnTransport = isOnTransport;
        _animator.SetBool(OnTransport, isOnTransport);
        _animator.SetInteger(TransportType, (int) transportType);
        CurrentTransportType = transportType;

        if (IsOnTransport && State != CourierState.Idle) State = CourierState.Running;
        
        Obstacle.ObstaclesAreCollidable = !isOnTransport;
    }
}

public enum CourierState
{
    Idle = 0,
    Running = 1,
    Jumping = 2,
    Falling = 3,
    Sliding = 4,
    // Damaged = 6
}