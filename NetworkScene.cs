using Godot;
using System;
using System.Threading.Tasks; 

public partial class NetworkScene : Node
{
	private const int PORT = 9080;
	//private const string SERVER_IP = "192.168.100.5";
	private const string SERVER_IP = "127.0.0.1";
	public bool isServer = false, isClient = false;
	public String serverIP;
	private ENetMultiplayerPeer peer;
	
	// Добавляем флаг для отслеживания текущего состояния
	private bool isActive = false;
	
	public void ShowAcceptDialog(string title, string message)
	{
		var dialog = new AcceptDialog();
		dialog.Title = title;
		dialog.DialogText = message;		
		GetTree().Root.AddChild(dialog);
		dialog.PopupCentered();
	}
	
	
	public void CreateServerClient()
	{
		//// Проверяем, есть ли уже активное подключение
		if (isActive)
		{
			GD.Print("⚠️ Уже есть активное подключение!");
			ShowAcceptDialog("Внимание", 
				isServer ? "Сервер уже запущен" : "Клиент уже подключен");
			return;
		}
		GD.Print("Сервер не найден, создаем новый...");
		isServer = CreateServer();
		isActive = isServer;
		ShowAcceptDialog("Alert", 
			isServer ? "✅ Сервер создан" : "❌ Не удалось создать сервер");
		if( isServer == false ){
		// Сервер найден - подключаемся как клиент
			GD.Print("Сервер найден, подключаемся как клиент...");
			isClient = ConnectAsClient();
			isActive = isClient;
			ShowAcceptDialog("Alert", 
				isClient ? "✅ Подключились к серверу" : "❌ Не удалось подключиться");
		}
		
	}
	//
	private void FillIpServer()
	{
		var addresses = IP.GetLocalAddresses();
		foreach (var ip in addresses)
		{
			// Пропускаем локальные адреса (loopback)
			if (!ip.StartsWith("127.") && !ip.StartsWith("::1"))
			{
				GD.Print($"✅ Сервер создан на {ip}:{PORT}");
				// или можно сохранить в переменную класса
				serverIP = ip;
			}
		}
		// Если не нашли внешних адресов, показываем localhost
		if (string.IsNullOrEmpty(serverIP))
		{
			GD.Print($"✅ Сервер создан на localhost:{PORT}");
			serverIP = "127.0.0.1";
		}
	}
	
	/// <summary>
	/// Создать сервер
	/// </summary>
	public bool CreateServer(int maxClients = 10)
	{
		GD.Print($"Создание сервера на порту {PORT}...");
		
		peer = new ENetMultiplayerPeer();
		var result = peer.CreateServer(PORT, maxClients);
		
		if (result == Error.Ok)
		{
			Multiplayer.MultiplayerPeer = peer;			
			FillIpServer();
			// Подписываемся на события отключения
			peer.PeerDisconnected += OnPeerDisconnected;
			peer.PeerConnected += OnPeerConnected;
			isServer = true;
			isActive = true;
			
			GD.Print($"✅ Сервер создан на порту {PORT}");
			return true;
		}
		else
		{
			GD.PrintErr($"❌ Не удалось создать сервер: {result}");
			isServer = false;
			isActive = false;
			return false;
		}
	}
	
	/// <summary>
	/// Подключиться как клиент
	/// </summary>
	public bool ConnectAsClient(string ip = SERVER_IP)
	{
		GD.Print($"Подключение к серверу {ip}:{PORT}...");
		
		peer = new ENetMultiplayerPeer();
		var result = peer.CreateClient(ip, PORT);
		
		if (result == Error.Ok)
		{
			Multiplayer.MultiplayerPeer = peer;
			
			// Подписываемся на события отключения
			peer.PeerDisconnected += OnPeerDisconnected;
		//	peer.onConnectionFailed += OnConnectionFailed;
			
			isClient = true;
			isActive = true;
			
			GD.Print($"✅ Подключились к серверу {ip}:{PORT}");
			return true;
		}
		else
		{
			GD.PrintErr($"❌ Не удалось подключиться: {result}");
			isClient = false;
			isActive = false;
			return false;
		}
	}
	
	// Обработчики событий подключения/отключения
	private void OnPeerConnected(long id)
	{
		GD.Print($"🔗 Подключился клиент {id}");
	}
	
	private void OnPeerDisconnected(long id)
	{
		GD.Print($"🔌 Отключился клиент {id}");
		
		if (isClient && id == 1) // ID сервера обычно 1
		{
			GD.Print("Сервер отключился");
			ResetConnection();
		}
	}
	
	private void OnConnectionFailed()
	{
		GD.Print("❌ Ошибка подключения");
		ResetConnection();
	}
	
	/// <summary>
	/// Сбросить состояние подключения
	/// </summary>
	public void ResetConnection()
	{
		if (peer != null)
		{
			peer.Close();
			peer = null;
		}
		
		Multiplayer.MultiplayerPeer = null;
		isServer = false;
		isClient = false;
		isActive = false;
		
		GD.Print("Сброшено состояние подключения");
	}
	
	/// <summary>
	/// Отключиться от сети
	/// </summary>
	public void Disconnect()
	{
		if (peer != null)
		{
			peer.Close();
			peer = null;
			Multiplayer.MultiplayerPeer = null;
			
			isServer = false;
			isClient = false;
			isActive = false;
			
			GD.Print("📭 Отключились от сети");
		}
	}
	
	 /// <summary>
	/// Проверить статус подключения
	/// </summary>
	/// <returns>True если подключение активно</returns>
	public bool CheckConnection() // Переименован метод
	{
		return peer != null && 
			   peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;
	}
	
	// Остальные методы остаются без изменений...
	public void SendMessage(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			GD.Print("❌ Пустое сообщение");
			return;
		}
		
		if (!CheckConnection())
		{
			GD.Print("❌ Нет подключения");
			return;
		}
		
		Rpc(nameof(ReceiveMessage), message);
		GD.Print($"📤 Отправлено: {message}");
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	protected virtual void ReceiveMessage(string text)
	{
		long senderId = Multiplayer.GetRemoteSenderId();
		GD.Print($"📨 Сообщение от {senderId}: {text}");
		OnMessageReceived?.Invoke(senderId, text);
	}
	
	public delegate void MessageReceivedHandler(long senderId, string message);
	public event MessageReceivedHandler OnMessageReceived;
}
