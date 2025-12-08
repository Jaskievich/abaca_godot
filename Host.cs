using Godot;
using System;
using System.Collections.Generic;
using Godot;

public partial class Server : Node
{
	private const int PORT = 9080;
	private TextEdit logText;

	public override void _Ready()
	{
		logText = GetNode<TextEdit>("UI/LogText");
		
		// Создаем сервер
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(PORT, 10);
		
		if (error == Error.Ok)
		{
			Multiplayer.MultiplayerPeer = peer;
			AddLog("✅ Сервер запущен на порту " + PORT);
			AddLog("Ожидаем сообщения от клиента...");
		}
		else
		{
			AddLog("❌ Ошибка создания сервера: " + error);
		}

		// Подписываемся на событие получения RPC
		Multiplayer.PeerConnected += OnPeerConnected;
	}

	private void OnPeerConnected(long id)
	{
		AddLog($"📞 Клиент подключился: {id}");
		
		// Отправляем приветственное сообщение новому клиенту
		RpcId(id, nameof(ClientReceiveMessage), "Добро пожаловать на сервер!");
	}

	// RPC метод для приема сообщений ОТ клиентов
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ServerReceiveMessage(string text)
	{
		long senderId = Multiplayer.GetRemoteSenderId();
		AddLog($"📨 Сообщение от {senderId}: {text}");
		
		// Отправляем ответ обратно клиенту
		RpcId(senderId, nameof(ClientReceiveMessage), $"Сервер получил: '{text}'");
	}

	// RPC метод для отправки сообщений клиентам
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ClientReceiveMessage(string text)
	{
		// Этот метод будет вызываться на клиентах
		// Сервер его только вызывает, но не реализует
	}

	private void AddLog(string message)
	{
		logText.Text += $"{Time.GetTimeStringFromSystem()}: {message}\n";
		
		// Прокручиваем вниз
		var scrollbar = logText.GetVScrollBar();
		scrollbar.Value = scrollbar.MaxValue;
	}
}
