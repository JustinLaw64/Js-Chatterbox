using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileIO = System.IO.File;
using DirectoryIO = System.IO.Directory;
using JsChatterBoxNetworking;
using JsChatterBoxNetworking.Implementations;

namespace JsChatterBox
{
    static class Program
    {
        public static PersistentDataManager DataManager = null;

        public static ClientWindow OpenChatConnection(PeerConnection ExistingConnection)
        {
            ClientWindow NewWindow = new ClientWindow(ExistingConnection);
            NewWindow.OwnsConnection = true;
            NewWindow.Show();

            return NewWindow;
        }
        public static ClientWindow OpenChatConnection(String HostName, int Port)
        {
            HostInformation NewInfo = new HostInformation(HostName, Port);
            DataManager.HostList.RecentHosts.Remove(NewInfo);
            DataManager.HostList.RecentHosts.Add(NewInfo);

            ClientWindow NewWindow = new ClientWindow(HostName, Port);
            NewWindow.Show();

            return NewWindow;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DataManager = new PersistentDataManager();
            DataManager.Load();
            RootForm NewWindow = new RootForm();
            Application.Run(NewWindow);
        }
    }

    public class PersistentDataManager
    {
        // This is where the data save folder will be stored.
        public bool IsLoaded { get { return _IsLoaded; } }

        // Data
        public String UserName = null;
        public int WorkingPort = NetworkConfig.DefaultServerPort; // To tailor to the user's specified port.
        public UserHostList HostList = null;

        public void Save()
        {
            if (_IsLoaded)
            {
                DirectoryIO.CreateDirectory(DataSavePath);

                String HostListAsText = JsEncoder.EncoderStream.EncodeTable(UserHostList.ToTable(HostList));
                String ConfigAsText; // This won't be null after the region below

                #region ConfigFileSaving
                {
                    JsEncoder.TableValue ConfigTable = new JsEncoder.TableValue();
                    ConfigTable.Set(new JsEncoder.StringValue("UserName"), new JsEncoder.StringValue(UserName));
                    ConfigTable.Set(new JsEncoder.StringValue("WorkingPort"), new JsEncoder.IntValue(WorkingPort));

                    ConfigAsText = JsEncoder.EncoderStream.EncodeTable(ConfigTable);
                }
                #endregion

                String VersionFileBackupPath = String.Concat(VersionFilePath, ".bak");
                String ConfigFileBackupPath = String.Concat(ConfigFilePath, ".bak");
                String HostListFileBackupPath = String.Concat(HostListFilePath, ".bak");

                if (FileIO.Exists(VersionFileBackupPath)) FileIO.Delete(VersionFileBackupPath);
                if (FileIO.Exists(ConfigFileBackupPath)) FileIO.Delete(ConfigFileBackupPath);
                if (FileIO.Exists(HostListFileBackupPath)) FileIO.Delete(HostListFileBackupPath);

                if (FileIO.Exists(VersionFilePath)) FileIO.Move(VersionFilePath, VersionFileBackupPath);
                if (FileIO.Exists(ConfigFilePath)) FileIO.Move(ConfigFilePath, ConfigFileBackupPath);
                if (FileIO.Exists(HostListFilePath)) FileIO.Move(HostListFilePath, HostListFileBackupPath);

                FileIO.WriteAllText(VersionFilePath, NetworkConfig.VersionString, System.Text.Encoding.Unicode);
                FileIO.WriteAllText(ConfigFilePath, ConfigAsText, System.Text.Encoding.Unicode);
                FileIO.WriteAllText(HostListFilePath, HostListAsText, System.Text.Encoding.Unicode);
            }
        }
        public bool Load()
        {
            bool r = false;

            bool ve = FileIO.Exists(VersionFilePath);
            if (ve)
            {
                String VersionFileContents = FileIO.ReadAllText(VersionFilePath, System.Text.Encoding.Unicode);
                if (VersionFileContents == NetworkConfig.VersionString)
                    r = true;
                else
                {
                    String IncompatPath = String.Concat(DataSavePath, "_", VersionFileContents);
                    if (DirectoryIO.Exists(IncompatPath)) DirectoryIO.Delete(IncompatPath, true);
                    DirectoryIO.Move(DataSavePath, IncompatPath);
                    System.Windows.Forms.MessageBox.Show(String.Concat(
                        "The application tried to load Js ChatterBox files that were incompatible ",
                        "with this version. They have been moved to \"", IncompatPath, "\"."));
                }
            }

            if (r)
            {
                try
                {
                    #region ConfigFileLoading
                    {
                        String ConfigText = FileIO.ReadAllText(ConfigFilePath, System.Text.Encoding.Unicode);
                        JsEncoder.TableValue ConfigTable = (JsEncoder.TableValue)JsEncoder.DecoderStream.DecodeValue(ConfigText);

                        UserName = ((JsEncoder.StringValue)ConfigTable.Get(new JsEncoder.StringValue("UserName"))).GetValue();
                        WorkingPort = ((JsEncoder.IntValue)ConfigTable.Get(new JsEncoder.StringValue("WorkingPort"))).GetValue();
                    }
                    #endregion

                    String HostListFileContents = FileIO.ReadAllText(HostListFilePath, System.Text.Encoding.Unicode);
                    JsEncoder.TableValue HostListTable = (JsEncoder.TableValue)JsEncoder.DecoderStream.DecodeValue(HostListFileContents);
                    HostList = UserHostList.FromTable(HostListTable);
                }
                catch (Exception e)
                {
                    String BaseIncompatPath = String.Concat(DataSavePath, "_Damaged<N>");
                    String FinalIncompatPath = null;
                    int i = 0;
                    while (FinalIncompatPath == null)
                    {
                        i++;
                        FinalIncompatPath = BaseIncompatPath.Replace("<N>", i.ToString());
                        if (DirectoryIO.Exists(FinalIncompatPath))
                            FinalIncompatPath = null;
                    }
                    DirectoryIO.Move(DataSavePath, FinalIncompatPath);

                    r = false;
                    System.Windows.Forms.MessageBox.Show(String.Concat(
                        "An error occured while loading your save files. They have been moved to \"",
                        FinalIncompatPath , "\" and you will now get the defaults. ",
                        "The error message is \"", e.Message, "\"."));
                }
            }
            if (!r)
            {
                UserName = "Unnamed";
                HostList = new UserHostList();
                WorkingPort = NetworkConfig.DefaultServerPort;
            }
            _IsLoaded = true;
            return r;
        }
        public PeerIdentity GetPeerIdentity() { return new PeerIdentity(0, UserName); }

        private bool _IsLoaded = false;

        public static String DataSavePath { get { return ("{0}\\JsChatterBox").Replace("{0}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)); } }
        public static String VersionFilePath { get { return String.Concat(DataSavePath, "\\Version.dat"); } }
        public static String ConfigFilePath { get { return String.Concat(DataSavePath, "\\Config.dat"); } }
        public static String HostListFilePath { get { return String.Concat(DataSavePath, "\\HostList.dat"); } }
    }
    public class UserHostList
    {
        public readonly List<HostInformation> FavoriteHosts = new List<HostInformation>();
        public readonly List<HostInformation> RecentHosts = new List<HostInformation>();

        public static JsEncoder.TableValue ToTable(UserHostList Value)
        {
            JsEncoder.TableValue t = new JsEncoder.TableValue();
            JsEncoder.TableValue ft = new JsEncoder.TableValue();
            JsEncoder.TableValue rt = new JsEncoder.TableValue();
            t.Set(1, ft);
            t.Set(2, rt);
            HostInformation[] fa = Value.FavoriteHosts.ToArray();
            HostInformation[] ra = Value.RecentHosts.ToArray();
            int fl = fa.Length;
            int rl = ra.Length;
            for (int i = 0; i < fl; i++)
            {
                JsEncoder.TableValue item = HostInformation.ToTable(fa[i]);
                ft.Set(i + 1, item);
            }
            for (int i = 0; i < rl; i++)
            {
                JsEncoder.TableValue item = HostInformation.ToTable(ra[i]);
                rt.Set(i + 1, item);
            }

            return t;
        }
        public static UserHostList FromTable(JsEncoder.TableValue Value)
        {
            JsEncoder.TableValue ft = (JsEncoder.TableValue)Value.Get(1);
            JsEncoder.TableValue rt = (JsEncoder.TableValue)Value.Get(2);
            JsEncoder.TableDigested ftd = ft.DigestTable();
            JsEncoder.TableDigested rtd = rt.DigestTable();
            UserHostList r = new UserHostList();
            foreach (var item in ftd.AutoIntArray)
            {
                JsEncoder.TableValue v = (JsEncoder.TableValue)item;
                r.FavoriteHosts.Add(HostInformation.FromTable(v));
            }
            foreach (var item in rtd.AutoIntArray)
            {
                JsEncoder.TableValue v = (JsEncoder.TableValue)item;
                r.RecentHosts.Add(HostInformation.FromTable(v));
            }

            return r;
        }
    }
    public struct HostInformation
    {
        public String HostName;
        public int Port;

        public override string ToString() { return String.Concat(HostName, ":", Port.ToString()); }

        public static JsEncoder.TableValue ToTable(HostInformation Value)
        {
            JsEncoder.TableValue r = new JsEncoder.TableValue();
            r.Set(1, new JsEncoder.StringValue(Value.HostName));
            r.Set(2, new JsEncoder.IntValue(Value.Port));
            return r;
        }
        public static HostInformation FromTable(JsEncoder.TableValue Value)
        {
            JsEncoder.StringValue HostNameV = (JsEncoder.StringValue)Value.Get(1);
            JsEncoder.IntValue PortV = (JsEncoder.IntValue)Value.Get(2);
            return new HostInformation(HostNameV.GetValue(), PortV.GetValue());
        }

        public HostInformation(String HostName, int Port)
        {
            this.HostName = HostName;
            this.Port = Port;
        }
    }
}
