// NetworkCode.cs
// This file contains the backbones that make the chat program work.

#pragma once

// Causes most errors in the chat system to be unhandled. Useful for debugging.
#define EXPOSE_ERRORS

namespace JsChatterBox
{
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::Threading;
	using namespace System::Threading::Tasks;
	using namespace System::Net::Sockets;
	using TcpSocketClient = System::Net::Sockets::TcpClient;

	// Class Declarations

	static ref class NetworkConstants;
	value class PeerIdentity;
	value class PeerMessageDigested;
	ref class PeerConnection;
	ref class ChatClient;
	ref class ChatServer;
	ref class SocketEncoderAssembly;

	// Class Definitions

    static public ref class NetworkConstants
    {
	public:
		static const int DefaultServerPort = 28760;
		static const String^ VersionString = "V0.2";

		static const float MaxMessagesPerSecond = 15;
		static const float HeartBeatInterval = 0.5f;
		static const float HeartBeatSilenceTimeout = 10;
		static const float ConnectionTimeout = 5;
		static const float DisconnectWait = 3;
	};
	public value class PeerIdentity
    {
	public:
		int PeerType; // 0 = Client, 1 = Server
        String^ Name; // A null name means anonymous.
        
		String^ ToString() override;

		PeerIdentity(int PeerType, String^ Name);

		static PeerIdentity GetGeneric(int PeerType);
		static PeerIdentity FromTable(JsEncoder::TableValue^ Table);
		static JsEncoder::TableValue^ ToTable(PeerIdentity Info);
	};
    public value class PeerMessageDigested
    {
	public:
		JsEncoder::TableValue^ Table;

        // Header
        String^ Header;

        // First two values
        JsEncoder::ValueBase^ contents1V;
        JsEncoder::ValueBase^ contents2V;

        // Converted to Strings
        String^ contents1S;
        String^ contents2S;

		PeerMessageDigested(JsEncoder::TableValue^ Table);
	};
    public ref class PeerConnection : public IDisposable
    {
	public:
        // Creation and Destruction
		PeerConnection(PeerIdentity ThisPeerID);
		PeerConnection(PeerIdentity ThisPeerID, TcpSocketClient^ Socket);
		~PeerConnection();
		property bool IsDisposed { bool get() { return !_NotDisposed; } };
	private:
		bool _NotDisposed = true;
		void TryThrowDisposedError();

	public:
        // Properties
        bool OwnsSocket = false; // The socket will also be disposed if this is true.
		property TcpSocketClient^ Socket { TcpSocketClient^ get(){ return _Socket; } }
		property int ConnectionStatus { int get(){ return _ConnectionStatus; } }
		property bool IsConnected { bool get(){ return (_ConnectionStatus == 1); } }
        property PeerIdentity ThisPeerID { PeerIdentity get() { return _ThisPeerID; } }
		property Nullable<PeerIdentity> OtherPeerID { Nullable<PeerIdentity> get() { return _OtherPeerID; } }

        event Action<String^>^ OnHumanLogOutput;

		void RunCycle(float DeltaTime);

        // Connection Management
		void BeginConnect(TcpSocketClient^ Socket);
		void BeginConnect(String^ HostName, int Port);
		void BeginDisconnect(int ReasonID, bool WarnOtherPeer);
		void DropConnection();
		void ChangeID(PeerIdentity NewID);

		void SendMessage(String^ MessageHeader, String^ Contents);
		void SendMessage(String^ MessageHeader, String^ Contents1, String^ Contents2);
		void SendMessage(String^ MessageHeader, IEnumerable<String^>^ Contents);
		void SendMessage(String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents);
		void SendMessage(String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents, bool SendImmediately);
		array<PeerMessageDigested>^ GetMessageOutput();

	private:
        // The Identities this connection deals with.
        PeerIdentity _ThisPeerID;
		Nullable<PeerIdentity> _OtherPeerID = Nullable<PeerIdentity>();

        // Things to manage the connection.
        TcpSocketClient^ _Socket;
        int _ConnectionStatus = 0; // 0 = Disconnected, 1 = Connected, 2 = Connecting, 3 = Disconnecting
        bool _GreetingSent = false;
        bool _GreetingReceived = false;
        SocketEncoderAssembly^ _SocketAdapter;
        List<JsEncoder::ValueBase^>^ _MessageQueue;
        List<PeerMessageDigested>^ _MessageOutput;

        // Timers
        float _MessageCapacityLeft = NetworkConstants::MaxMessagesPerSecond;
        float _ConnectionTimeout = 0;
        float _DisconnectTimer = 0;
        float _HeartBeatSendTimer = 0;
        float _HeartBeatTimeout = 0;

		void SendGreeting();

		void OutputMessage(PeerMessageDigested Message);
		void OnDisconnection();
		void LogHumanMessage(String^ Line);
		void LogSystemHumanMessage(String^ Line);

        // For TcpListener Handling.
	public:
		static PeerConnection^ AcceptConnectionFromTcpListener(PeerIdentity ThisPeerID, TcpListener^ Listener);
		static List<PeerConnection^>^ AcceptConnectionsFromTcpListener(PeerIdentity ThisPeerID, TcpListener^ Listener);
	};
    public ref class ChatClient : public IDisposable
    {
	public:
		property bool IsConnected { bool get() { return _c->IsConnected; } }
		property PeerIdentity ClientID { PeerIdentity get(){ return _c->ThisPeerID; } }
		property String^ ConnectedHostName { String^ get(){ return _ConnectedHostName; } }

		void BeginConnect(String^ HostName, int Port);
		void BeginDisconnect();
		void DropConnection();
		void SendHumanMessage(String^ Contents);
		void ChangeName(String^ NewName);
		void RunCycle(float DeltaTime);

		Dictionary<int, PeerIdentity>^ GetGuestList();
		Nullable<PeerIdentity> GetGuest(int ID);

        event Action<String^>^ OnHumanLogOutput;

		ChatClient();
		ChatClient(PeerIdentity ID);
		~ChatClient();

	private:
        // Used to communicate to the other peer. 
		PeerConnection^ _c;

        // Things about the server
        String^ _ConnectedHostName = nullptr;
        Dictionary<int, PeerIdentity>^ _GuestList = gcnew Dictionary<int, PeerIdentity>();

		void LogHumanMessage(String^ Line);
		Action<String^>^ LogHumanMessageDelegate;
	};
    public ref class ChatServer : public IDisposable
    {
	private:
		ref class ClientInfo sealed : public IDisposable
		{
		public:
			ChatServer^ ParentServer;
			PeerConnection^ c; // Connection
			int ID;

			String^ ToString() override
			{
				return String::Concat(c->OtherPeerID.ToString(), " (", ID.ToString(), ")");
			}

			ClientInfo(ChatServer^ ParentServer, PeerConnection^ Socket, int Id) : ParentServer{ ParentServer }, c{ Socket }, ID{ Id }
			{
				MessageOutputResponderDelegate = gcnew Action<String^>(this, &ClientInfo::MessageOutputResponder);
				c->OnHumanLogOutput += MessageOutputResponderDelegate;
			}
			~ClientInfo()
			{
				if (c != nullptr)
				{
					c->OnHumanLogOutput -= MessageOutputResponderDelegate;
					c->~PeerConnection();
					c = nullptr;
				}
			}

		private:
			Action<String^>^ MessageOutputResponderDelegate;
			void MessageOutputResponder(String^ Message)
			{
				ParentServer->OutputMessageLine(String::Concat(this->ToString(), " ", Message));
			}
		};

	public:
		ChatServer();
		ChatServer(int Port);
		~ChatServer();

		array<PeerIdentity>^ GetGuests();
		array<String^>^ GetGuestList();
		void RunCycle(float DeltaTime);

        event Action<String^>^ OnLineOutput;

	private:
		int NextGuestId = 1;

        TcpListener^ _ListenerSocket;
        List<ClientInfo^>^ Clients = gcnew List<ClientInfo^>();

		void OutputMessageLine(String^ Line);
		void AcceptAll();
		void DropClient(ClientInfo^ client);
		void CheckOnClients();
		void CheckOnClient(ClientInfo^ client);

		void SendClientInfo(ClientInfo^ Client);
		void BroadcastClientInfo();
		void SendMessage(ClientInfo^ Client, String^ MessageHeader, String^ Contents);
		void SendMessage(ClientInfo^ Client, String^ MessageHeader, String^ Contents1, String^ Contents2);
		void SendMessage(ClientInfo^ Client, String^ MessageHeader, IEnumerable<String^>^ Contents);
		void SendMessage(ClientInfo^ Client, String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents);
		void BroadcastMessage(String^ MessageHeader, String^ Contents);
		void BroadcastMessage(String^ MessageHeader, String^ Contents1, String^ Contents2);
		void BroadcastMessage(String^ MessageHeader, IEnumerable<String^>^ Contents);
		void BroadcastMessage(String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents);
	};
    public ref class SocketEncoderAssembly
    {
	public:
		TcpSocketClient^ Socket;

		void InputData(IEnumerable<JsEncoder::ValueBase^>^ Data);
		array<JsEncoder::ValueBase^>^ ReceiveOutput();

		SocketEncoderAssembly(TcpSocketClient^ Socket);
		SocketEncoderAssembly();

	private:
		void InputBytes(array<System::Byte>^ input);
		array<System::Byte>^ ReceiveOutputBytes();

		System::Text::Encoding^ TextFormatEncoder;

		JsEncoder::EncoderStream^ Encoder = gcnew JsEncoder::EncoderStream();
		JsEncoder::DecoderStream^ Decoder = gcnew JsEncoder::DecoderStream();
	};

	// Member Definitions

	String^ PeerIdentity::ToString() { return Name; }
	PeerIdentity::PeerIdentity(int PeerType, String^ Name) : PeerType{ PeerType }, Name{ Name } {}
	PeerIdentity PeerIdentity::GetGeneric(int PeerType) { return PeerIdentity(0, "Unnamed"); }
	PeerIdentity PeerIdentity::FromTable(JsEncoder::TableValue^ Table)
	{
		JsEncoder::IntValue^ PeerType = (JsEncoder::IntValue^)Table->Get(1);
		JsEncoder::StringValue^ Name = (JsEncoder::StringValue^)Table->Get(2);
		return PeerIdentity(PeerType->GetValue(), Name->GetValue());
	}
	JsEncoder::TableValue^ PeerIdentity::ToTable(PeerIdentity Info)
	{
		JsEncoder::TableValue^ r = gcnew JsEncoder::TableValue();
		r->Dictionary[gcnew JsEncoder::IntValue(1)] = gcnew JsEncoder::IntValue(Info.PeerType);
		r->Dictionary[gcnew JsEncoder::IntValue(2)] = gcnew JsEncoder::StringValue(Info.Name);
		return r;
	}

	PeerMessageDigested::PeerMessageDigested(JsEncoder::TableValue^ Table) : Table{ Table }
	{
		JsEncoder::StringValue^ headerV = (JsEncoder::StringValue^)Table->Get(1);
		Header = headerV->GetValue();

		// First two values
		contents1V = Table->Get(2);
		contents2V = Table->Get(3);

		// Converted to Strings
		JsEncoder::StringValue^ contents1SV = dynamic_cast<JsEncoder::StringValue^>(contents1V);
		JsEncoder::StringValue^ contents2SV = dynamic_cast<JsEncoder::StringValue^>(contents2V);
		contents1S = contents1SV != nullptr ? contents1SV->GetValue() : nullptr;
		contents2S = contents2SV != nullptr ? contents2SV->GetValue() : nullptr;
	}

	PeerConnection::PeerConnection(PeerIdentity ThisPeerID)
	{
		_SocketAdapter = gcnew SocketEncoderAssembly();
		_MessageQueue = gcnew List<JsEncoder::ValueBase^>();
		_MessageOutput = gcnew List<PeerMessageDigested>();

		_ThisPeerID = ThisPeerID;
	}
	PeerConnection::PeerConnection(PeerIdentity ThisPeerID, TcpSocketClient^ Socket) : PeerConnection(ThisPeerID) { BeginConnect(Socket); }
	PeerConnection::~PeerConnection()
	{
		if (!_NotDisposed)
		{
			_NotDisposed = false;
			DropConnection();
		}
	}
	void PeerConnection::TryThrowDisposedError()
	{
		if (!_NotDisposed)
			throw gcnew ObjectDisposedException("PeerConnection");
	}
	void PeerConnection::RunCycle(float DeltaTime)
	{
		TryThrowDisposedError();

#ifndef EXPOSE_ERRORS
		try
#endif
		{
			// ## BEGIN ConnectionStateSpecific
			switch (_ConnectionStatus)
			{
			case 1: // Connected

				// Send a HeartBeat
				_HeartBeatSendTimer += DeltaTime;
				if (_HeartBeatSendTimer > NetworkConstants::HeartBeatInterval)
				{
					SendMessage("HEARTBEAT", "");
					_HeartBeatSendTimer = 0;
				}

				// HeartBeat listening timer. "How long ago did I last hear you?"
				_HeartBeatTimeout += DeltaTime;
				if (_HeartBeatTimeout > NetworkConstants::HeartBeatSilenceTimeout)
				{
					_HeartBeatTimeout = 0;
					DropConnection();
					LogSystemHumanMessage("The HeartBeat timeout was exceeded! The connection was dropped.");
				}
				break;
			case 2: // Connecting
				if (!_GreetingSent)
					SendGreeting();
				else if (_GreetingReceived)
					_ConnectionStatus = 1;

				_ConnectionTimeout += DeltaTime;
				if (_ConnectionTimeout > NetworkConstants::ConnectionTimeout)
				{
					_ConnectionTimeout = 0;
					DropConnection();
					LogSystemHumanMessage("The connection timeout was exceeded! The connection was dropped.");
				}

				break;
			case 3: // Disconnecting
				_DisconnectTimer += DeltaTime;
				if (_DisconnectTimer > NetworkConstants::DisconnectWait)
				{
					_DisconnectTimer = 0;
					DropConnection();
					LogSystemHumanMessage("Disconnected!");
				}
				break;
			default:
				break;
			}
			// ## END ConnectionStateSpecific

			// Mailing Procedure
			if (_Socket != nullptr && _Socket->Connected)
			{
				// ## BEGIN Receive
				// Check on what was received.
				array<JsEncoder::ValueBase^>^ output = _SocketAdapter->ReceiveOutput();
				for each (JsEncoder::ValueBase^ item in output)
				{
					_MessageCapacityLeft -= 1;
					if (_MessageCapacityLeft <= 0)
					{
						DropConnection();
						LogSystemHumanMessage("The other peer abusively sent a heap of messages in one go.");
						break;
					}

					// Essential Information
					JsEncoder::TableValue^ outputT = (JsEncoder::TableValue^)item;
					PeerMessageDigested m = PeerMessageDigested(outputT);
					String^ mh = m.Header;

					if (mh == "GREETING"){
						if (_ConnectionStatus == 2)
						{
							bool VersionMatches = (m.contents1S == (String^)NetworkConstants::VersionString);
							PeerIdentity PeerID = PeerIdentity::FromTable((JsEncoder::TableValue^)m.contents2V);
							_OtherPeerID = PeerID;
							_GreetingReceived = true;
							if (VersionMatches)
								LogSystemHumanMessage("The other peer greeted you.");
							else
								LogSystemHumanMessage("The other peer sent you a greeting with an unmatching version string. Incompatibilities may occur.");
							if (_GreetingSent)
								_ConnectionStatus = 1;
						}
					}
					else if (mh == "IDCHANGE")
					{
						JsEncoder::TableValue^ NewIDRaw = (JsEncoder::TableValue^)m.contents1V;
						PeerIdentity NewID = PeerIdentity::FromTable(NewIDRaw);
						_OtherPeerID = NewID;
					}
					else if (mh == "DISCONNECTING")
					{
						if (_ConnectionStatus == 1)
							BeginDisconnect(0, false);
						break;
					}
					else if (mh == "HEARTBEAT")
						_HeartBeatTimeout = 0;

					OutputMessage(PeerMessageDigested(outputT));
				}
				_MessageCapacityLeft = Math::Min(_MessageCapacityLeft + (NetworkConstants::MaxMessagesPerSecond * DeltaTime), NetworkConstants::MaxMessagesPerSecond);
				// ## END Receive
				// ## BEGIN Send
				// Send the pending messages
				// Don't send anything on 3 because we want to stop communications on that stage.
				if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
					_SocketAdapter->InputData(_MessageQueue->ToArray());
				_MessageQueue->Clear();
				// ## END Send
			}
		}
#ifndef EXPOSE_ERRORS
		catch (Exception^ e)
		{
			OnDisconnection();
			LogSystemHumanMessage(String::Concat("The peer connection crashed due to an error. Error Message: ", e->Message));
		}
#endif
	}
	void PeerConnection::BeginConnect(TcpSocketClient^ Socket)
	{
		TryThrowDisposedError();

		if (Socket != nullptr)
		{
			if (_ConnectionStatus == 0)
			{
				_Socket = Socket;
				_SocketAdapter->Socket = Socket;

				_ConnectionStatus = 2;
			}
			else
				throw gcnew Exception("Already connected!");
		}
		else
			throw gcnew ArgumentNullException("Socket");
	}
	void PeerConnection::BeginConnect(String^ HostName, int Port)
	{
		try
		{
			TcpSocketClient^ s = gcnew TcpSocketClient();
			s->Connect(HostName, Port);
			BeginConnect(s);
		}
		catch (Exception^)
		{
			DropConnection();
			LogSystemHumanMessage("Error: Could not connect!");
		}
	}
	void PeerConnection::BeginDisconnect(int ReasonID, bool WarnOtherPeer) // Begins disconnecting from the other peer.
	{
		TryThrowDisposedError();

		if (_ConnectionStatus == 1)
		{
			LogSystemHumanMessage("Disconnecting...");
			if (WarnOtherPeer)
			{
				array<JsEncoder::ValueBase^>^ Param2 = gcnew array<JsEncoder::ValueBase^>(1);
				Param2[0] = gcnew JsEncoder::IntValue(ReasonID);
				SendMessage("DISCONNECTING", Param2, true);
			}
			_ConnectionStatus = 3;
		}
		else
			throw gcnew Exception("Already disconnected!");
	}
	void PeerConnection::DropConnection() // You might want to use this as a last resort in case of an emergency.
	{
		TryThrowDisposedError();

		_ConnectionStatus = 0;
		_MessageQueue->Clear();
		if (_Socket != nullptr)
		{
			if (OwnsSocket)
			{
				_Socket->Close();
				OwnsSocket = false;
			}
			_Socket = nullptr;
			_SocketAdapter->Socket = nullptr;
		}

		_GreetingSent = false;
		_GreetingReceived = false;

		_ConnectionTimeout = 0;
		_DisconnectTimer = 0;
		_HeartBeatSendTimer = 0;
		_HeartBeatTimeout = 0;
		_MessageCapacityLeft = 0;
	}
	void PeerConnection::ChangeID(PeerIdentity NewID)
	{
		_ThisPeerID = NewID;
		if (_ConnectionStatus != 0)
		{
			array<JsEncoder::ValueBase^>^ Param2 = gcnew array<JsEncoder::ValueBase^>(1);
			Param2[0] = PeerIdentity::ToTable(NewID);
			SendMessage("IDCHANGE", Param2);
		}
	}
	void PeerConnection::SendMessage(String^ MessageHeader, String^ Contents) {
		array<String^>^ NewArray = gcnew array<String^>(1);
		NewArray[0] = Contents;
		SendMessage(MessageHeader, NewArray);
	}
	void PeerConnection::SendMessage(String^ MessageHeader, String^ Contents1, String^ Contents2)
	{
		array<String^>^ Param2 = gcnew array<String^>(2);
		Param2[0] = Contents1;
		Param2[1] = Contents2;
		SendMessage(MessageHeader, Param2);
	}
	void PeerConnection::SendMessage(String^ MessageHeader, IEnumerable<String^>^ Contents)
	{
		List<JsEncoder::ValueBase^>^ ResponseArray = gcnew List<JsEncoder::ValueBase^>();
		for each (String^ item in Contents)
			ResponseArray->Add(gcnew JsEncoder::StringValue(item));
		SendMessage(MessageHeader, ResponseArray);
	}
	void PeerConnection::SendMessage(String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents)
	{
		SendMessage(MessageHeader, Contents, false);
	}
	void PeerConnection::SendMessage(String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents, bool SendImmediately)
	{
		if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
		{
			TryThrowDisposedError();

			List<JsEncoder::ValueBase^>^ ResponseArray = gcnew List<JsEncoder::ValueBase^>();
			ResponseArray->Add(gcnew JsEncoder::StringValue(MessageHeader));
			for each (JsEncoder::ValueBase^ item in Contents)
				ResponseArray->Add(item);

			JsEncoder::TableValue^ ResponseTable = JsEncoder::TableValue::ArrayToTable(ResponseArray->ToArray());

			if (SendImmediately)
			{
				array<JsEncoder::ValueBase^>^ Param1 = gcnew array<JsEncoder::ValueBase^>(1);
				Param1[0] = ResponseTable;
				_SocketAdapter->InputData(Param1);
			}
			else
				_MessageQueue->Add(ResponseTable);
		}
	}
	array<PeerMessageDigested>^ PeerConnection::GetMessageOutput()
	{
		TryThrowDisposedError();

		array<PeerMessageDigested>^ r = _MessageOutput->ToArray();
		_MessageOutput->Clear();
		return r;
	}
	void PeerConnection::SendGreeting()
	{
		if (!_GreetingSent)
		{
			array<JsEncoder::ValueBase^>^ Values = gcnew array<JsEncoder::ValueBase^>(2);
			Values[0] = gcnew JsEncoder::StringValue((String^)NetworkConstants::VersionString);
			Values[1] = PeerIdentity::ToTable(_ThisPeerID);

			SendMessage("GREETING", Values);
			_GreetingSent = true;
		}
		else
			LogSystemHumanMessage("SendGreeting was called again even though one was sent. Not sending another one.");
	}
	void PeerConnection::OutputMessage(PeerMessageDigested Message)
	{
		_MessageOutput->Add(Message);
	}
	void PeerConnection::OnDisconnection()
	{
		DropConnection();
		LogSystemHumanMessage("You've lost connection with the other peer.");
	}
	void PeerConnection::LogHumanMessage(String^ Line)
	{
		//if (OnHumanLogOutput != nullptr)
		OnHumanLogOutput(Line);
	}
	void PeerConnection::LogSystemHumanMessage(String^ Line) { LogHumanMessage(String::Concat("Connection: ", Line)); }
	PeerConnection^ PeerConnection::AcceptConnectionFromTcpListener(PeerIdentity ThisPeerID, TcpListener^ Listener)
	{
		PeerConnection^ r = nullptr;
		if (Listener->Pending())
		{
			TcpSocketClient^ ConnectedSocket = Listener->AcceptTcpClient();
			ConnectedSocket->ReceiveTimeout = 5;
			ConnectedSocket->SendTimeout = 5;
			r = gcnew PeerConnection(ThisPeerID, ConnectedSocket);
			r->OwnsSocket = true;
		}
		return r;
	}
	List<PeerConnection^>^ PeerConnection::AcceptConnectionsFromTcpListener(PeerIdentity ThisPeerID, TcpListener^ Listener)
	{
		List<PeerConnection^>^ r = gcnew List<PeerConnection^>();
		while (Listener->Pending())
			AcceptConnectionFromTcpListener(ThisPeerID, Listener);
		return r;
	}

	void ChatClient::BeginConnect(String^ HostName, int Port)
	{
		_c->BeginConnect(HostName, Port);
		_c->OwnsSocket = true;
		_ConnectedHostName = HostName;
	}
	void ChatClient::BeginDisconnect()
	{
		if (_c->IsConnected)
		{
			_c->BeginDisconnect(0, true);

			_GuestList->Clear();
			_ConnectedHostName = nullptr;
		}
	}
	void ChatClient::DropConnection()
	{
		_c->DropConnection();

		_GuestList->Clear();
		_ConnectedHostName = nullptr;
	}
	void ChatClient::SendHumanMessage(String^ Contents)
	{
		if (Contents != nullptr & Contents != "")
			_c->SendMessage("MESSAGE", "", Contents);
	}
	void ChatClient::ChangeName(String^ NewName)
	{
		if (NewName != nullptr && NewName != "")
		{
			PeerIdentity ID = _c->ThisPeerID;
			ID.Name = NewName;
			_c->ChangeID(ID);

			LogHumanMessage(String::Concat("Client: Your name has been changed to ", NewName, "."));
		}
		else
			LogHumanMessage(String::Concat("Client: Your name can't be blanked out!"));
	}
	void ChatClient::RunCycle(float DeltaTime)
	{
		_c->RunCycle(DeltaTime);

		for each (PeerMessageDigested m in _c->GetMessageOutput())
		{
			String^ mh = m.Header;
			if (mh == "MESSAGE")
			{
				int GuestID = ((JsEncoder::IntValue^)m.contents1V)->GetValue();
				Nullable<PeerIdentity> SendingGuest = GetGuest(GuestID);
				String^ GuestName = "";
				if (SendingGuest.HasValue)
					GuestName = SendingGuest.Value.ToString();
				else
					GuestName = String::Concat("Unknown guest with ID ", GuestID.ToString());
				LogHumanMessage(String::Concat(GuestName, " (", GuestID.ToString(), "): ", m.contents2S));
			}
			else if (mh == "GUESTINFOLIST")
			{
				JsEncoder::TableValue^ table = (JsEncoder::TableValue^)m.contents1V;
				_GuestList->Clear();
				for each(KeyValuePair<JsEncoder::ValueBase^, JsEncoder::ValueBase^> pair in table->Dictionary)
				{
					JsEncoder::TableValue^ GuestTable = (JsEncoder::TableValue^)pair.Value;
					PeerIdentity guest = PeerIdentity::FromTable(GuestTable);
					_GuestList->Add(((JsEncoder::IntValue^)pair.Key)->GetValue(), guest);
				}
			}
		}
	}
	Dictionary<int, PeerIdentity>^ ChatClient::GetGuestList() { return _GuestList; }
	Nullable<PeerIdentity> ChatClient::GetGuest(int ID)
	{
		PeerIdentity r;
		return (_GuestList->TryGetValue(ID, r) ? r : Nullable<PeerIdentity>());
	}
	ChatClient::ChatClient() : ChatClient(PeerIdentity::GetGeneric(0)) { }
	ChatClient::ChatClient(PeerIdentity ID)
	{
		LogHumanMessageDelegate = gcnew Action<String^>(this, &ChatClient::LogHumanMessage);
		_c = gcnew PeerConnection(ID);
		_c->OnHumanLogOutput += LogHumanMessageDelegate;
	}
	ChatClient::~ChatClient()
	{
		BeginDisconnect();
		_c->OnHumanLogOutput -= LogHumanMessageDelegate;
		_c->~PeerConnection();
	}
	void ChatClient::LogHumanMessage(String^ Line) { OnHumanLogOutput(Line); }

	ChatServer::ChatServer() : ChatServer(NetworkConstants::DefaultServerPort) { }
	ChatServer::ChatServer(int Port)
	{
		_ListenerSocket = gcnew TcpListener(System::Net::IPAddress::Any, Port);
		_ListenerSocket->Start();
	}
	ChatServer::~ChatServer()
	{
		_ListenerSocket->Stop();
		for each (ClientInfo^ item in Clients->ToArray())
			item->~ClientInfo();
		Clients->Clear();
	}
	array<PeerIdentity>^ ChatServer::GetGuests()
	{
		array<ClientInfo^>^ clients = Clients->ToArray();
		int ClientsLength = clients->Length;
		array<PeerIdentity>^ r = gcnew array<PeerIdentity>(ClientsLength);
		for (int i = 0; i < ClientsLength; i++)
		{
			ClientInfo^ client = clients[i];
			r[i] = client->c->OtherPeerID.Value;
		}
		return r;
	}
	array<String^>^ ChatServer::GetGuestList()
	{
		array<ClientInfo^>^ guests = Clients->ToArray();
		int guestsL = guests->Length;
		array<String^>^ lines = gcnew array<String^>(guestsL);
		for (int i = 0; i < guestsL; i++)
		{
			ClientInfo^ guest = guests[i];
			lines[i] = String::Concat(guest->ToString());
		}
		return lines;
	}
	void ChatServer::RunCycle(float DeltaTime)
	{
		AcceptAll();
		for each (ClientInfo^ item in Clients->ToArray())
			item->c->RunCycle(DeltaTime);
		CheckOnClients();
		BroadcastClientInfo();
	}
	void ChatServer::OutputMessageLine(String^ Line) { OnLineOutput(Line); }
	void ChatServer::AcceptAll()
	{
		bool HasNewGuests = false;
		while (_ListenerSocket->Pending())
		{
			PeerConnection^ NewConnection = PeerConnection::AcceptConnectionFromTcpListener(PeerIdentity(1, "Old Server"), _ListenerSocket);
			ClientInfo^ NewClient = gcnew ClientInfo(this, NewConnection, NextGuestId);
			Clients->Add(NewClient);
			OutputMessageLine(String::Concat("A new guest has connected! It was assigned an ID of ", NextGuestId, "."));

			NextGuestId++;
			HasNewGuests = true;
		}
		if (HasNewGuests) BroadcastClientInfo();
	}
	void ChatServer::DropClient(ClientInfo^ client)
	{
		client->~ClientInfo();
		Clients->Remove(client);
		BroadcastClientInfo();
	}
	void ChatServer::CheckOnClients()
	{
		for each (ClientInfo^ client in Clients->ToArray())
		{
			CheckOnClient(client);
		}
	}
	void ChatServer::CheckOnClient(ClientInfo^ client)
	{
		if (client->c->ConnectionStatus != 0)
		{
#ifndef EXPOSE_ERRORS
			try
#endif
			{
				// Receive a command and send an answer.
				array<PeerMessageDigested>^ output = client->c->GetMessageOutput();
				for each (PeerMessageDigested m in output)
				{
					if (m.Header == "MESSAGE")
					{
						int ClientId = client->ID;
						String^ MessageText = m.contents2S;

						OutputMessageLine(String::Concat(client->ToString(), " says: ", MessageText));
						array<JsEncoder::ValueBase^>^ Param2 = gcnew array<JsEncoder::ValueBase^>(2);
						Param2[0] = gcnew JsEncoder::IntValue(ClientId);
						Param2[1] = gcnew JsEncoder::StringValue(MessageText);
						BroadcastMessage("MESSAGE", Param2);
					}
				}
			}
#ifndef EXPOSE_ERRORS
			catch (Exception^ e)
			{
				OutputMessageLine(String::Concat("Error: ", e->Message));
				DropClient(client);
			}
#endif
		}
		else
		{
			String^ name = client->ToString();
			DropClient(client);
			OutputMessageLine(String::Concat(name, " has disconnected from the server."));
		}
	}
	void ChatServer::SendClientInfo(ClientInfo^ Client)
	{
		JsEncoder::TableValue^ table = gcnew JsEncoder::TableValue();
		array<ClientInfo^>^ ClientArray = Clients->ToArray();
		for each (ClientInfo^ client in ClientArray)
		{
			Nullable<PeerIdentity> ClientID = client->c->OtherPeerID;
			table->Dictionary->Add(gcnew JsEncoder::IntValue(client->ID), (ClientID.HasValue ? PeerIdentity::ToTable(ClientID.Value) : nullptr));
		}
		array<JsEncoder::ValueBase^>^ Param2 = gcnew array<JsEncoder::ValueBase^>(1);
		Param2[0] = table;
		if (Client != nullptr)
			SendMessage(Client, "GUESTINFOLIST", Param2);
		else
			BroadcastMessage("GUESTINFOLIST", Param2);
	}
	void ChatServer::BroadcastClientInfo() { SendClientInfo(nullptr); }
	void ChatServer::SendMessage(ClientInfo^ Client, String^ MessageHeader, String^ Contents) { Client->c->SendMessage(MessageHeader, Contents); }
	void ChatServer::SendMessage(ClientInfo^ Client, String^ MessageHeader, String^ Contents1, String^ Contents2) { Client->c->SendMessage(MessageHeader, Contents1, Contents2); }
	void ChatServer::SendMessage(ClientInfo^ Client, String^ MessageHeader, IEnumerable<String^>^ Contents) { Client->c->SendMessage(MessageHeader, Contents); }
	void ChatServer::SendMessage(ClientInfo^ Client, String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents) { Client->c->SendMessage(MessageHeader, Contents); }
	void ChatServer::BroadcastMessage(String^ MessageHeader, String^ Contents)
	{
		for each (ClientInfo^ item in Clients->ToArray())
			item->c->SendMessage(MessageHeader, Contents);
	}
	void ChatServer::BroadcastMessage(String^ MessageHeader, String^ Contents1, String^ Contents2)
	{
		for each (ClientInfo^ item in Clients->ToArray())
			item->c->SendMessage(MessageHeader, Contents1, Contents2);
	}
	void ChatServer::BroadcastMessage(String^ MessageHeader, IEnumerable<String^>^ Contents)
	{
		for each (ClientInfo^ item in Clients->ToArray())
			item->c->SendMessage(MessageHeader, Contents);
	}
	void ChatServer::BroadcastMessage(String^ MessageHeader, IEnumerable<JsEncoder::ValueBase^>^ Contents)
	{
		for each (ClientInfo^ item in Clients->ToArray())
			item->c->SendMessage(MessageHeader, Contents);
	}

	void SocketEncoderAssembly::InputData(IEnumerable<JsEncoder::ValueBase^>^ Data)
	{
		for each (JsEncoder::ValueBase^ item in Data)
			Encoder->InputValue(item);
		String^ EncodedData = Encoder->PopOutput();
		array<wchar_t>^ EncodedChars = EncodedData->ToCharArray();
		array<System::Byte>^ EncodedBytes = TextFormatEncoder->GetBytes(EncodedChars);
		InputBytes(EncodedBytes);
	}
	array<JsEncoder::ValueBase^>^ SocketEncoderAssembly::ReceiveOutput()
	{
		array<System::Byte>^ EncodedBytes = ReceiveOutputBytes();
		array<wchar_t>^ EncodedChars = TextFormatEncoder->GetChars(EncodedBytes);
		String^ EncodedData = gcnew String(EncodedChars);
		Decoder->InputValue(EncodedData);
		Decoder->RunParser();
		return Decoder->PopOutput();
	}
	SocketEncoderAssembly::SocketEncoderAssembly(TcpSocketClient^ Socket)
	{
		this->TextFormatEncoder = System::Text::Encoding::Unicode;
		this->Socket = Socket;
	}
	SocketEncoderAssembly::SocketEncoderAssembly() : SocketEncoderAssembly(nullptr) { }
	void SocketEncoderAssembly::InputBytes(array<System::Byte>^ input) { Socket->Client->Send(input); }
	array<System::Byte>^ SocketEncoderAssembly::ReceiveOutputBytes()
	{
		int NumberOfBytes = Socket->Available;
		array<System::Byte>^ EncodedBytes = gcnew array<System::Byte>(NumberOfBytes);
		int BytesReceivedBySocket = 0;
		if (NumberOfBytes > 0)
			BytesReceivedBySocket = Socket->Client->Receive(EncodedBytes);
		return EncodedBytes;
	}
}

#ifdef EXPOSE_ERRORS
#undef EXPOSE_ERRORS
#endif