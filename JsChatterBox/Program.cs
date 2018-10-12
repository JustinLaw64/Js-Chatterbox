using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsChatterBox
{
    static class Program
    {
        public static PersistentDataManager DataManager = null;

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
        public String UserName = null;
        public UserHostList HostList = null;

        public void Save()
        {
            if (_IsLoaded)
            {
                System.IO.Directory.CreateDirectory(DataSavePath);

                String HostListAsText = JsEncoder.EncoderStream.EncodeTable(UserHostList.ToTable(HostList));

                String VersionFileBackupPath = String.Concat(VersionFilePath, ".bak");
                String UserNameFileBackupPath = String.Concat(UserNameFilePath, ".bak");
                String HostListFileBackupPath = String.Concat(HostListFilePath, ".bak");

                if (System.IO.File.Exists(VersionFileBackupPath)) System.IO.File.Delete(VersionFileBackupPath);
                if (System.IO.File.Exists(UserNameFileBackupPath)) System.IO.File.Delete(UserNameFileBackupPath);
                if (System.IO.File.Exists(HostListFileBackupPath)) System.IO.File.Delete(HostListFileBackupPath);

                if (System.IO.File.Exists(VersionFilePath)) System.IO.File.Move(VersionFilePath, VersionFileBackupPath);
                if (System.IO.File.Exists(UserNameFilePath)) System.IO.File.Move(UserNameFilePath, UserNameFileBackupPath);
                if (System.IO.File.Exists(HostListFilePath)) System.IO.File.Move(HostListFilePath, HostListFileBackupPath);

                System.IO.File.WriteAllText(VersionFilePath, NetworkConstants.VersionString, System.Text.Encoding.Unicode);
                System.IO.File.WriteAllText(UserNameFilePath, UserName, System.Text.Encoding.Unicode);
                System.IO.File.WriteAllText(HostListFilePath, HostListAsText, System.Text.Encoding.Unicode);
            }
        }
        public bool Load()
        {
            bool r = false;

            bool ve = System.IO.File.Exists(VersionFilePath);
            if (ve)
            {
                String VersionFileContents = System.IO.File.ReadAllText(VersionFilePath, System.Text.Encoding.Unicode);
                if (VersionFileContents == NetworkConstants.VersionString)
                    r = true;
                else
                {
                    String IncompatPath = String.Concat(DataSavePath, "_", VersionFileContents);
                    if (System.IO.Directory.Exists(IncompatPath)) System.IO.Directory.Delete(IncompatPath);
                    System.IO.Directory.Move(DataSavePath, IncompatPath);
                    System.Windows.Forms.MessageBox.Show(String.Concat(
                        "The application tried to load Js ChatterBox files that were incompatible ",
                        "with this version. They have been moved to \"", IncompatPath, "\"."));
                }
            }

            if (r)
            {
                try
                {
                    UserName = System.IO.File.ReadAllText(UserNameFilePath, System.Text.Encoding.Unicode);
                    String HostListFileContents = System.IO.File.ReadAllText(HostListFilePath, System.Text.Encoding.Unicode);
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
                        if (System.IO.File.Exists(FinalIncompatPath))
                            FinalIncompatPath = null;
                    }
                    System.IO.Directory.Move(DataSavePath, FinalIncompatPath);

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
            }
            _IsLoaded = true;
            return r;
        }

        private bool _IsLoaded = false;

        public static String DataSavePath { get { return ("{0}\\JsChatterBox").Replace("{0}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)); } }
        public static String VersionFilePath { get { return String.Concat(DataSavePath, "\\Version.dat"); } }
        public static String UserNameFilePath { get { return String.Concat(DataSavePath, "\\UserName.dat"); } }
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
