using Godot;
using System.Collections.Generic;
//
//public partial class Client : Node
//{
	//private const string SERVER_IP = "127.0.0.1";
	//private const int PORT = 9080;
	//
	//private LineEdit messageInput;
	//private Button sendButton;
	//private Button connectButton;
	//private TextEdit logText;
//
	//public override void _Ready()
	//{
		//// Получаем UI элементы
		//messageInput = GetNode<LineEdit>("UI/MessageInput");
		//sendButton = GetNode<Button>("UI/SendButton");
		//connectButton = GetNode<Button>("UI/ConnectButton");
		//logText = GetNode<TextEdit>("UI/LogText");
//
		//// Подписываемся на события
		//sendButton.Pressed += OnSendButtonPressed;
		//connectButton.Pressed += OnConnectButtonPressed;
		//
		//sendButton.Disabled = true;
		//AddLog("Нажмите 'Подключиться' для соединения с сервером");
	//}
//
	//private void OnConnectButtonPressed()
	//{
		//ConnectToServer();
	//}
//
	//private void ConnectToServer()
	//{
		//ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		//var error = peer.CreateClient(SERVER_IP, PORT);
		//
		//if (error == Error.Ok)
		//{
			//Multiplayer.MultiplayerPeer = peer;
			//AddLog("✅ Подключились к серверу!");
			//sendButton.Disabled = false;
			//connectButton.Disabled = true;
		//}
		//else
		//{
			//AddLog("❌ Ошибка подключения: " + error);
		//}
	//}
//
	//private void OnSendButtonPressed()
	//{
		//string message = messageInput.Text.Trim();
		//if (string.IsNullOrEmpty(message))
			//return;
//
		//// Отправляем сообщение на сервер (ID 1 - это сервер)
		//RpcId(1, nameof(ServerReceiveMessage), message);
		//AddLog($"📤 Отправлено: {message}");
		//
		//messageInput.Text = ""; // Очищаем поле ввода
	//}
//
	//// RPC метод для приема сообщений ОТ сервера
	//[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	//private void ClientReceiveMessage(string text)
	//{
		//AddLog($"📨 Сервер сказал: {text}");
	//}
//
	//// RPC метод для отправки сообщений НА сервер (должен быть на сервере)
	//[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	//private void ServerReceiveMessage(string text)
	//{
		//// Этот метод будет вызываться на сервере
		//// Клиент его только вызывает, но не реализует
	//}
//
	//public override void _Input(InputEvent @event)
	//{
		//// Отправка по Enter
		//if (@event.IsActionPressed("ui_accept") && messageInput.HasFocus())
		//{
			//OnSendButtonPressed();
		//}
	//}
//
	//private void AddLog(string message)
	//{
		//logText.Text += $"{Time.GetTimeStringFromSystem()}: {message}\n";
		//
		//// Прокручиваем вниз
		//var scrollbar = logText.GetVScrollBar();
		//scrollbar.Value = scrollbar.MaxValue;
	//}
//}
