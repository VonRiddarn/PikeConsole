using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;

namespace FractalPike.PikeConsole.Examples;

#pragma warning disable IDE0044
#pragma warning disable IDE1006


/*
 * Absolute shitshow of an example at this time.
 * I'm just keeping it because it contains the magic source values for yaw and pitch tbh.
 * 
 * TODO: Cleanup this script and make it look at least a little bit like what you'd see in a real game.
 * 
 * NOTE: The way the pause is implemented is very sub-optimal and prone to breaking.  
 * That's because we're mixing concerns in order to keep the scrtipt simple.
*/

public partial class CharacterBody3d : CharacterBody3D
{

	[Export] CVarFloat _gravity;
	[Export] CVarFloat _speed;
	[Export] CVarFloat _jumpForce;
	[Export] float _runMultiplier = 1.33f;

	// m_yaw = Left / Right
	// m_pitch = Up / Down
	// Default: 0.022
	[Export] CVarFloat _mSensitivity;
	[Export] CVarFloat _mYaw;
	float _mYawCache = 0.022f;
	[Export] CVarFloat _mPitch;
	float _mPitchCache = 0.022f;

	[Export] Node3D _cameraNode { get; set; }

	float _cameraPitch = 0.0f;

	bool _isActive = true;

	public override void _EnterTree()
	{
		_mYawCache = Mathf.DegToRad(_mYaw.Value);
		_mPitchCache = Mathf.DegToRad(_mPitch.Value);

		_mYaw.ValueChanged += OnMYawChanged;
		_mPitch.ValueChanged += OnMPitchChanged;

		PikeConsoleStates.ConsoleUIActiveChanged += OnConsoleUIChanged;
	}
	public override void _ExitTree()
	{
		_mYaw.ValueChanged -= OnMYawChanged;
		_mPitch.ValueChanged -= OnMPitchChanged;

		PikeConsoleStates.ConsoleUIActiveChanged += OnConsoleUIChanged;
	}

	private void OnConsoleUIChanged(bool isActive) => _isActive = !isActive;

	void OnMYawChanged(float value) =>
		_mYawCache = Mathf.DegToRad(value);

	void OnMPitchChanged(float value) =>
		_mPitchCache = Mathf.DegToRad(value);


	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Input.UseAccumulatedInput = false;
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (_isActive && e is InputEventMouseMotion mouseMotion)
		{
			float sens = _mSensitivity.Value;
			float yawInput = mouseMotion.Relative.X * _mYawCache * sens;
			float pitchInput = mouseMotion.Relative.Y * _mPitchCache * sens;

			RotateY(-yawInput);

			if (_cameraNode != null)
			{
				_cameraPitch -= pitchInput;
				_cameraPitch = Mathf.Clamp(_cameraPitch, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));

				Vector3 camRotation = _cameraNode.Rotation;
				camRotation.X = _cameraPitch;
				_cameraNode.Rotation = camRotation;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		float speed = _speed.Value;

		if (Input.IsActionPressed("run"))
			speed *= _runMultiplier;

		if (!IsOnFloor())
		{
			Vector3 g = GetGravity();
			velocity += new Vector3(g.X, g.Y * _gravity.Value, g.Z) * (float)delta;
		}

		if (_isActive && Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = _jumpForce.Value;

		Vector2 inputDir = _isActive ? Input.GetVector("move_left", "move_right", "move_forward", "move_back") : Vector2.Zero;

		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}