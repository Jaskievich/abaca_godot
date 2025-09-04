using Godot;
using System;

public partial class Dice : RigidBody2D
{
	[Export] private Texture2D[] _diceTextures;
	[Export] private int _currentIndex ;	
	[Export] private float _highlightScale = 1.2f;
	[Export] private Color _highlightColor = new Color(1.2f, 1.2f, 1.2f);
	
	private int _currentValue = 1;	
	private Sprite2D _sprite;
	private Vector2 _originalScale;
	private Color _originalColor;
	private bool isEnabled = true;
	
		
	[Signal]
	public delegate void _SetDiceEnabledEventHandler(int index, bool newVal);
	
	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("DiceSprite");
		UpdateTexture();	
		_originalScale = _sprite.Scale;
		_originalColor = _sprite.Modulate;
		
		// Подключаем сигналы мыши
		var _clickArea =  GetNode<Area2D>("Area2D");
		_clickArea.MouseEntered += OnMouseEntered;
		_clickArea.MouseExited += OnMouseExited;
		_clickArea.InputEvent += OnInputEvent;
	}
	
	public void SetValue(int value)
	{
		_currentValue = Mathf.Clamp(value, 1, 6);
		UpdateTexture();
	}
	
	public int GetValue()
	{
		return _currentValue;
	}
	
	public bool IsEnabled
	{
		set{
			isEnabled = value;
		}
		get{
			return isEnabled ;
		}
	}
	
	private void UpdateTexture()
	{
		if (_diceTextures != null && _diceTextures.Length >= 6 && _sprite != null)
		{
			_sprite.Texture = _diceTextures[_currentValue - 1];
		}
	}
	
	 private void OnMouseEntered()
	{
		if( isEnabled )
			Highlight(true);
	}
	
	private void OnMouseExited()
	{
		if( isEnabled )
			Highlight(false);
	}
	
	private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event.IsActionPressed("mouse_left"))
		{
			OnDiceClicked();
		}
	}
	
	
	
	public void Highlight(bool enable)
	{
		var tween = CreateTween();		
		if (enable)
		{
			tween.TweenProperty(_sprite, "scale", _originalScale * _highlightScale, 0.1f);
			tween.Parallel().TweenProperty(_sprite, "modulate", _highlightColor, 0.1f);
		}
		else
		{
			tween.TweenProperty(_sprite, "scale", _originalScale, 0.1f);
			tween.Parallel().TweenProperty(_sprite, "modulate", _originalColor, 0.1f);
		}
	}
	
	private void OnDiceClicked()
	{	
		isEnabled = !isEnabled;
		SetDiceEnabled(isEnabled);
	}
	
	public void SetDiceEnabledView( bool enabled)
	{
		// Основное свойство
		Sleeping = !enabled;
		Freeze = !enabled;	
		// Визуальные эффекты
		var sprite = GetNode<Sprite2D>("DiceSprite");
		if (enabled)
		{
			sprite.Modulate = Colors.White;
		}
		else
		{
			sprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.6f);
		}
	}
	
	public void SetDiceEnabled( bool enabled)
	{
		SetDiceEnabledView(enabled);
		EmitSignal(SignalName._SetDiceEnabled, _currentIndex, enabled );
	}
	
}
