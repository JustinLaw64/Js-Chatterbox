//#define EXPOSE_ERRORS

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using Text = System.Text;

namespace JsChatterBox
{
    public class ChatClient : IDisposable
    {
        public bool IsConnected { get { return _ClientSocket != null; } }
        public String ClientName { get { return _ClientName; } }
        public String ConnectedHostName { get { return _ConnectedHostName; } }

        public void Connect(String HostName) { Connect(HostName, true); }
        public void Connect(String HostName, bool OutputMessages)
        {
            if (_ClientSocket == null)
            {
                try
                {
                    if (OutputMessages) OutputMessageLine("Connecting...");
                    _ClientSocket = new TcpClient();
                    _ClientSocket.Connect(HostName, Program.DefaultServerPort);
                    _SocketAdapter.Socket = _ClientSocket.Client;
                    if (OutputMessages) OutputMessageLine("Connected!");
                    _ConnectedHostName = HostName;

                }
                catch (Exception)
                {
                    Disconnect(false);
                    OutputMessageLine("Error: Could not connect!");
                    ///throw;
                }
            }
            else
                throw new Exception("Already connected!");
        }
        public void Disconnect() { Disconnect(true); }
        public void Disconnect(bool OutputMessages)
        {
            if (_ClientSocket != null)
            {
                if (OutputMessages) OutputMessageLine("Disconnecting...");
                if (_ClientSocket.Connected)
                    SendMessage("DISCONNECTING", "");
                _SocketAdapter.Socket = null;
                _ClientSocket.Close();
                _ClientSocket = null;
                _GuestList.Clear();
                if (OutputMessages) OutputMessageLine("Disconnected!");
                _ConnectedHostName = null;
            }
        }
        public void SendHumanMessage(String Contents)
        {
            if (Contents != null & Contents != "")
                SendMessage("MESSAGE", "", Contents);
        }
        public void ChangeName(String NewName)
        {
            if (NewName != null && NewName != "")
            {
                _ClientName = NewName;
                if (IsConnected)
                    SendMessage("NAMECHANGE", NewName);
                OutputMessageLine(String.Concat("Client: Your name has been changed to ", NewName, "."));
            }
            else
                OutputMessageLine(String.Concat("Client: Your name can't be blanked out!"));
        }

        public void RunCycle() { CheckOnConnection(); }

        public GuestInfo[] GetGuestList() { return _GuestList.ToArray(); }
        public GuestInfo? GetGuest(int ID) 
        {
            foreach (GuestInfo item in GetGuestList())
                if (item.GuestId == ID)
                    return item;
            return null;
        }

        public event Action<String> OnLineOutput;

        public ChatClient() { _SocketAdapter = new SocketEncoderAssembly(); }
        public void Dispose()
        {
            Disconnect();
            //Stop();
            //item.Dispose();
        }

        private String _ClientName = "Unnamed";

        private TcpClient _ClientSocket;
        private String _ConnectedHostName = null;
        private List<GuestInfo> _GuestList = new List<GuestInfo>();
        private SocketEncoderAssembly _SocketAdapter;
        private List<JsEncoder.ValueBase> _MessageQueue = new List<JsEncoder.ValueBase>();

        private void ReportDisconnection()
        {
            OutputMessageLine("You've been disconnected!");
            Disconnect(false);
        }
        private void CheckOnConnection()
        {
            if (_ClientSocket != null)
            {
#if !EXPOSE_ERRORS
                try
#endif
                {
                    if (_ClientSocket.Connected)
                    {
                        // Check on what was received.
                        JsEncoder.ValueBase[] output = _SocketAdapter.ReceiveOutput();
                        foreach (JsEncoder.ValueBase item in output)
                        {
                            JsEncoder.TableValue outputT = (JsEncoder.TableValue)item;
                            JsEncoder.StringValue headerV = (JsEncoder.StringValue)outputT.Get(1);
                            JsEncoder.ValueBase contents1V = outputT.Get(2);
                            JsEncoder.ValueBase contents2V = outputT.Get(3);
                            JsEncoder.StringValue contents1SV = (contents1V as JsEncoder.StringValue);
                            JsEncoder.StringValue contents2SV = (contents2V as JsEncoder.StringValue);
                            String header = headerV.Value;
                            String contents1S = contents1SV != null ? contents1SV.Value : null;
                            String contents2S = contents2SV != null ? contents2SV.Value : null;

                            switch (header)
                            {
                                case "GREETING":
                                    OutputMessageLine("The server greeted you. Greeting back.");
                                    SendMessage("GREETING", Program.VersionString, _ClientName);
                                    break;
                                case "ERROR":
                                    OutputMessageLine(String.Concat("Server Error: ", contents1S));
                                    break;
                                case "MESSAGE":
                                    int GuestID = ((JsEncoder.IntValue)contents1V).Value;
                                    GuestInfo? SendingGuest = GetGuest(GuestID);
                                    String GuestName = "";
                                    if (SendingGuest.HasValue)
                                        GuestName = SendingGuest.Value.ToString();
                                    else
                                        GuestName = String.Concat("Unknown guest with ID ", GuestID.ToString());
                                    OutputMessageLine(String.Concat(GuestName, ": ", contents2S));
                                    break;
                                case "GUESTINFOLIST":
                                    JsEncoder.TableValue table = (JsEncoder.TableValue)contents1V;
                                    _GuestList.Clear();
                                    foreach (var pair in table.Dictionary)
                                        _GuestList.Add(new GuestInfo(((JsEncoder.IntValue)pair.Key).Value, ((JsEncoder.StringValue)pair.Value).Value));
                                    break;
                                case "HEARTBEAT":

                                    break;
                                default:
                                    break;
                            }
                        }

                        // Send the pending messages
                        if (IsConnected)
                        {
                            SendMessage("HEARTBEAT", "");

                            _SocketAdapter.InputData(_MessageQueue.ToArray());
                            _MessageQueue.Clear();
                        }
                    }
                    else
                        ReportDisconnection();
                }
#if !EXPOSE_ERRORS
                catch (Exception e)
                {
                    OutputMessageLine(String.Concat("Client Error: ", e.Message));
                    ReportDisconnection();
                }
#endif
            }
            else
            {
                _MessageQueue.Clear();
            }
        }
        private void OutputMessageLine(String Line)
        {
            if (OnLineOutput != null)
                OnLineOutput(Line);
        }

        private void SendMessage(String MessageHeader, String Contents) { SendMessage(MessageHeader, new String[] { Contents }); }
        private void SendMessage(String MessageHeader, String Contents1, String Contents2) { SendMessage(MessageHeader, new String[] { Contents1, Contents2 }); }
        private void SendMessage(String MessageHeader, IEnumerable<String> Contents)
        {
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            foreach (String item in Contents)
                ResponseArray.Add(new JsEncoder.StringValue(item));
            SendMessage(MessageHeader, ResponseArray);
        }
        private void SendMessage(String MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents)
        {
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            ResponseArray.Add(new JsEncoder.StringValue(MessageHeader));
            foreach (var item in Contents)
                ResponseArray.Add(item);

            JsEncoder.TableValue ResponseTable = JsEncoder.TableValue.ArrayToTable(ResponseArray.ToArray());
            _MessageQueue.Add(ResponseTable);
        }
    }
    public class ChatServer : IDisposable
    {
        public GuestInfo[] GetGuests()
        {
            ClientInfo[] clients = Clients.ToArray();
            int ClientsLength = clients.Length;
            GuestInfo[] r = new GuestInfo[ClientsLength];
            for (int i = 0; i < ClientsLength; i++)
            {
                r[i] = clients[i].Info;
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
        public void RunCycle()
        {
            AcceptAll();
            CheckOnClients();
        }

        public event Action<String> OnLineOutput;

        public ChatServer() : this(Program.DefaultServerPort) { }
        public ChatServer(int Port)
        {
            _ListenerSocket = new TcpListener(Port);
            _ListenerSocket.Start();
        }
        public void Dispose()
        {
            _ListenerSocket.Stop();
            foreach (var item in Clients)
                item.Dispose();
        }

        private int NextGuestId = 1;

        private TcpListener _ListenerSocket;
        private List<ClientInfo> Clients = new List<ClientInfo>();

        private void OutputMessageLine(String Line)
        {
            if (OnLineOutput != null)
                OnLineOutput(Line);
        }
        private void AcceptAll()
        {
            bool HasNewGuests = false;
            while (_ListenerSocket.Pending())
            {
                Socket ConnectedSocket = _ListenerSocket.AcceptSocket();
                ConnectedSocket.ReceiveTimeout = 1;
                ConnectedSocket.SendTimeout = 1;
                ClientInfo NewClient = new ClientInfo(ConnectedSocket, NextGuestId, new GuestInfo(NextGuestId, "Unknown"));
                Clients.Add(NewClient);
                SendGreeting(NewClient);
                OutputMessageLine(String.Concat("A new guest has connected! It was assigned an ID of ", NextGuestId, " and was sent a greeting."));

                NextGuestId++;
                HasNewGuests = true;
            }
            if (HasNewGuests) BroadcastClientInfo();
        }
        private void DropClient(ClientInfo client)
        {
            if (client.IsConnected)
            {
                client.Socket.Disconnect(false);
            }
            client.Dispose();
            Clients.Remove(client);
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
#if !EXPOSE_ERRORS
            try
#endif
            {
                if (client.IsConnected)
                {
                    if (!client.WasGreetingSent)
                        SendGreeting(client);

                    // Receive a command and send an answer.
                    JsEncoder.ValueBase[] output = client.Adapter.ReceiveOutput();
                    foreach (JsEncoder.ValueBase item in output)
                    {
                        JsEncoder.TableValue outputT = (JsEncoder.TableValue)item;
                        JsEncoder.StringValue headerV = (JsEncoder.StringValue)outputT.Get(1);
                        JsEncoder.ValueBase contents1V = outputT.Get(2);
                        JsEncoder.ValueBase contents2V = outputT.Get(3);
                        JsEncoder.StringValue contents1SV = (contents1V as JsEncoder.StringValue);
                        JsEncoder.StringValue contents2SV = (contents2V as JsEncoder.StringValue);
                        String header = headerV.Value;
                        String contents1 = (contents1SV != null ? contents1SV.Value : null);
                        String contents2 = (contents2SV != null ? contents2SV.Value : null);

                        if (client.WasGreetedBy)
                        {
                            switch (header)
                            {
                                case "NAMECHANGE":
                                    string OldName = client.Info.Name;
                                    string OldIdName = client.Info.ToString();
                                    string NewName = contents1;

                                    if (NewName != OldName)
                                    {
                                        OutputMessageLine(String.Concat(OldIdName, " changed his/her name to ", NewName, "."));

                                        client.Info.Name = NewName;
                                        BroadcastClientInfo();
                                    }
                                    break;
                                case "MESSAGE":
                                    int ClientId = client.Id;
                                    String MessageText = contents2;

                                    OutputMessageLine(String.Concat(client.Info.ToString(), " says: ", MessageText));
                                    BroadcastMessage("MESSAGE", new JsEncoder.ValueBase[] { new JsEncoder.IntValue(ClientId), new JsEncoder.StringValue(MessageText) });
                                    break;
                                case "DISCONNECTING":
                                    OutputMessageLine(String.Concat(client.Info.ToString(), " is disconnecting."));
                                    DropClient(client);
                                    break;
                                case "HEARTBEAT":

                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (header == "GREETING")
                        {
                            client.WasGreetedBy = true;
                            client.Info.Name = contents2;
                            BroadcastClientInfo();
                            if (contents1 == Program.VersionString)
                                OutputMessageLine(String.Concat("A proper greeting was received from ", client.Info.ToString(), "!"));
                            else
                                OutputMessageLine(String.Concat("A greeting with the wrong version string was received from ", client.Info.ToString(), ". Compatibility errors may occur!"));
                        }
                    }

                    // Send the server's messages
                    if (client.IsConnected)
                    {
                        SendMessage(client, "HEARTBEAT", "");

                        client.Adapter.InputData(client.MessageQueue.ToArray());
                        client.MessageQueue.Clear();
                    }
                }
                else
                {
                    OutputMessageLine(String.Concat("The client ", client.Info.ToString(), " has disconnected without saying goodbye for some reason."));
                    DropClient(client);
                }
            }
#if !EXPOSE_ERRORS
            catch (Exception e)
            {
                OutputMessageLine(String.Concat("Error: ", e.Message));
            }
#endif
        }
        private void SendGreeting(ClientInfo client)
        {
            SendMessage(client, "GREETING", Program.VersionString);
            client.WasGreetingSent = true;
        }

        private void SendClientInfo(ClientInfo Client)
        {
            JsEncoder.TableValue table = new JsEncoder.TableValue();
            foreach (var client in Clients.ToArray())
	        {
                table.Dictionary.Add(new JsEncoder.IntValue(client.Id), new JsEncoder.StringValue(client.Info.Name));
	        }
            if (Client != null)
                SendMessage(Client, "GUESTINFOLIST", new JsEncoder.ValueBase[]{table});
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
        private void SendMessage(ClientInfo Client, String MessageHeader, IEnumerable<JsEncoder.ValueBase> Contents)
        {
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            ResponseArray.Add(new JsEncoder.StringValue(MessageHeader));
            foreach (var item in Contents)
                ResponseArray.Add(item);

            JsEncoder.TableValue ResponseTable = JsEncoder.TableValue.ArrayToTable(ResponseArray.ToArray());
            Client.MessageQueue.Add(ResponseTable);
        }
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
            List<JsEncoder.ValueBase> ResponseArray = new List<JsEncoder.ValueBase>();
            ResponseArray.Add(new JsEncoder.StringValue(MessageHeader));
            foreach (var item in Contents)
		        ResponseArray.Add(item);

            JsEncoder.TableValue ResponseTable = JsEncoder.TableValue.ArrayToTable(ResponseArray.ToArray());
            foreach (ClientInfo item in Clients.ToArray())
                item.MessageQueue.Add(ResponseTable);
        }

        private class ClientInfo : IDisposable
        {
            public readonly Socket Socket;
            public readonly SocketEncoderAssembly Adapter;
            public int Id;
            public GuestInfo Info;

            public bool IsConnected { get { return Socket.Connected; } }
            public bool WasGreetedBy = false;
            public bool WasGreetingSent = false;
            public List<JsEncoder.ValueBase> MessageQueue = new List<JsEncoder.ValueBase>();

            public override string ToString()
            {
                return String.Concat(Info.ToString(), " (",(WasGreetedBy ? "Has Greeted" : "Hasn't Greeted."),")");
            }

            public ClientInfo(Socket Socket, int Id, GuestInfo Info)
            {
                this.Socket = Socket;
                this.Id = Id;
                this.Info = Info;
                Adapter = new SocketEncoderAssembly(Socket);
            }
            public void Dispose()
            {
                if (Socket != null)
                    Socket.Close();
            }
        }
    }

    public class SocketEncoderAssembly
    {
        public Socket Socket;

        public void InputData(IEnumerable<JsEncoder.ValueBase> Data)
        {
            foreach (JsEncoder.ValueBase item in Data)
                Encoder.InputValue(item);
            String EncodedData = Encoder.PopOutput();
            char[] EncodedChars = EncodedData.ToCharArray();
            byte[] EncodedBytes = TextFormatEncoder.GetBytes(EncodedChars);
            InputBytes(EncodedBytes);
            
            return;
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

        public SocketEncoderAssembly(Socket Socket) { this.Socket = Socket; }
        public SocketEncoderAssembly() : this(null) { }

        private void InputBytes(byte[] input) { Socket.Send(input); }
        private byte[] ReceiveOutputBytes()
        {
            int NumberOfBytes = Socket.Available;
            byte[] EncodedBytes = new byte[NumberOfBytes];
            int BytesReceivedBySocket = 0;
            if (NumberOfBytes > 0)
                BytesReceivedBySocket = Socket.Receive(EncodedBytes);
            return EncodedBytes;
        }

        private Text.Encoding TextFormatEncoder = Text.Encoding.Unicode;

        private JsEncoder.EncoderStream Encoder = new JsEncoder.EncoderStream();
        private JsEncoder.DecoderStream Decoder = new JsEncoder.DecoderStream();
    }

    public struct GuestInfo
    {
        public int GuestId;
        public String Name;
        
        public JsEncoder.TableValue ToTable() { return ToTable(this); }
        public override string ToString() { return String.Concat(Name, " (", GuestId.ToString(), ")"); }

        public GuestInfo(int GuestId, String Name) { this.GuestId = GuestId; this.Name = Name; }

        public static GuestInfo FromTable(JsEncoder.TableValue Table)
        {
            JsEncoder.IntValue ID = (JsEncoder.IntValue)Table.Dictionary[new JsEncoder.IntValue(1)];
            JsEncoder.StringValue Name = (JsEncoder.StringValue)Table.Dictionary[new JsEncoder.IntValue(2)];
            return new GuestInfo(ID.Value, Name.Value);
        }
        public static JsEncoder.TableValue ToTable(GuestInfo Info)
        { 
            JsEncoder.TableValue r = new JsEncoder.TableValue();
            r.Dictionary[new JsEncoder.IntValue(1)] = new JsEncoder.IntValue(Info.GuestId);
            r.Dictionary[new JsEncoder.IntValue(2)] = new JsEncoder.StringValue(Info.Name);
            return r;
        }
    }
}
