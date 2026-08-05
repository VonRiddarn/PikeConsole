using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;
using System;

namespace FractalPike.PikeConsole.Examples;

#pragma warning disable IDE0044


/*
 * Absolute shitshow of an example at this time.
 * I'm just keeping it because it contains the magic source values for yaw and pitch tbh.
 * 
 * TODO: Cleanup this script and make it look at least a little bit like what you'd see in a real game.
*/

public partial class CharacterBody3d : CharacterBody3D
{

	[Export] CVarFloat PlayerGravity;
	[Export] CVarFloat PlayerSpeed;
	[Export] CVarFloat PlayerJumpForce;

	// m_yaw = Left / Right
	// m_pitch = Up / Down
	// Default: 0.022
	[Export] CVarFloat MSensitivity;
	[Export] CVarFloat MYaw;
	float _mYawCache = 0.022f;
	[Export] CVarFloat MPitch;
	float _mPitchCache = 0.022f;

	[Export] public Node3D CameraNode { get; set; }

	float _cameraPitch = 0.0f;

	bool _isActive = true;

	public override void _EnterTree()
	{
		_mYawCache = Mathf.DegToRad(MYaw.Value);
		_mPitchCache = Mathf.DegToRad(MPitch.Value);

		MYaw.ValueChanged += OnMYawChanged;
		MPitch.ValueChanged += OnMPitchChanged;

		PikeConsoleStates.ConsoleUIActiveChanged += OnConsoleUIChanged;
	}
	public override void _ExitTree()
	{
		MYaw.ValueChanged -= OnMYawChanged;
		MPitch.ValueChanged -= OnMPitchChanged;

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
			float sens = MSensitivity.Value;
			float yawInput = mouseMotion.Relative.X * _mYawCache * sens;
			float pitchInput = mouseMotion.Relative.Y * _mPitchCache * sens;

			RotateY(-yawInput);

			if (CameraNode != null)
			{
				_cameraPitch -= pitchInput;
				_cameraPitch = Mathf.Clamp(_cameraPitch, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));

				Vector3 camRotation = CameraNode.Rotation;
				camRotation.X = _cameraPitch;
				CameraNode.Rotation = camRotation;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		float speed = PlayerSpeed.Value;

		if (!IsOnFloor())
		{
			Vector3 g = GetGravity();
			velocity += new Vector3(g.X, g.Y * PlayerGravity.Value, g.Z) * (float)delta;
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = PlayerJumpForce.Value;

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