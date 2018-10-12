// NetworkCode.cs
// This file contains the backbones that make the chat program work.

// Causes most errors in the chat system to be unhandled. Useful for debugging.
//#define EXPOSE_ERRORS

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using Text = System.Text;
using TcpSocketClient = System.Net.Sockets.TcpClient;

namespace JsChatterBox
{
    public static class NetworkConstants
    {
        public const int DefaultServerPort = 28760;
        public const String VersionString = "V0.2";

        public const float MaxMessagesPerSecond = 15f;
        public const float HeartBeatInterval = 0.5f;
        public const float HeartBeatSilenceTimeout = 10f;
        public const float ConnectionTimeout = 5f;
        public const float DisconnectWait = 3f;
    }
    public class ChatClient : IDisposable
    {
        public bool IsConnected { get { return _c.IsConnected; } }
        public PeerIdentity ClientID { get { return _c.ThisPeerID; } }
        public String ConnectedHostName { get { return _ConnectedHostName; } }

        public void BeginConnect(String HostName, int Port)
        {
            _c.BeginConnect(HostName, Port);
            _c.OwnsSocket = true;
            _ConnectedHostName = HostName;
        }
        public void BeginDisconnect()
        {
            if (_c.IsConnected)
            {
                _c.BeginDisconnect(0, true);

                _GuestList.Clear();
                _ConnectedHostName = null;
            }
        }
        public void DropConnection()
        {
            _c.DropConnection();

            _GuestList.Clear();
            _ConnectedHostName = null;
        }
        public void SendHumanMessage(String Contents)
        {
            if (Contents != null & Contents != "")
                _c.SendMessage("MESSAGE", "", Contents);
        }
        public void ChangeName(String NewName)
        {
            if (NewName != null && NewName != "")
            {
                PeerIdentity ID = _c.ThisPeerID;
                ID.Name = NewName;
                _c.ChangeID(ID);

                LogHumanMessage(String.Concat("Client: Your name has been changed to ", NewName, "."));
            }
            else
                LogHumanMessage(String.Concat("Client: Your name can't be blanked out!"));
        }

        public void RunCycle(float DeltaTime)
        {
            _c.RunCycle(DeltaTime);

            foreach (PeerMessageDigested m in _c.GetMessageOutput())
            {
                switch (m.Header)
                {
                    case "MESSAGE":
                        int GuestID = ((JsEncoder.IntValue)m.contents1V).Value;
                        PeerIdentity? SendingGuest = GetGuest(GuestID);
                        String GuestName = "";
                        if (SendingGuest.HasValue)
                            GuestName = SendingGuest.Value.ToString();
                        else
                            GuestName = String.Concat("Unknown guest with ID ", GuestID.ToString());
                        LogHumanMessage(String.Concat(GuestName , " (", GuestID.ToString(), "): ", m.contents2S));
                        break;
                    case "GUESTINFOLIST":
                        JsEncoder.TableValue table = (JsEncoder.TableValue)m.contents1V;
                        _GuestList.Clear();
                        foreach (var pair in table.Dictionary)
                        {
                            JsEncoder.TableValue GuestTable = (JsEncoder.TableValue)pair.Value;
                            PeerIdentity guest = PeerIdentity.FromTable(GuestTable);
                            _GuestList.Add(((JsEncoder.IntValue)pair.Key).Value, guest);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public Dictionary<int, PeerIdentity> GetGuestList() { return _GuestList; }
        public PeerIdentity? GetGuest(int ID)
        {
            PeerIdentity r;
            return (_GuestList.TryGetValue(ID, out r) ? (PeerIdentity?)r : null);
        }

        public event Action<String> OnHumanLogOutput;

        public ChatClient() : this(PeerIdentity.GetGeneric(0)) { }
        public ChatClient(PeerIdentity ID)
        {
            _c = new PeerConnection(ID);
            _c.OnHumanLogOutput += LogHumanMessage;
        }
        public void Dispose()
        {
            BeginDisconnect();
            _c.OnHumanLogOutput -= LogHumanMessage;
            _c.Dispose();
        }

        #region PrivateMembers

        // Used to communicate to the other peer.
        private PeerConnection _c;

        // Things about the server
        private String _ConnectedHostName = null;
        private Dictionary<int, PeerIdentity> _GuestList = new Dictionary<int, PeerIdentity>();

        private void LogHumanMessage(String Line)
        {
            if (OnHumanLogOutput != null)
                OnHumanLogOutput(Line);
        }

        #endregion
    }
    public class ChatServer : IDisposable
    {
        public ChatServer() : this(NetworkConstants.DefaultServerPort) { }
        public ChatServer(int Port)
        {
            _ListenerSocket = new TcpListener(System.Net.IPAddress.Any, Port);
            _ListenerSocket.Start();
        }
        public void Dispose()
        {
            _ListenerSocket.Stop();
            foreach (var item in Clients)
                item.Dispose();
        }

        public PeerIdentity[] GetGuests()
        {
            ClientInfo[] clients = Clients.ToArray();
            int ClientsLength = clients.Length;
            PeerIdentity[] r = new PeerIdentity[ClientsLength];
            for (int i = 0; i < ClientsLength; i++)
            {
                r[i] = clients[i].c.OtherPeerID.Value;
            }
            return r;
        }
        public String[] GetGuestList()
        {
            ClientInfo[] guests = Clients.ToArray();
            int guestsL = guests.Length;
            String[] lines = new String[guestsL];
            for (int i = 0; i < guestsL; i++)
            {
                ClientInfo guest = guests[i];
                lines[i] = String.Concat(guest.ToString());
            }
            return lines;
        }
        public void RunCycle(float DeltaTime)
        {
            AcceptAll();
            foreach (var item in Clients.ToArray())
            {
                item.c.RunCycle(DeltaTime);
            }
            CheckOnClients();
            BroadcastClientInfo();
        }

        public event Action<String> OnLineOutput;

        private int NextGuestId = 1;

        private TcpListener _ListenerSocket;
        private List<ClientInfo> Clients = new List<ClientInfo>();

        private void OutputMessageLine(String Line)
        {
            if (OnLineOutput != null) OnLineOutput(Line);
        }
        private void AcceptAll()
        {
            bool HasNewGuests = false;
            while (_ListenerSocket.Pending())
            {
                PeerConnection NewConnection = PeerConnection.AcceptConnectionFromTcpListener(new PeerIdentity(1, "Old Server"), _ListenerSocket);
                ClientInfo NewClient = new ClientInfo(NewConnection, NextGuestId);
                Clients.Add(NewClient);
                OutputMessageLine(String.Concat("A new guest has connected! It was assigned an ID of ", NextGuestId, "."));

                NextGuestId++;
                HasNewGuests = true;
            }
            if (HasNewGuests) BroadcastClientInfo();
        }
        private void DropClient(ClientInfo client)
        {
            client.Dispose();
            Clients.Remove(client);
            BroadcastClientInfo();
        }
        private void CheckOnClients()
        {
            foreach (ClientInfo client in Clients.ToArray())
            {
                CheckOnClient(client);
            }
        }
        private void CheckOnClient(ClientInfo client)
        {
            if (client.c.ConnectionStatus != 0)
            {
#if !EXPOSE_ERRORS
                try
#endif
                {
                    // Receive a command and send an answer.
                    PeerMessageDigested[] output = client.c.GetMessageOutput();
                    foreach (PeerMessageDigested m in output)
                    {
                        switch (m.Header)
                        {
                            case "MESSAGE":
                                int ClientId = client.ID;
                                String MessageText = m.contents2S;

                                OutputMessageLine(String.Concat(client.ToString(), " says: ", MessageText));
                                BroadcastMessage("MESSAGE", new JsEncoder.ValueBase[] { new JsEncoder.IntValue(ClientId), new JsEncoder.StringValue(MessageText) });
                                break;
                            default:
                                break;
                        }
                    }
                }
#if !EXPOSE_ERRORS
                catch (Exception e)
                {
                    OutputMessageLine(String.Concat("Error: ", e.Message));
                    DropClient(client);
                }
#endif
            }
            else
            {
                String name = client.ToString();
                client.Dispose();
                Clients.Remove(client);
                OutputMessageLine(String.Concat(name, " has disconnected from the server."));
            }
        }

        private void SendClientInfo(ClientInfo Client)
        {
            JsEncoder.TableValue table = new JsEncoder.TableValue();
            ClientInfo[] ClientArray = Clients.ToArray();
            foreach (ClientInfo client in ClientArray)
            {
                PeerIdentity? ClientID = client.c.OtherPeerID;
                table.Dictionary.Add(new JsEncoder.IntValue(client.ID), ClientID.HasValue ? PeerIdentity.ToTable(ClientID.Value) : null);
			}
            if (Client != null)
                SendMessage(Client, "GUESTINFOLIST", new JsEncoder.ValueBase[] { table });
            else
                BroadcastMessage("GUESTINFOLIST", new JsEncoder.ValueBase[] { table });
        }
        private void BroadcastClientInfo() { SendClientInfo(null); }
        private void SendMessage(ClientInfo Client, String MessageHeader, String Contents) { SendMessage(Client, MessageHeader, new String[] { Contents }); }
        private void SendMessage(ClientInfo Client, String MessageHeader, String Contents1, String Contents2) { SendMessage(Client, MessageHeader, new String[] { Contents1, Contents2 }); }
        private void SendMessage(ClientInfo Client, String MessageHeader, IEnumerable<String> Contents)
        {
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            foreach (String item in Contents)
		        ResponseArray.Add(new JsEncoder.StringValue(item));
            SendMessage(Client, MessageHeader, ResponseArray);
        }
        private void SendMessage(ClientInfo Client, String MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents) { Client.c.SendMessage(MessageHeader, Contents); }
        private void BroadcastMessage(String MessageHeader, String Contents) { BroadcastMessage(MessageHeader, new String[] { Contents }); }
        private void BroadcastMessage(String MessageHeader, String Contents1, String Contents2) { BroadcastMessage(MessageHeader, new String[] { Contents1, Contents2 }); }
        private void BroadcastMessage(String MessageHeader, IEnumerable<String> Contents)
        {
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            foreach (String item in Contents)
		        ResponseArray.Add(new JsEncoder.StringValue(item));
            BroadcastMessage(MessageHeader, ResponseArray);
        }
        private void BroadcastMessage(String MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents)
        {
            foreach (ClientInfo item in Clients.ToArray())
                item.c.SendMessage(MessageHeader, Contents);
        }

        private sealed class ClientInfo : IDisposable
        {
            public PeerConnection c; // Connection
            public int ID;

            public override string ToString()
            {
                return String.Concat(c.OtherPeerID.ToString(), " (", ID.ToString(), ")");
            }

            public ClientInfo(PeerConnection Socket, int Id)
            {
                this.c = Socket;
                this.ID = Id;
            }
            public void Dispose()
            {
                if (c != null)
                { 
                    c.Dispose();
                    c = null;
                }
            }
        }
    }
    public class PeerConnection : IDisposable
    {
        // Creation and Destruction
        public PeerConnection(PeerIdentity ThisPeerID) { _ThisPeerID = ThisPeerID; }
        public PeerConnection(PeerIdentity ThisPeerID, TcpSocketClient Socket) : this(ThisPeerID) { BeginConnect(Socket); }
        public void Dispose()
        {
            if (!_NotDisposed)
            {
                _NotDisposed = false;
                DropConnection();
            }
        }
        public bool IsDisposed { get { return !_NotDisposed; } }
        private bool _NotDisposed = true;
        private void TryThrowDisposedError() { if (!_NotDisposed) throw new ObjectDisposedException("PeerConnection"); }

        // Properties
        public TcpSocketClient Socket { get { return _Socket; } }
        public int ConnectionStatus { get { return _ConnectionStatus; } }
        public bool IsConnected { get { return (_ConnectionStatus == 1); } }
        public PeerIdentity ThisPeerID { get { return _ThisPeerID; } }
        public PeerIdentity? OtherPeerID { get { return _OtherPeerID; } }
        public bool OwnsSocket = false; // The socket will also be disposed if this is true.

        public event Action<String> OnHumanLogOutput;

        public void RunCycle(float DeltaTime)
        {
            TryThrowDisposedError();

#if !EXPOSE_ERRORS
            try
#endif
            {
                #region ConnectionStateSpecific
                switch (_ConnectionStatus)
                {
                    case 1: // Connected
                        // Send a HeartBeat
                        _HeartBeatSendTimer += DeltaTime;
                        if (_HeartBeatSendTimer > NetworkConstants.HeartBeatInterval)
                        {
                            SendMessage("HEARTBEAT", "");
                            _HeartBeatSendTimer = 0;
                        }

                        // HeartBeat listening timer. "How long ago did I last hear you?"
                        _HeartBeatTimeout += DeltaTime;
                        if (_HeartBeatTimeout > NetworkConstants.HeartBeatSilenceTimeout)
                        {
                            _HeartBeatTimeout = 0f;
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
                        if (_ConnectionTimeout > NetworkConstants.ConnectionTimeout)
                        {
                            _ConnectionTimeout = 0f;
                            DropConnection();
                            LogSystemHumanMessage("The connection timeout was exceeded! The connection was dropped.");
                        }

                        break;
                    case 3: // Disconnecting
                        _DisconnectTimer += DeltaTime;
                        if (_DisconnectTimer > NetworkConstants.DisconnectWait)
                        {
                            _DisconnectTimer = 0f;
                            DropConnection();
                            LogSystemHumanMessage("Disconnected!");
                        }
                        break;
                    default:
                        break;
                }
                #endregion

                // Mailing Procedure
                if (_Socket != null && _Socket.Connected)
                {
                    #region Receive
                    // Check on what was received.
                    JsEncoder.ValueBase[] output = _SocketAdapter.ReceiveOutput();
                    foreach (JsEncoder.ValueBase item in output)
                    {
                        _MessageCapacityLeft -= 1;
                        if (_MessageCapacityLeft <= 0)
                        {
                            DropConnection();
                            LogSystemHumanMessage("The other peer abusively sent a heap of messages in one go.");
                            break;
                        }

                        // Essential Information
                        JsEncoder.TableValue outputT = (JsEncoder.TableValue)item;
                        PeerMessageDigested m = new PeerMessageDigested(outputT);

                        switch (m.Header)
                        {
                            case "GREETING":
                                if (_ConnectionStatus == 2)
                                {
                                    bool VersionMatches = (m.contents1S == NetworkConstants.VersionString);
                                    PeerIdentity PeerID = PeerIdentity.FromTable((JsEncoder.TableValue)m.contents2V);
                                    _OtherPeerID = PeerID;
                                    _GreetingReceived = true;
                                    if (VersionMatches)
                                        LogSystemHumanMessage("The other peer greeted you.");
                                    else
                                        LogSystemHumanMessage("The other peer sent you a greeting with an unmatching version string. Incompatibilities may occur.");
                                    if (_GreetingSent)
                                        _ConnectionStatus = 1;
                                }
                                break;
                            case "IDCHANGE":
                                JsEncoder.TableValue NewIDRaw = (JsEncoder.TableValue)m.contents1V;
                                PeerIdentity NewID = PeerIdentity.FromTable(NewIDRaw);
                                _OtherPeerID = NewID;

                                break;
                            case "DISCONNECTING":
                                if (_ConnectionStatus == 1)
                                    BeginDisconnect(0, false);
                                break;
                            //case "MESSAGE":
                            //    break;
                            case "HEARTBEAT":
                                _HeartBeatTimeout = 0f;
                                break;
                            default:
                                break;
                        }
                        OutputMessage(new PeerMessageDigested(outputT));
                    }
                    _MessageCapacityLeft = Math.Min(_MessageCapacityLeft + (NetworkConstants.MaxMessagesPerSecond * DeltaTime), NetworkConstants.MaxMessagesPerSecond);
                    #endregion
                    #region Send
                    // Send the pending messages
                    // Don't send anything on 3 because we want to stop communications on that stage.
                    if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
                        _SocketAdapter.InputData(_MessageQueue.ToArray());
                    _MessageQueue.Clear();
                    #endregion
                }
            }
#if !EXPOSE_ERRORS
            catch (Exception e)
            {
                OnDisconnection();
                LogSystemHumanMessage(String.Concat("The peer connection crashed due to an error. Error Message: ", e.Message));
            }
#endif
        }

        // Connection Management
        public void BeginConnect(TcpSocketClient Socket)
        {
            TryThrowDisposedError();

            if (Socket != null)
            {
                if (_ConnectionStatus == 0)
                {
                    _Socket = Socket;
                    _SocketAdapter.Socket = Socket;

                    _ConnectionStatus = 2;
                }
                else
                    throw new Exception("Already connected!");
            }
            else
                throw new ArgumentNullException("Socket");
        }
        public void BeginConnect(String HostName, int Port)
        {
            try
            {
                TcpSocketClient s = new TcpSocketClient();
                s.Connect(HostName, Port);
                BeginConnect(s);
            }
            catch (Exception)
            {
                DropConnection();
                LogSystemHumanMessage("Error: Could not connect!");
            }
        }
        public void BeginDisconnect(int ReasonID, bool WarnOtherPeer) // Begins disconnecting from the other peer.
        {
            TryThrowDisposedError();

            if (_ConnectionStatus == 1)
            {
                LogSystemHumanMessage("Disconnecting...");
                if (WarnOtherPeer) SendMessage("DISCONNECTING", new JsEncoder.ValueBase[] { new JsEncoder.IntValue(ReasonID) }, true);
                _ConnectionStatus = 3;
            }
            else
                throw new Exception("Already disconnected!");
        }
        public void DropConnection() // You might want to use this as a last resort in case of an emergency.
        {
            TryThrowDisposedError();

            _ConnectionStatus = 0;
            _MessageQueue.Clear();
            if (_Socket != null)
            {
                if (OwnsSocket)
                {
                    _Socket.Close();
                    OwnsSocket = false;
                }
                _Socket = null;
                _SocketAdapter.Socket = null;
            }

            _GreetingSent = false;
            _GreetingReceived = false;

            _ConnectionTimeout = 0f;
            _DisconnectTimer = 0f;
            _HeartBeatSendTimer = 0f;
            _HeartBeatTimeout = 0f;
            _MessageCapacityLeft = 0f;
        }
        public void ChangeID(PeerIdentity NewID)
        {
            _ThisPeerID = NewID;
            if (_ConnectionStatus != 0)
                SendMessage("IDCHANGE", new JsEncoder.ValueBase[] { NewID.ToTable() });
        }

        public void SendMessage(String MessageHeader, String Contents) { SendMessage(MessageHeader, new String[] { Contents }); }
        public void SendMessage(String MessageHeader, String Contents1, String Contents2) { SendMessage(MessageHeader, new String[] { Contents1, Contents2 }); }
        public void SendMessage(String MessageHeader, IEnumerable<String> Contents)
        {
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            foreach (String item in Contents)
                ResponseArray.Add(new JsEncoder.StringValue(item));
            SendMessage(MessageHeader, ResponseArray);
        }
        public void SendMessage(String MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents)
        {
            SendMessage(MessageHeader, Contents, false);
        }
        public void SendMessage(String MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents, bool SendImmediately)
        {
            if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
            {
                TryThrowDisposedError();

                List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
                ResponseArray.Add(new JsEncoder.StringValue(MessageHeader));
                foreach (var item in Contents)
                    ResponseArray.Add(item);

                JsEncoder.TableValue ResponseTable = JsEncoder.TableValue.ArrayToTable(ResponseArray.ToArray());

                if (SendImmediately)
                    _SocketAdapter.InputData(new JsEncoder.ValueBase[] { ResponseTable });
                else
                    _MessageQueue.Add(ResponseTable);
            }
        }
        public PeerMessageDigested[] GetMessageOutput()
        {
            TryThrowDisposedError();

            PeerMessageDigested[] r = _MessageOutput.ToArray();
            _MessageOutput.Clear();
            return r;
        }

        // The Identities this connection deals with.
        private PeerIdentity _ThisPeerID;
        private PeerIdentity? _OtherPeerID = null;

        // Things to manage the connection.
        private TcpSocketClient _Socket;
        private int _ConnectionStatus = 0; // 0 = Disconnected, 1 = Connected, 2 = Connecting, 3 = Disconnecting
        private bool _GreetingSent = false;
        private bool _GreetingReceived = false;
        private SocketEncoderAssembly _SocketAdapter = new SocketEncoderAssembly();
        private List<JsEncoder.ValueBase> _MessageQueue = new List<JsEncoder.ValueBase>();
        private List<PeerMessageDigested> _MessageOutput = new List<PeerMessageDigested>();

        // Timers
        private float _MessageCapacityLeft = NetworkConstants.MaxMessagesPerSecond;
        private float _ConnectionTimeout = 0f;
        private float _DisconnectTimer = 0f;
        private float _HeartBeatSendTimer = 0f;
        private float _HeartBeatTimeout = 0f;

        private void SendGreeting()
        {
            if (!_GreetingSent)
            {
                SendMessage("GREETING", new JsEncoder.ValueBase[]{
                    new JsEncoder.StringValue(NetworkConstants.VersionString),
                    _ThisPeerID.ToTable(),
                });
                _GreetingSent = true;
            }
            else
                LogSystemHumanMessage("SendGreeting was called again even though one was sent. Not sending another one.");
        }

        private void OutputMessage(PeerMessageDigested Message)
        {
            _MessageOutput.Add(Message);
        }
        private void OnDisconnection()
        {
            DropConnection();
            LogSystemHumanMessage("You've lost connection with the other peer.");
        }
        private void LogHumanMessage(String Line)
        {
            if (OnHumanLogOutput != null)
                OnHumanLogOutput(Line);
        }
        private void LogSystemHumanMessage(String Line) { LogHumanMessage(String.Concat("Connection: ", Line)); }

        // For TcpListener Handling.
        public static PeerConnection AcceptConnectionFromTcpListener(PeerIdentity ThisPeerID, TcpListener Listener)
        {
            PeerConnection r = null;
            if (Listener.Pending())
            {
                TcpSocketClient ConnectedSocket = Listener.AcceptTcpClient();
                ConnectedSocket.ReceiveTimeout = 5;
                ConnectedSocket.SendTimeout = 5;
                r = new PeerConnection(ThisPeerID, ConnectedSocket);
                r.OwnsSocket = true;
            }
            return r;
        }
        public static List<PeerConnection> AcceptConnectionsFromTcpListener(PeerIdentity ThisPeerID, TcpListener Listener)
        {
            List<PeerConnection> r = new List<PeerConnection>();
            while (Listener.Pending())
                AcceptConnectionFromTcpListener(ThisPeerID, Listener);
            return r;
        }
    }

    public class SocketEncoderAssembly
    {
        public TcpSocketClient Socket;

        public void InputData(IEnumerable<JsEncoder.ValueBase> Data)
        {
            foreach (JsEncoder.ValueBase item in Data)
                Encoder.InputValue(item);
            String EncodedData = Encoder.PopOutput();
            char[] EncodedChars = EncodedData.ToCharArray();
            byte[] EncodedBytes = TextFormatEncoder.GetBytes(EncodedChars);
            InputBytes(EncodedBytes);
        }
        public JsEncoder.ValueBase[] ReceiveOutput()
        {
            byte[] EncodedBytes = ReceiveOutputBytes();
            char[] EncodedChars = TextFormatEncoder.GetChars(EncodedBytes);
            String EncodedData = new String(EncodedChars);
            Decoder.InputValue(EncodedData);
            Decoder.RunParser();
            return Decoder.PopOutput();
        }

        public SocketEncoderAssembly(TcpSocketClient Socket) { this.Socket = Socket; }
        public SocketEncoderAssembly() : this(null) { }

        private void InputBytes(byte[] input) { Socket.Client.Send(input); }
        private byte[] ReceiveOutputBytes()
        {
            int NumberOfBytes = Socket.Available;
            byte[] EncodedBytes = new byte[NumberOfBytes];
            int BytesReceivedBySocket = 0;
            if (NumberOfBytes > 0)
                BytesReceivedBySocket = Socket.Client.Receive(EncodedBytes);
            return EncodedBytes;
        }

        private Text.Encoding TextFormatEncoder = Text.Encoding.Unicode;

        private JsEncoder.EncoderStream Encoder = new JsEncoder.EncoderStream();
        private JsEncoder.DecoderStream Decoder = new JsEncoder.DecoderStream();
    }

    public struct PeerIdentity
    {
        public int PeerType; // 0 = Client, 1 = Server
        public String Name; // A null name means anonymous.
        
        public JsEncoder.TableValue ToTable() { return ToTable(this); }
        public override string ToString() { return Name; }

        public PeerIdentity(int PeerType, String Name)
        {
            this.PeerType = PeerType;
            this.Name = Name;
        }

        public static PeerIdentity GetGeneric(int PeerType) { return new PeerIdentity(0, "Unnamed"); }
        public static PeerIdentity FromTable(JsEncoder.TableValue Table)
        {
            JsEncoder.IntValue PeerType = (JsEncoder.IntValue)Table.Get(1);
            JsEncoder.StringValue Name = (JsEncoder.StringValue)Table.Get(2);
            return new PeerIdentity(PeerType.Value, Name.Value);
        }
        public static JsEncoder.TableValue ToTable(PeerIdentity Info)
        { 
            JsEncoder.TableValue r = new JsEncoder.TableValue();
            r.Dictionary[new JsEncoder.IntValue(1)] = new JsEncoder.IntValue(Info.PeerType);
            r.Dictionary[new JsEncoder.IntValue(2)] = new JsEncoder.StringValue(Info.Name);
            return r;
        }
    }
    public struct PeerMessageDigested
    {
        public readonly JsEncoder.TableValue Table;

        // Header
        public readonly String Header;

        // First two values
        public readonly JsEncoder.ValueBase contents1V;
        public readonly JsEncoder.ValueBase contents2V;

        // Converted to Strings
        public readonly String contents1S;
        public readonly String contents2S;

        public PeerMessageDigested(JsEncoder.TableValue Table)
        {
            this.Table = Table;

            JsEncoder.StringValue headerV = (JsEncoder.StringValue)Table.Get(1);
            Header = headerV.Value;

            // First two values
            contents1V = Table.Get(2);
            contents2V = Table.Get(3);

            // Converted to Strings
            JsEncoder.StringValue contents1SV = (contents1V as JsEncoder.StringValue);
            JsEncoder.StringValue contents2SV = (contents2V as JsEncoder.StringValue);
            contents1S = contents1SV != null ? contents1SV.Value : null;
            contents2S = contents2SV != null ? contents2SV.Value : null;
        }
    }
}
