using Godot;
using System;
using System.Threading.Tasks; 

public partial class Main : Node2D, INetWorkCtrl
{
	private Button openNetButton;
	private Hud _hud;
	
	private InputDialog _inputDialog;
		
	public override void _Ready()
	{		
		_hud = GetNode<Hud>("HUD");		
		openNetButton = GetNode<Button>("Button");
		openNetButton.Pressed += OnOpenNetPressed;	
		if(InitInputDialog())
		{
			// Используем CallDeferred чтобы убедиться, что все узлы полностью загружены
			CallDeferred(nameof(ShowInputDialog));
		}	
		_hud.SetNetWorkCtrl(this);	
	}
	private bool InitInputDialog()
	{
		_inputDialog = GD.Load<PackedScene>("res://InputDialog.tscn").Instantiate<InputDialog>();
		if(_inputDialog == null) 
		{
			GD.PrintErr("Не удалось загрузить InputDialog");
			return false;
		}				
		AddChild(_inputDialog);				
		// Подключаем сигналы
		_inputDialog.OkPressed += _hud.OnFirstButtonPressed;
		_inputDialog.CancelPressed += _hud.OnSecondButtonPressed;			
		return true;
	}

	private void ShowInputDialog()
	{
		 _inputDialog.ShowDialog("Введите имена игроков");
	}
	
	
	 private void OnOpenNetPressed()
	{
		GD.Print("OnOpenNetPressed");
		GetTree().ChangeSceneToFile("res://network_scene.tscn");
	}
	
	public async Task SendMessge(string msg){
		GD.Print(msg);
	}
}
