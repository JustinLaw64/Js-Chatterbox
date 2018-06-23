//#define EXPOSE_ERRORS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpClient = System.Net.Sockets.TcpClient;

namespace JsChatterBox.Networking
{
    public static class NetworkConfig
    {
        public const int DefaultServerPort = 28760;
        public const String VersionString = "V0.5dev"; // Protocol Version

        public const float MaxMessagesPerSecond = 15;
        public const float HeartBeatInterval = 0.5f;
        public const float HeartBeatSilenceTimeout = 10;
        public const float ConnectionTimeout = 20;
        public const float DisconnectWait = 3;
    }
    /// <summary>
    /// Handles the parsing exchanges between JsEncoder objects and a TcpClient,
    /// facilitating network communications using JsEncoder.
    /// 
    /// Note:
    /// 
    /// This does not handle anything more than data conversion and sending/receiving.
    /// </summary>
    public class SocketEncoderAssembly
    {
        public TcpClient Socket;

        public void SendInput(IEnumerable<JsEncoder.ValueBase> Data)
        {
            foreach (JsEncoder.ValueBase item in Data)
                _Encoder.InputValue(item);
            string EncodedData = _Encoder.PopOutput();
            char[] EncodedChars = EncodedData.ToCharArray();
            byte[] EncodedBytes = _EncodingFormat.GetBytes(EncodedChars);
            InputBytes(EncodedBytes);
        }
        public JsEncoder.ValueBase[] ReceiveOutput()
        {
            byte[] EncodedBytes = ReceiveOutputBytes();
            char[] EncodedChars = _EncodingFormat.GetChars(EncodedBytes);
            string EncodedData = new string(EncodedChars);
            _Decoder.InputValue(EncodedData);
            _Decoder.RunParser();
            return _Decoder.PopOutput();
        }

        public SocketEncoderAssembly(TcpClient Socket) { this.Socket = Socket; }
        public SocketEncoderAssembly() : this(null) { }

        private System.Text.Encoding _EncodingFormat = System.Text.Encoding.Unicode;
        private JsEncoder.EncoderStream _Encoder = new JsEncoder.EncoderStream();
        private JsEncoder.DecoderStream _Decoder = new JsEncoder.DecoderStream();

        private void InputBytes(System.Byte[] input) { Socket.Client.Send(input); }
        private System.Byte[] ReceiveOutputBytes()
        {
            int NumberOfBytes = Socket.Available;
            byte[] EncodedBytes = new byte[NumberOfBytes];
            if (NumberOfBytes > 0)
                Socket.Client.Receive(EncodedBytes);
            return EncodedBytes;
        }
    }
    public struct PeerMessageDigested
    {
        public JsEncoder.TableValue Table;
        
        public String Header;
        public JsEncoder.ValueBase contents1V;
        public JsEncoder.ValueBase contents2V;
        public String contents1S;
        public String contents2S;

        public PeerMessageDigested(JsEncoder.TableValue Table)
        {
            this.Table = Table;
            JsEncoder.StringValue headerV = (JsEncoder.StringValue)Table.Get(1);
            Header = headerV.GetValue();

            // First two values
            contents1V = Table.Get(2);
            contents2V = Table.Get(3);

            // Convert them to Strings
            JsEncoder.StringValue contents1SV = contents1V as JsEncoder.StringValue;
            JsEncoder.StringValue contents2SV = contents2V as JsEncoder.StringValue;
            contents1S = contents1SV?.GetValue();
            contents2S = contents2SV?.GetValue();
        }
    }
    public class PeerConnection : IDisposable
    {
        // Creation and Destruction
        public PeerConnection(string ThisPeerID) { _ThisPeerID = ThisPeerID; }
        public PeerConnection(string ThisPeerID, TcpClient Socket) : this(ThisPeerID) { BeginConnect(Socket); }
        public void Dispose()
        {
            if (!_Disposed)
            {
                DropConnection();
                _Disposed = true;
            }
        }
        public bool IsDisposed { get { return _Disposed; } }

        private bool _Disposed = false;
        private void AssertNotDisposed()
        {
            if (_Disposed)
                throw new ObjectDisposedException("PeerConnection");
        }

        // Properties
        public string ThisPeerID { get { return _ThisPeerID; } }
        public string OtherPeerID { get { return _OtherPeerID; } }
        public String OtherPeerDisplayName { get { return (_OtherPeerID != null ? _OtherPeerID : "Unknown"); } }
        public TcpClient Socket { get { return _Socket; } }
        public bool OwnsSocket = false; // The socket will also be disposed when disposing this object while this is true.
        public bool PrintPeerMessages = true; // The messages that the other peer sends will be printed out
        public bool AutoSendGreeting = true; // If false, then SendGreeting must be called manually before the connection can complete.
        public int ConnectionStatus { get { return _ConnectionStatus; } }
        public bool IsConnected { get { return (_ConnectionStatus == 1); } }
        public bool GreetingSent { get { return _GreetingSent; } }
        public bool GreetingReceived { get { return _GreetingReceived; } }

        // Events
        public event OnHumanLogOutputHandler OnHumanLogOutput;
        public event OnConnectionStatusChangedHandler OnConnectionStatusChanged;

        // Message Loop
        public void RunCycle(float DeltaTime)
        {
            AssertNotDisposed();

#if !EXPOSE_ERRORS
            try
#endif
            {
                // ## BEGIN ConnectionStateSpecific
                switch (_ConnectionStatus)
                {
                    case 1: // Connected

                        // Send a HeartBeat
                        _HeartBeatSendTimer += DeltaTime;
                        if (_HeartBeatSendTimer > NetworkConfig.HeartBeatInterval)
                        {
                            SendMessage("HEARTBEAT", "");
                            _HeartBeatSendTimer = 0;
                        }

                        // HeartBeat listening timer. "How long ago did I last hear you?"
                        _HeartBeatTimeout += DeltaTime;
                        if (_HeartBeatTimeout > NetworkConfig.HeartBeatSilenceTimeout)
                        {
                            _HeartBeatTimeout = 0;
                            DropConnection();
                            LogSystemHumanMessage("The HeartBeat timeout was exceeded! The connection was dropped.");
                        }
                        break;
                    case 2: // Connecting
                        if (AutoSendGreeting && !_GreetingSent)
                            SendGreeting();
                        else if (_GreetingSent & _GreetingReceived)
                            ChangeConnectionStatusValue(1);

                        _ConnectionTimeout += DeltaTime;
                        if (_ConnectionTimeout > NetworkConfig.ConnectionTimeout)
                        {
                            _ConnectionTimeout = 0;
                            DropConnection();
                            LogSystemHumanMessage("The connection timeout was exceeded! The connection was dropped.");
                        }

                        break;
                    case 3: // Disconnecting
                        _DisconnectTimer += DeltaTime;
                        if (_DisconnectTimer > NetworkConfig.DisconnectWait)
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
                if (_Socket != null && _Socket.Connected)
                {
                    // ## BEGIN Receive

                    // Check on what was received.
                    JsEncoder.ValueBase[] output = _SocketEncoder.ReceiveOutput();
                    foreach (JsEncoder.ValueBase item in output)
                    {
                        // Debug
                        //LogSystemHumanMessage("RECEIVED: " + JsEncoder.EncoderStream.EncodeValue(item));

                        // Essential Information
                        JsEncoder.TableValue outputT = (JsEncoder.TableValue)item;
                        PeerMessageDigested m = new PeerMessageDigested(outputT);
                        string mh = m.Header;

                        if (mh == "GREETING")
                        {
                            if (_ConnectionStatus == 2)
                            {
                                bool VersionMatches = (m.contents1S == (string)NetworkConfig.VersionString);
                                string PeerID = m.contents2S;
                                _OtherPeerID = PeerID;
                                _GreetingReceived = true;
                                if (VersionMatches)
                                    LogSystemHumanMessage("The other peer greeted you.");
                                else
                                    LogSystemHumanMessage("The other peer sent you a greeting with an unmatching version string. Incompatibilities may occur.");
                                if (_GreetingSent)
                                    ChangeConnectionStatusValue(1);
                            }
                        }
                        else if (mh == "DISCONNECTING")
                        {
                            if (_ConnectionStatus == 1)
                                BeginDisconnect(0, false);
                            break;
                        }
                        else if (mh == "HEARTBEAT")
                            _HeartBeatTimeout = 0;
                        else if (mh == "HUMANMESSAGE" && PrintPeerMessages)
                            LogHumanMessage(string.Format("({0}) {1}", OtherPeerDisplayName, m.contents1S));

                        OutputMessage(new PeerMessageDigested(outputT));
                    }

                    // ## END Receive

                    // ## BEGIN Send

                    // Debug
                    //foreach (JsEncoder.ValueBase item in _MessageQueue)
                    //    LogSystemHumanMessage("SENT: " + JsEncoder.EncoderStream.EncodeValue(item));

                    // Send the pending messages
                    // Don't send anything on 3 because we want to stop communications on that stage.
                    if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
                        _SocketEncoder.SendInput(_MessageQueue.ToArray());
                    _MessageQueue.Clear();

                    // ## END Send
                }
            }
#if !EXPOSE_ERRORS
            catch (Exception e)
            {
                DropConnection();
                LogSystemHumanMessage(string.Concat("You lost connection due to an error. Error Message: ", e.Message));
            }
#endif
        }

        // Connection Management
        public void BeginConnect(TcpClient Socket)
        {
            AssertNotDisposed();

            if (Socket != null)
            {
                if (_ConnectionStatus == 0)
                {
                    LogSystemHumanMessage("Connecting...");

                    _Socket = Socket;
                    _SocketEncoder = new SocketEncoderAssembly();
                    _SocketEncoder.Socket = Socket;

                    ChangeConnectionStatusValue(2);
                }
                else
                    throw new Exception("Already connected!");
            }
            else
                throw new ArgumentNullException("Socket");
        }
        public void BeginConnect(String HostName, int Port)
        {
            AssertNotDisposed();

            bool r = false;
            TcpClient s = null;
            try
            {
                s = new TcpClient();
                s.Connect(HostName, Port);
                BeginConnect(s);

                r = true;
            }
#if !EXPOSE_ERRORS
            catch (Exception)
            {
                DropConnection();
                LogSystemHumanMessage("Error: Could not connect!");
            }
#endif
            finally
            {
                if (!r & (s != null))
                {
                    s.Close();
                    s = null;
                }
            }
        }
        public void BeginDisconnect(int ReasonID, bool WarnOtherPeer) // Begins disconnecting from the other peer.
        {
            AssertNotDisposed();

            if (_ConnectionStatus == 1)
            {
                LogSystemHumanMessage("Disconnecting...");
                if (WarnOtherPeer)
                {
                    JsEncoder.ValueBase[] Param2 = new JsEncoder.ValueBase[1];
                    Param2[0] = new JsEncoder.IntValue(ReasonID);
                    SendMessage("DISCONNECTING", Param2, true);
                }
                ChangeConnectionStatusValue(3);
            }
            else
                throw new Exception("Already disconnected!");
        }
        public void DropConnection() // You might want to use this as a last resort in case of an emergency.
        {
            AssertNotDisposed();

            ChangeConnectionStatusValue(0);
            _MessageQueue.Clear();
            _OtherPeerID = null;
            if (_Socket != null)
            {
                if (OwnsSocket)
                {
                    _Socket.Close();
                    OwnsSocket = false;
                }
                _Socket = null;
                _SocketEncoder.Socket = null;
            }

            _GreetingSent = false;
            _GreetingReceived = false;

            _ConnectionTimeout = 0;
            _DisconnectTimer = 0;
            _HeartBeatSendTimer = 0;
            _HeartBeatTimeout = 0;
        }

        public void SendMessage(string MessageHeader, string Contents)
        {
            string[] NewArray = new string[1];
            NewArray[0] = Contents;
            SendMessage(MessageHeader, NewArray);
        }
        public void SendMessage(string MessageHeader, IEnumerable<string> Contents)
        {
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            foreach (string item in Contents)
                ResponseArray.Add(new JsEncoder.StringValue(item));
            SendMessage(MessageHeader, ResponseArray);
        }
        public void SendMessage(string MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents) { SendMessage(MessageHeader, Contents, false); }
        public void SendMessage(string MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents, bool SendImmediately)
        {
            AssertNotDisposed();

            if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
            {
                List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>() { new JsEncoder.StringValue(MessageHeader) };
                foreach (JsEncoder.ValueBase item in Contents)
                    ResponseArray.Add(item);
                JsEncoder.TableValue ResponseTable = JsEncoder.TableValue.ArrayToTable(ResponseArray.ToArray());

                if (SendImmediately)
                {
                    JsEncoder.ValueBase[] Param1 = new JsEncoder.ValueBase[1];
                    Param1[0] = ResponseTable;
                    _SocketEncoder.SendInput(Param1);
                }
                else
                    _MessageQueue.Add(ResponseTable);
            }
        }
        public void SendGreeting()
        {
            if (_ConnectionStatus == 2)
            {
                if (!_GreetingSent)
                {
                    JsEncoder.ValueBase[] Values = new JsEncoder.ValueBase[2];
                    Values[0] = new JsEncoder.StringValue(NetworkConfig.VersionString);
                    Values[1] = new JsEncoder.StringValue(_ThisPeerID);

                    SendMessage("GREETING", Values);
                    _GreetingSent = true;
                }
                else
                    LogSystemHumanMessage("SendGreeting was called again even though one was sent. Not sending another one.");
            }
            else
                LogSystemHumanMessage("SendGreeting was called when the ConnectionStatus wasn't 2 (Connecting).");
        }
        public void SendHumanMessage(string Contents)
        {
            if (Contents != null & Contents != "")
            {
                LogHumanMessage(string.Format("[{0}] {1}", _ThisPeerID, Contents));
                SendMessage("HUMANMESSAGE", Contents);
            }
        }
        public PeerMessageDigested[] GetMessageOutput()
        {
            AssertNotDisposed();

            PeerMessageDigested[] r = _MessageOutput.ToArray();
            _MessageOutput.Clear();
            return r;
        }

        public override string ToString() { return OtherPeerDisplayName; }

        // The Identities this connection deals with.
        private string _ThisPeerID;
        private string _OtherPeerID = null;

        // Things to manage the connection.
        private TcpClient _Socket;
        private SocketEncoderAssembly _SocketEncoder = new SocketEncoderAssembly();
        private int _ConnectionStatus = 0; // 0 = Disconnected, 1 = Connected, 2 = Connecting, 3 = Disconnecting
        private bool _GreetingSent = false;
        private bool _GreetingReceived = false;
        private List<JsEncoder.ValueBase> _MessageQueue = new List<JsEncoder.ValueBase>();
        private List<PeerMessageDigested> _MessageOutput = new List<PeerMessageDigested>();

        // Timers
        private float _ConnectionTimeout = 0;
        private float _DisconnectTimer = 0;
        private float _HeartBeatSendTimer = 0;
        private float _HeartBeatTimeout = 0;

        private void ChangeConnectionStatusValue(int NewStatus)
        {
            _ConnectionStatus = NewStatus;
            OnConnectionStatusChanged?.Invoke(this, NewStatus);
        }
        private void OutputMessage(PeerMessageDigested Message) { _MessageOutput.Add(Message); }
        private void LogHumanMessage(string Line) { OnHumanLogOutput?.Invoke(this, Line); }
        private void LogSystemHumanMessage(string Line) { LogHumanMessage(string.Concat("[Connection] ", Line)); }

        // For TcpListener Handling.
        public static PeerConnection AcceptConnectionFromTcpListener(string ThisPeerID, TcpListener Listener)
        {
            PeerConnection r = null;
            if (Listener.Pending())
            {
                TcpClient ConnectedSocket = Listener.AcceptTcpClient();
                ConnectedSocket.ReceiveTimeout = 5;
                ConnectedSocket.SendTimeout = 5;
                r = new PeerConnection(ThisPeerID, ConnectedSocket);
                r.OwnsSocket = true;
            }
            return r;
        }
        public static List<PeerConnection> AcceptAllConnectionsFromTcpListener(string ThisPeerID, TcpListener Listener)
        {
            List<PeerConnection> r = new List<PeerConnection>();
            while (Listener.Pending())
                AcceptConnectionFromTcpListener(ThisPeerID, Listener);
            return r;
        }
    };
    public class ConnectionRequestListener : IDisposable
    {
        public ConnectionRequestListener(string ID, int Port)
        {
            LocalIdentity = ID;
            _Port = Port;
        }
        public void Dispose()
        {
            Stop();

            foreach (PeerConnection item in _PendingConnections.ToArray())
                item.Dispose();
            _PendingConnections.Clear();
        }

        public string LocalIdentity;
        public bool IsActive { get { return _Active; } }
        public int Port
        {
            get { return _Port; }
            set
            {
                Stop();
                _Port = value;
            }
        }

        public void RunCycle(float DeltaTime)
        {
            foreach (PeerConnection item in _PendingConnections.ToArray())
            {
                item.RunCycle(DeltaTime);

                // Clean up the connections that finished being rejected or was lost.
                if (item.ConnectionStatus == 0)
                {
                    _PendingConnections.Remove(item);
                    item.Dispose();
                }
            }
            if (_Active)
            {
                while (_Listener.Pending())
                {
                    PeerConnection NewConnection = PeerConnection.AcceptConnectionFromTcpListener(LocalIdentity, _Listener);
                    NewConnection.AutoSendGreeting = false; // Specifically for enabling user request scrutiny.
                    _PendingConnections.Add(NewConnection);
                }
            }
        }
        public void Start()
        {
            if (!_Active)
            {
                _Active = true;
                bool r = false;
                try
                {
                    _Listener = new TcpListener(System.Net.IPAddress.Any, _Port);
                    _Listener.Start();
                    r = true;
                }
                finally
                {
                    if (!r)
                        Stop();
                }
            }
        }
        public void Stop()
        {
            if (_Active)
            {
                _Active = false;
                _Listener.Stop();
                _Listener = null;
            }
        }
        public PeerConnection[] GetPendingRequests() { return _PendingConnections.ToArray(); }
        public bool RejectConnection(PeerConnection Connection)
        {
            bool r = _PendingConnections.Remove(Connection);
            if (r)
                Connection.BeginDisconnect(0, true);
            return r;
        }
        public bool AcceptConnection(PeerConnection Connection)
        {
            bool r = _PendingConnections.Remove(Connection);
            if (r)
            {
                Connection.AutoSendGreeting = true;
                Connection.SendGreeting();
            }
            return r;
        }

        private int _Port;

        private TcpListener _Listener;
        private bool _Active = false;
        private List<PeerConnection> _PendingConnections = new List<PeerConnection>();
    }
    public class ChatServer : IDisposable
    {
        private sealed class ClientInfo : IDisposable
        {
            public ChatServer ParentServer;
            public PeerConnection c; // Connection
            public int ID;

            public override string ToString()
            {
                return string.Concat(c.OtherPeerID, " (", ID.ToString(), ")");
            }

            public ClientInfo(ChatServer ParentServer, PeerConnection Socket, int Id)
            {
                this.ParentServer = ParentServer;
                c = Socket;
                ID = Id;
                MessageOutputResponderDelegate = new OnHumanLogOutputHandler(MessageOutputResponder);
                c.OnHumanLogOutput += MessageOutputResponderDelegate;
            }
            public void Dispose()
            {
                if (c != null)
                {
                    c.OnHumanLogOutput -= MessageOutputResponderDelegate;
                    c.Dispose();
                    c = null;
                }
            }

            private OnHumanLogOutputHandler MessageOutputResponderDelegate;
            private void MessageOutputResponder(PeerConnection Sender, string Message)
            {
                ParentServer.OutputMessageLine(string.Concat(this.ToString(), " ", Message));
            }
        }

        public ChatServer(int Port)
        {
            _Port = Port;
            _ListenerSocket = new TcpListener(System.Net.IPAddress.Any, Port);
        }
        public void Dispose()
        {
            Stop();
            foreach (ClientInfo item in _Clients.ToArray())
                item.Dispose();
            _Clients.Clear();
        }

        public int Port { get { return _Port; } }
        public bool IsActive { get { return _Active; } }

        public void Start()
        {
            if (!_Active)
            {
                _Active = true;

                bool r = false;
                try
                {
                    _ListenerSocket.Start();
                    r = true;
                }
                catch (Exception e)
                {
                    OutputMessageLine(string.Concat("Server: The listener socket could not be opened. Error: \"", e.Message, "\""));
                }
                finally
                {
                    if (!r)
                        Stop();
                }
            }
        }
        public void Stop()
        {
            if (_Active)
            {
                _Active = false;
                _ListenerSocket.Stop();
            }
        }

        public string[] GetGuests()
        {
            ClientInfo[] clients = _Clients.ToArray();
            int ClientsLength = clients.Length;
            string[] r = new string[ClientsLength];
            for (int i = 0; i < ClientsLength; i++)
            {
                ClientInfo client = clients[i];
                r[i] = client.c.OtherPeerID;
            }
            return r;
        }
        public string[] GetGuestList()
        {
            ClientInfo[] guests = _Clients.ToArray();
            int guestsL = guests.Length;
            string[] lines = new string[guestsL];
            for (int i = 0; i < guestsL; i++)
            {
                ClientInfo guest = guests[i];
                lines[i] = string.Concat(guest.ToString());
            }
            return lines;
        }
        public void RunCycle(float DeltaTime)
        {
            if (_Active)
            {
                bool HasNewGuests = false;
                while (_ListenerSocket.Pending())
                {
                    PeerConnection NewConnection = PeerConnection.AcceptConnectionFromTcpListener("Old Server", _ListenerSocket);
                    ClientInfo NewClient = new ClientInfo(this, NewConnection, _NextGuestId);
                    _Clients.Add(NewClient);
                    OutputMessageLine(string.Concat("A new guest has connected! It was assigned an ID of ", _NextGuestId, "."));

                    _NextGuestId++;
                    HasNewGuests = true;
                }
                //if (HasNewGuests) BroadcastClientInfo();
            }
            foreach (ClientInfo item in _Clients.ToArray())
                item.c.RunCycle(DeltaTime);
            CheckOnClients();
        }

        public event Action<string> OnLineOutput;

        private int _Port;
        private bool _Active = false;
        private int _NextGuestId = 1;

        private TcpListener _ListenerSocket;
        private List<ClientInfo> _Clients = new List<ClientInfo>();

        private void OutputMessageLine(string Line) { OnLineOutput?.Invoke(Line); }
        private void DropClient(ClientInfo client)
        {
            client.Dispose();
            _Clients.Remove(client);
            //BroadcastClientInfo();
        }
        private void CheckOnClients()
        {
            foreach (ClientInfo client in _Clients.ToArray())
                CheckOnClient(client);
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
                        if (m.Header == "HUMANMESSAGE")
                        {
                            int ClientId = client.ID;
                            string MessageText = m.contents1S;
                            BroadcastMessage("HUMANMESSAGE", string.Format("({0}) {1}", client.ToString(), MessageText));
                        }
                    }
                }
#if !EXPOSE_ERRORS
                catch (Exception e)
                {
                    OutputMessageLine(string.Concat("Error: ", e.Message));
                    DropClient(client);
                }
#endif
            }
            else
            {
                string name = client.ToString();
                DropClient(client);
                OutputMessageLine(string.Concat(name, " has disconnected from the server."));
            }
        }

        private void SendMessage(ClientInfo Client, string MessageHeader, string Contents) { Client.c.SendMessage(MessageHeader, Contents); }
        private void SendMessage(ClientInfo Client, string MessageHeader, IEnumerable<string> Contents) { Client.c.SendMessage(MessageHeader, Contents); }
        private void SendMessage(ClientInfo Client, string MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents) { Client.c.SendMessage(MessageHeader, Contents); }
        private void BroadcastMessage(string MessageHeader, string Contents)
        {
            foreach (ClientInfo item in _Clients.ToArray())
                item.c.SendMessage(MessageHeader, Contents);
        }
        private void BroadcastMessage(string MessageHeader, IEnumerable<string> Contents)
        {
            foreach (ClientInfo item in _Clients.ToArray())
                item.c.SendMessage(MessageHeader, Contents);
        }
        private void BroadcastMessage(string MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents)
        {
            foreach (ClientInfo item in _Clients.ToArray())
                item.c.SendMessage(MessageHeader, Contents);
        }
    }
    public delegate void OnHumanLogOutputHandler(PeerConnection Sender, string Message);
    public delegate void OnConnectionStatusChangedHandler(PeerConnection Sender, int NewStatus);
    public delegate void OnNewConnectionHandler(ConnectionRequestListener Sender, PeerConnection NewConnection);
}
