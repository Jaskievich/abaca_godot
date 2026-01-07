using Godot;
using System;
using System.Threading.Tasks; 

public partial class Main : Node2D, INetWorkCtrl
{
	private Hud _hud;
	private NetworkScene network = null;
	private InputDialog _inputDialog;
			
	public override void _Ready()
	{		
		_hud = GetNode<Hud>("HUD");		
		if(InitInputDialog())
		{
			// Используем CallDeferred чтобы убедиться, что все узлы полностью загружены
			CallDeferred(nameof(ShowInputDialog));
		}			
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
		_inputDialog.CancelPressed += OnCancelButtonPressed;	
		_inputDialog._OnCheckButton2Toggled += OnCheckButtonServerToggled;	
		_inputDialog._OnCheckButton3Toggled += OnCheckButtonClientToggled;		
		return true;
	}

	private void ShowInputDialog()
	{
		 _inputDialog.ShowDialog("Введите имена игроков");
	}
	
	public void ShowAcceptDialog(string title, string message)
	{
		var dialog = new AcceptDialog();
		dialog.Title = title;
		dialog.DialogText = message;	
		GetTree().Root.AddChild(dialog);
		dialog.PopupCentered();
	}
	
	private async void OnOpenNetServer(string firstText, string secondText)
	{
		GD.Print("OnOpenNetServer");
		GD.Print(firstText + " "+secondText);
		if(network== null){
			network = new NetworkScene();
			AddChild(network);
			_hud.SetNetWorkCtrl(this);	
			network.OnMessageReceived += HandleNetworkMessage;
			network.CreateServer();
			if( network.isServer ){
				NameCurrent = firstText;
				GetNode<Label>("ServerIp").Text = network.serverIP;
				GD.Print(NameCurrent);
				GetNode<Label>("NameNetGamer").Text = NameCurrent ;
			}
			else 
				GetNode<Label>("NameNetGamer").Text = "Сервер не создан" ;
		}	
	}
	
	private async void OnOpenNetClient(string firstText, string serverIP)
	{
		GD.Print("OnOpenNetClient");
		GD.Print(firstText + " "+serverIP);
		if(network== null){
			network = new NetworkScene();
			AddChild(network);
			_hud.SetNetWorkCtrl(this);	
			network.OnMessageReceived += HandleNetworkMessage;
			if( string.IsNullOrEmpty(serverIP) )
				network.ConnectAsClient();
			else
				network.ConnectAsClient(serverIP);
			if(  network.isClient ){
				NameCurrent = firstText;
				// послать сервреу имя клиента (второго игрока)
				GetNode<Label>("NameNetGamer").Text = NameCurrent ;
				await Task.Delay(3000);
				network.SendMessage($"nmc {firstText}");
			}
			else 
				GetNode<Label>("NameNetGamer").Text = "Клиент не создан" ;
		}
		
	}
	
	public async Task SendMessge(string msg){
		GD.Print("SendMessge " + msg);
		if( network!=null )
			network.SendMessage(msg);
	}
	
	private void HandleNetworkMessage(long senderId, string message)
	{
		GD.Print($"Main получил сообщение от {senderId}: {message}");
	//	ShowAcceptDialog("Alert", message);
		string [] arr = message.Split(' ');
		if (arr.Length > 2 && arr[0]=="mvg" && Enum.TryParse<Combination>(arr[2], out Combination sb))
		{
			_hud.SetSymbolClick(int.Parse(arr[1]), sb);			
		}
		else if(arr.Length > 1 && arr[0]=="nmc" )
		{
			ShowAcceptDialog("Внимание", "Произошло подключение");
			_hud.OnFirstButtonPressed(NameCurrent, arr[1] );
			// Отправить имя сервреа клиенту
			SendMessge($"nms {NameCurrent}");
		}
		else if(arr.Length > 1 && arr[0]=="nms" )
		{
			ShowAcceptDialog("Внимание", "Произошло подключение");
			_hud.OnFirstButtonPressed(arr[1], NameCurrent );
		}
		else
			ShowAcceptDialog("Ошибка", "Ошибка парсинга");
	}
	public string NameCurrent {get;private set;	}
	
	public void OnCheckButtonServerToggled(){
		GD.Print("main OnCheckButton2Toggled ");
		// сетевое подключение
			//_inputDialog.OkPressed -= _hud.OnFirstButtonPressed;
		_inputDialog.OkPressed += OnOpenNetServer;
	}
	
	public void OnCheckButtonClientToggled(){
		GD.Print("main OnCheckButton2Toggled ");
		// сетевое подключение		
		//_inputDialog.OkPressed -= _hud.OnFirstButtonPressed;
		_inputDialog.OkPressed += OnOpenNetClient;
	}
	
	public void OnCancelButtonPressed()
	{
		GD.Print("Ввод имен отменен");		
		// Инициализируем UI с именами по умолчанию
		 GetTree().Quit();
	}
}
