using UnityEngine.InputSystem;
using UnityEngine;
using StarterAssets;
using Unity.Netcode;
using static Unity.Collections.AllocatorManager;

public class PlayerController : NetworkBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 4.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 6.0f;
	[Tooltip("Rotation speed of the character")]
	public float RotationSpeed = 1.0f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;

	public Transform ArmPos;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 90.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -90.0f;

	// cinemachine
	private float _cinemachineTargetPitch;

	// player
	private float _speed;
	private float _rotationVelocity;
	private float _verticalVelocity;

	private PlayerInput _playerInput;
	private CharacterController _controller;
	private StarterAssetsInputs _input;
	private GameObject _mainCamera;

	public float grabDistance = 2f;
	private bool _IsGrabing = false;

	private const float _threshold = 0.01f;

	public Block HoldBlock;

	private bool IsCurrentDeviceMouse
	{
		get
		{
		 return _playerInput.currentControlScheme == "KeyboardMouse";
		}
	}

	private void Awake()
	{
		// get a reference to our main camera
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}

	private void Start()
	{
		_controller = GetComponent<CharacterController>();
		_input = GetComponent<StarterAssetsInputs>();
		_playerInput = GetComponent<PlayerInput>();

	}

	private void Update()
	{
		if(!IsOwner)
		{
			return;
		}

		Move();
		Grab();
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void CameraRotation()
	{
		// if there is an input
		if (_input.look.sqrMagnitude >= _threshold)
		{
			//Don't multiply mouse input by Time.deltaTime
			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

			_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
			_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

			// clamp our pitch rotation
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Update Cinemachine camera target pitch
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

			// rotate the player left and right
			transform.Rotate(Vector3.up * _rotationVelocity);
		}
	}

	private void Move()
	{
		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed = MoveSpeed;

		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is no input, set the target speed to 0
		if (_input.move == Vector2.zero) targetSpeed = 0.0f;

		// a reference to the players current horizontal velocity
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}

		// normalise input direction
		Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

		// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is a move input rotate player when the player is moving
		if (_input.move != Vector2.zero)
		{
			// move
			inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
		}

		// move the player
		_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
	}

	private void Grab()
	{
		if (!_input.grab)
		{
			return;
		}

		if (!_IsGrabing)
		{
			Ray ray = _mainCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());

			if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
			{
				if (hit.collider.TryGetComponent<Block>(out Block block))
				{
					HoldBlock = block;

					SetParentServerRpc(HoldBlock.gameObject, ArmPos.parent.gameObject);
					//SetParentServerRpc(block.transform, ArmPos);
					//block.transform.parent = ArmPos;
					//HoldBlock.transform.position = Vector3.zero;
					_IsGrabing = true;
				}
			}

		}
		else
		{
			DropBlockServerRpc(HoldBlock.gameObject);
			HoldBlock = null;
			_IsGrabing = false;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetParentServerRpc(NetworkObjectReference block, NetworkObjectReference armPos)
	{
		/*if(!IsOwner)
		{
			return;
		}*/

		block.TryGet(out NetworkObject networkObjectBlock);
		armPos.TryGet(out NetworkObject networkObjectArmPos);

		networkObjectBlock.gameObject.transform.SetParent(networkObjectArmPos.gameObject.transform);
	}

	[ServerRpc(RequireOwnership = false)]
	private void DropBlockServerRpc(NetworkObjectReference block)
	{
		/*if (!IsOwner)
		{
			return;
		}*/

		block.TryGet(out NetworkObject networkObjectBlock);

		networkObjectBlock.transform.SetParent(null);

	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

}
