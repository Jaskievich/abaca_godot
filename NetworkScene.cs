using Godot;
using System;

public partial class NetworkScene : Node
{
	private const int PORT = 9080;
	private const string SERVER_IP = "127.0.0.1";
	
	private TextEdit logText;
	private LineEdit messageInput;
	private Button serverButton;
	private Button clientButton;
	private Button sendButton;
	private bool isServer = false;
	private bool isClient = false;
	
	public override void _Ready()
	{
		
		logText = GetNode<TextEdit>("UI/LogText");
		messageInput = GetNode<LineEdit>("UI/MessageInput");
		serverButton = GetNode<Button>("UI/ServerButton");
		clientButton = GetNode<Button>("UI/ClientButton");
		sendButton = GetNode<Button>("UI/SendButton");

		// Для теста: первый экземпляр - сервер, второй - клиент
		serverButton.Pressed += OnServerButtonPressed;
		clientButton.Pressed += OnClientButtonPressed;
		sendButton.Pressed += OnSendButtonPressed;
	
		AddLog("Нажмите 'Создать сервер' или 'Подключиться'");
	}

	private void OnServerButtonPressed()
	{	
		GD.Print("OnActionButtonPressed");
		GD.Print(isServer);
		GD.Print(Multiplayer.MultiplayerPeer);
		var status = Multiplayer.MultiplayerPeer.GetConnectionStatus();
		GD.Print(status);
		GD.Print("Сервер");
		CreateServer();
	}
	
	private void OnClientButtonPressed()
	{		
		GD.Print("OnActionButtonPressed");
		GD.Print(isServer);
		GD.Print(Multiplayer.MultiplayerPeer);
		var status = Multiplayer.MultiplayerPeer.GetConnectionStatus();
		GD.Print(status);
		GD.Print("Клиент");
		ConnectAsClient();
	}
	
	private void OnSendButtonPressed()
	{		
		GD.Print("OnActionButtonPressed");
		GD.Print(isServer);
		GD.Print(Multiplayer.MultiplayerPeer);
		var status = Multiplayer.MultiplayerPeer.GetConnectionStatus();
		GD.Print(status);
		GD.Print("Отправить сообщение");
		SendMessage();
	}

	private void CreateServer()
	{
		GD.Print("CreateServer");
		var peer = new ENetMultiplayerPeer();
		if (peer.CreateServer(PORT, 10) == Error.Ok)
		{
			Multiplayer.MultiplayerPeer = peer;
			isServer = true;
			//Button.Text = "Отправить";
			AddLog("✅ Сервер создан! Ожидаем сообщения...");
		}
	}

	private void ConnectAsClient()
	{
		var peer = new ENetMultiplayerPeer();
		if (peer.CreateClient(SERVER_IP, PORT) == Error.Ok)
		{
			Multiplayer.MultiplayerPeer = peer;
		//	actionButton.Text = "Отправить";
			AddLog("✅ Подключились к серверу!");
		}
	}

	private void SendMessage()
	{
		string message = messageInput.Text;
		if (!string.IsNullOrEmpty(message))
		{
			Rpc(nameof(ReceiveMessage), message);
			AddLog($"📤 Отправлено: {message}");
			messageInput.Text = "";
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void ReceiveMessage(string text)
	{
		long senderId = Multiplayer.GetRemoteSenderId();
		AddLog($"📨 Сообщение от {senderId}: {text}");
	}

	private void AddLog(string message)
	{
	
		logText.Text += $"{Time.GetTimeStringFromSystem()}: {message}\n";
	}
}
