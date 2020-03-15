/*
    Js Chatterbox Class Library

    Copyright (C) 2018-2020  Justin Law

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

//#define EXPOSE_ERRORS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpClient = System.Net.Sockets.TcpClient;

namespace JsChatterBox
{
    public interface ITextOutputLogger
    {
        string[] Log_CollectOutput();
    }
}
namespace JsChatterBox.Networking
{
    public static class NetworkConfig
    {
        public const int DefaultServerPort = 28760;
        public const String VersionString = "V0.1"; // Protocol Version
        
        public const float HeartBeatInterval = 0.5f;
        public const float HeartBeatSilenceTimeout = 10f;
        public const float ConnectionTimeout = 20f;
        public const float DisconnectWait = 3f;
    }
    public struct PeerMessage
    {
        public JsEncoder.TableValue Table;
        
        public String Header;
        public JsEncoder.IAbstractValue Contents1V;
        public JsEncoder.IAbstractValue Contents2V;
        public String Contents1S;
        public String Contents2S;

        public PeerMessage(JsEncoder.TableValue Table)
        {
            this.Table = Table;
            JsEncoder.StringValue headerV = (JsEncoder.StringValue)Table[1];
            Header = headerV.Value;

            // First two values
            Contents1V = Table[2];
            Contents2V = Table[3];

            // Convert them to Strings
            JsEncoder.StringValue? contents1SV = Contents1V as JsEncoder.StringValue?;
            JsEncoder.StringValue? contents2SV = Contents2V as JsEncoder.StringValue?;
            Contents1S = contents1SV?.Value;
            Contents2S = contents2SV?.Value;
        }
    }
    public class PeerConnection : IDisposable, ITextOutputLogger
    {
        // Properties
        public string ThisPeerID { get { return _ThisPeerID; } }
        public string OtherPeerID { get { return _OtherPeerID; } }
        public String OtherPeerDisplayName { get { return (_OtherPeerID ?? "Unknown"); } }
        public TcpClient Socket { get { return _Socket; } }
        public int ConnectionStatus { get { return _ConnectionStatus; } }
        public bool IsConnected { get { return _ConnectionStatus == 1; } }
        public bool GreetingSent { get { return _GreetingSent; } }
        public bool GreetingReceived { get { return _GreetingReceived; } }
        public bool OwnsSocket = false; // The socket will also be disposed of when disposing this object while this is true.
        public bool AutoSendGreeting = true; // If false, then SendGreeting must be called manually before the connection can complete.

        // Connection Management
        public void BeginConnect(TcpClient Socket)
        {
            AssertNotDisposed();

            if (Socket != null)
            {
                if (_ConnectionStatus == 0)
                {
                    Log_Write_System("Connecting...");
                    _Socket = Socket;
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
                OwnsSocket = true;
                r = true;
            }
#if !EXPOSE_ERRORS
            catch (Exception)
            {
                DropConnection();
                Log_Write_System("Error: Could not connect!");
            }
#endif
            finally
            {
                if (!r & s != null)
                {
                    s.Close();
                    s = null;
                }
            }
        }
        public void BeginDisconnect(bool WarnOtherPeer = true) // Begins disconnecting from the other peer.
        {
            AssertNotDisposed();

            if (_ConnectionStatus == 1)
            {
                Log_Write_System("Disconnecting...");
                if (WarnOtherPeer)
                {
                    JsEncoder.IAbstractValue[] Param2 = new JsEncoder.IAbstractValue[1];
                    Param2[0] = new JsEncoder.IntValue(0);
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
            _MessageOutbox.Clear();
            _OtherPeerID = null;
            if (_Socket != null && OwnsSocket)
            {
                _Socket.Close();
                OwnsSocket = false;
            }
            _Socket = null;

            _GreetingSent = false;
            _GreetingReceived = false;

            _ConnectionTimeout = 0;
            _DisconnectTimer = 0;
            _HeartBeatSendTimer = 0;
            _HeartBeatTimeout = 0;
        }

        public void SendMessage(string MessageHeader, string Contents)
        {
            JsEncoder.IAbstractValue[] NewArray = new JsEncoder.IAbstractValue[1];
            NewArray[0] = new JsEncoder.StringValue(Contents);
            SendMessage(MessageHeader, NewArray);
        }
        public void SendMessage(string MessageHeader, IEnumerable<JsEncoder.IAbstractValue> Contents) { SendMessage(MessageHeader, Contents, false); }
        public void SendMessage(string MessageHeader, IEnumerable<JsEncoder.IAbstractValue> Contents, bool SendImmediately)
        {
            AssertNotDisposed();

            if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
            {
                List<JsEncoder.IAbstractValue> ResponseArray = new List<JsEncoder.IAbstractValue>() { new JsEncoder.StringValue(MessageHeader) };
                foreach (JsEncoder.IAbstractValue item in Contents)
                    ResponseArray.Add(item);
                JsEncoder.TableValue ResponseTable = JsEncoder.TableValue.ArrayToTable(ResponseArray.ToArray());

                if (SendImmediately)
                {
                    JsEncoder.IAbstractValue[] Param1 = new JsEncoder.IAbstractValue[1];
                    Param1[0] = ResponseTable;
                    Encoding_SendInput(Param1);
                }
                else
                    _MessageOutbox.Add(ResponseTable);
            }
        }
        public void SendGreeting()
        {
            if (_ConnectionStatus == 2 & !_GreetingSent)
            {
                JsEncoder.IAbstractValue[] Values = new JsEncoder.IAbstractValue[2];
                Values[0] = new JsEncoder.StringValue(NetworkConfig.VersionString);
                Values[1] = new JsEncoder.StringValue(_ThisPeerID);

                SendMessage("GREETING", Values);
                _GreetingSent = true;
            }
            else
                Log_Write_System("SendGreeting was called even though it's not necessary.");
        }
        public void SendHumanMessage(string Contents)
        {
            if (!string.IsNullOrEmpty(Contents))
            {
                SendMessage("HUMANMESSAGE", Contents);
                Log_Write(string.Concat("[You] ", Contents));
            }
        }
        public PeerMessage[] CollectInboxMessages()
        {
            AssertNotDisposed();

            PeerMessage[] r = _MessageInbox.ToArray();
            _MessageInbox.Clear();
            return r;
        }

        // Other Things
        public void RunCycle(float DeltaTime)
        {
            AssertNotDisposed();

#if !EXPOSE_ERRORS
            try
#endif
            {
                // Connection-State Specific Operations
                switch (_ConnectionStatus)
                {
                    case 1: // Connected

                        // Send HeartBeats
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
                            Log_Write_System("The HeartBeat timeout was exceeded! The connection was dropped.");
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
                            Log_Write_System("The connection timeout was exceeded! The connection was dropped.");
                        }

                        break;
                    case 3: // Disconnecting
                        _DisconnectTimer += DeltaTime;
                        if (_DisconnectTimer > NetworkConfig.DisconnectWait)
                        {
                            _DisconnectTimer = 0;
                            DropConnection();
                            Log_Write_System("Disconnected!");
                        }
                        break;
                    default:
                        break;
                }

                // Mailing Procedure
                if (_Socket != null && _Socket.Connected)
                {
                    // ## BEGIN Receive

                    // Check on what was received.
                    JsEncoder.IAbstractValue[] output = Encoding_CollectOutput();
                    foreach (JsEncoder.IAbstractValue item in output)
                    {
                        // Debug
                        //LogSystemHumanMessage("RECEIVED: " + JsEncoder.EncoderStream.EncodeValue(item));

                        // Essential Information
                        JsEncoder.TableValue outputT = (JsEncoder.TableValue)item;
                        PeerMessage m = new PeerMessage(outputT);
                        string mh = m.Header;

                        if (mh == "GREETING")
                        {
                            if (_ConnectionStatus == 2)
                            {
                                bool VersionMatches = (m.Contents1S == (string)NetworkConfig.VersionString);
                                string PeerID = m.Contents2S;
                                _OtherPeerID = PeerID;
                                _GreetingReceived = true;
                                if (VersionMatches)
                                    Log_Write_System("The other peer greeted you.");
                                else
                                    Log_Write_System("The other peer sent you a greeting with an unmatching version string. Incompatibilities may occur.");
                                if (_GreetingSent)
                                    ChangeConnectionStatusValue(1);
                            }
                        }
                        else if (mh == "DISCONNECTING")
                        {
                            if (_ConnectionStatus == 1)
                                BeginDisconnect(false);
                            break;
                        }
                        else if (mh == "HEARTBEAT")
                            _HeartBeatTimeout = 0;
                        else if (mh == "HUMANMESSAGE")
                            Log_Write(string.Format("({0}) {1}", OtherPeerDisplayName, m.Contents1S));

                        OutputMessage(new PeerMessage(outputT));
                    }

                    // ## END Receive

                    // ## BEGIN Send

                    // Debug
                    //foreach (JsEncoder.ValueBase item in _MessageQueue)
                    //    LogSystemHumanMessage("SENT: " + JsEncoder.EncoderStream.EncodeValue(item));

                    // Send the pending messages
                    // Don't send anything on 3 because we want to stop communications on that stage.
                    if (_ConnectionStatus == 1 || _ConnectionStatus == 2)
                        Encoding_SendInput(_MessageOutbox.ToArray());
                    _MessageOutbox.Clear();

                    // ## END Send
                }
            }
#if !EXPOSE_ERRORS
            catch (Exception e)
            {
                DropConnection();
                Log_Write_System(string.Concat("You lost connection due to an error. Error Message: ", e.Message));
            }
#endif
        }
        public string[] Log_CollectOutput()
        {
            string[] r = _Log_PendingOutput.ToArray();
            _Log_PendingOutput.Clear();
            return r;
        }
        public override string ToString() { return OtherPeerDisplayName; }

        // Creation and Destruction
        public PeerConnection(string ThisPeerID) { _ThisPeerID = ThisPeerID; }
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
        
        // Things to manage the connection.
        private TcpClient _Socket;
        private JsEncoder.EncoderStream _Encoder = new JsEncoder.EncoderStream();
        private JsEncoder.DecoderStream _Decoder = new JsEncoder.DecoderStream();
        private System.Text.Encoding _EncodingFormat = System.Text.Encoding.Unicode;
        private string _ThisPeerID = null;
        private string _OtherPeerID = null;
        private List<JsEncoder.IAbstractValue> _MessageOutbox = new List<JsEncoder.IAbstractValue>();
        private List<PeerMessage> _MessageInbox = new List<PeerMessage>();
        private List<string> _Log_PendingOutput = new List<string>();
        private int _ConnectionStatus = 0; // 0 = Disconnected, 1 = Connected, 2 = Connecting, 3 = Disconnecting
        private bool _GreetingSent = false;
        private bool _GreetingReceived = false;
        private float _ConnectionTimeout = 0;
        private float _DisconnectTimer = 0;
        private float _HeartBeatSendTimer = 0;
        private float _HeartBeatTimeout = 0;

        private void ChangeConnectionStatusValue(int NewStatus) { _ConnectionStatus = NewStatus; }
        private void OutputMessage(PeerMessage Message) { _MessageInbox.Add(Message); }
        private void Log_Write(string Line) { _Log_PendingOutput.Add(Line); }
        private void Log_Write_System(string Line) { Log_Write(string.Concat("[Connection] ", Line)); }
        private void Encoding_SendInput(IEnumerable<JsEncoder.IAbstractValue> Data)
        {
            // Encode Data
            foreach (JsEncoder.IAbstractValue item in Data)
                _Encoder.InputValue(item);
            string EncodedString = _Encoder.PopOutput();
            char[] EncodedChars = EncodedString.ToCharArray();
            byte[] EncodedBytes = _EncodingFormat.GetBytes(EncodedChars);
            _Socket.Client.Send(EncodedBytes);
        }
        private JsEncoder.IAbstractValue[] Encoding_CollectOutput()
        {
            // Receive Bytes
            int NumberOfBytes = _Socket.Available;
            byte[] EncodedBytes = new byte[NumberOfBytes];
            if (NumberOfBytes > 0)
                _Socket.Client.Receive(EncodedBytes);

            // Decode Data
            char[] EncodedChars = _EncodingFormat.GetChars(EncodedBytes);
            string EncodedString = new string(EncodedChars);
            _Decoder.InputValue(EncodedString);
            _Decoder.RunParser();
            return _Decoder.PopOutput();
        }

        // For TcpListener Handling.
        public static PeerConnection AcceptConnectionFromTcpListener(string ThisPeerID, TcpListener Listener)
        {
            PeerConnection r = null;
            if (Listener.Pending())
            {
                TcpClient ConnectedSocket = Listener.AcceptTcpClient();
                ConnectedSocket.ReceiveTimeout = 5;
                ConnectedSocket.SendTimeout = 5;
                r = new PeerConnection(ThisPeerID);
                r.BeginConnect(ConnectedSocket);
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
    }
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
                while (_Listener.Pending())
                {
                    PeerConnection NewConnection = PeerConnection.AcceptConnectionFromTcpListener(LocalIdentity, _Listener);
                    NewConnection.AutoSendGreeting = false; // Specifically for enabling user request scrutiny.
                    _PendingConnections.Add(NewConnection);
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
                Connection.BeginDisconnect(true);
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
    public class ChatServer : IDisposable, ITextOutputLogger
    {
        private sealed class ClientInfo : IDisposable
        {
            public ChatServer ParentServer;
            public PeerConnection C; // Connection
            public int ID;

            public override string ToString()
            {
                return string.Concat(C.OtherPeerID, " (", ID.ToString(), ")");
            }

            public ClientInfo(ChatServer ParentServer, PeerConnection Connection, int Id)
            {
                this.ParentServer = ParentServer;
                this.C = Connection;
                this.ID = Id;
            }
            public void Dispose()
            {
                if (C != null)
                {
                    C.Dispose();
                    C = null;
                }
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
                    Log_Write_System(string.Concat("The listener socket could not be opened. Error: \"", e.Message, "\""));
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
                r[i] = client.C.OtherPeerID;
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
                while (_ListenerSocket.Pending())
                {
                    PeerConnection NewConnection = PeerConnection.AcceptConnectionFromTcpListener("Some Guy's Server", _ListenerSocket);
                    ClientInfo NewClient = new ClientInfo(this, NewConnection, _NextGuestId);
                    _Clients.Add(NewClient);
                    Log_Write_System(string.Format("A new guest has connected! It was assigned an ID of {0}.", _NextGuestId));

                    _NextGuestId++;
                }
            }
            foreach (ClientInfo item in _Clients.ToArray())
            {
                item.C.RunCycle(DeltaTime);
                foreach (string m in item.C.Log_CollectOutput())
                    Log_Write(m);
            }
            CheckOnClients();
        }
        public string[] Log_CollectOutput()
        {
            string[] r = _Log_PendingOutput.ToArray();
            _Log_PendingOutput.Clear();
            return r;
        }

        private int _Port;
        private bool _Active = false;
        private int _NextGuestId = 1;

        private TcpListener _ListenerSocket;
        private List<ClientInfo> _Clients = new List<ClientInfo>();
        private List<string> _Log_PendingOutput = new List<string>();

        private void Log_Write(string Text) { _Log_PendingOutput.Add(Text); }
        private void Log_Write_System(string Text) { Log_Write(string.Concat("[Server] ", Text)); }
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
            if (client.C.ConnectionStatus != 0)
            {
#if !EXPOSE_ERRORS
                try
#endif
                {
                    // Receive a command and send an answer.
                    PeerMessage[] output = client.C.CollectInboxMessages();
                    foreach (PeerMessage m in output)
                    {
                        if (m.Header == "HUMANMESSAGE")
                        {
                            int ClientId = client.ID;
                            string MessageText = m.Contents1S;
                            BroadcastMessage("HUMANMESSAGE", string.Format("({0}) {1}", client.ToString(), MessageText));
                        }
                    }
                }
#if !EXPOSE_ERRORS
                catch (Exception e)
                {
                    Log_Write_System(string.Concat("Error: ", e.Message));
                    DropClient(client);
                }
#endif
            }
            else
            {
                string name = client.ToString();
                DropClient(client);
                Log_Write_System(string.Format("{0} has disconnected from the server.", name));
            }
        }

        private void SendMessage(ClientInfo Client, string MessageHeader, string Contents) { Client.C.SendMessage(MessageHeader, Contents); }
        private void SendMessage(ClientInfo Client, string MessageHeader, IEnumerable<JsEncoder.IAbstractValue> Contents) { Client.C.SendMessage(MessageHeader, Contents); }
        private void BroadcastMessage(string MessageHeader, string Contents)
        {
            foreach (ClientInfo item in _Clients.ToArray())
                item.C.SendMessage(MessageHeader, Contents);
        }
        private void BroadcastMessage(string MessageHeader, IEnumerable<JsEncoder.IAbstractValue> Contents)
        {
            foreach (ClientInfo item in _Clients.ToArray())
                item.C.SendMessage(MessageHeader, Contents);
        }
    }
}
