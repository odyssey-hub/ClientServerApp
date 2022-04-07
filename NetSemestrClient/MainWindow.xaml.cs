using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Essy.Tools.InputBox;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;

namespace NetSemestrClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string server_address;
        int port;
        CancellationTokenSource cancelTokenSource;
        CancellationToken token;
        string directory;
        public List<FileRecord> files { get; set; }
        string nick;
        TcpClient client;

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            directory = textBoxDirPath.Text;
            files = new List<FileRecord>();
            // FillTable();
        }


        public void FillTable()
        {
            List<FileInfo> infos = GetFiles();
            files = new List<FileRecord>();
            tableFiles.ItemsSource = files;
            foreach (FileInfo info in infos)
            {
                FileRecord record = new FileRecord(info.Name, info.Length);
                files.Add(record);
            }
            tableFiles.Items.Refresh();
        }

        public List<FileInfo> GetFiles()
        {
            string[] filepaths= Directory.GetFiles(directory);
            List<FileInfo> list_files = new List<FileInfo>();
            foreach(var filepath in filepaths)
            {
                list_files.Add(new FileInfo(filepath));
            }
            return list_files;
        }

        public Dictionary<string, int> GetFilesDictionary()
        {
            List<FileInfo> fileInfos = GetFiles();
            Dictionary<string, int> dictFiles = new Dictionary<string, int>();
            foreach(var info in fileInfos)
            {
                dictFiles.Add(info.Name, (int)info.Length);
            }
            return dictFiles;
        }
 

        public void SendFile(FileInfo file, TcpClient client, NetworkStream stream)
        {
            BinaryFormatter format = new BinaryFormatter();
            
            int count;
            FileStream fs = new FileStream(file.FullName, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            long k = fs.Length;//Размер файла.
            format.Serialize(stream, file.Name);
            format.Serialize(stream, k.ToString());//Вначале передаём размер
            int block_size = 1024;
            if (k < 1024) block_size = (int)k;
            byte[] buf = new byte[block_size];
            while ((count = br.Read(buf, 0, block_size)) > 0)
            {
                format.Serialize(stream, buf);//А теперь в цикле по 1024 байта передаём файл
            }
            //format.Deserialize(stream);
            //br.Close();
            //fs.Close();
        }


        public void DeleteFiles(List<FileInfo> getfiles)
        {
            foreach(FileInfo file in getfiles)
            {
                file.Delete();
            }
        }


        public void DownloadFile(NetworkStream stream, string filename = null)
        {
            int count = 0;
            try
            {
                BinaryFormatter outformat = new BinaryFormatter();
                if (filename == null) filename = (string)outformat.Deserialize(stream);
                string filepath = directory + "\\" + filename;
                FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate);
                BinaryWriter bw = new BinaryWriter(fs);
                count = int.Parse(outformat.Deserialize(stream).ToString());
                int block_size = 1024;
                if (count < 1024) block_size = count;
                int i = 0;
                for (; i < count; i += block_size)
                {

                    byte[] buf = (byte[])(outformat.Deserialize(stream));
                    bw.Write(buf);
                }
                //outformat.Serialize(stream, "0");
                bw.Close();
                fs.Close();
            } catch (Exception e)
            {
                return;
            }
            FileRecord fileRecord = new FileRecord(filename,count);
            files.Add(fileRecord);
        }

        public void DownloadFiles(NetworkStream stream, List<FileInfo> getfiles)
        {
            foreach (FileInfo file in getfiles)
            {
                DownloadFile(stream);
            }

        }

        public List<FileInfo> TransformToFileInfo(List<string> filenames)
        {
            List<FileInfo> infos = new List<FileInfo>();
            foreach(var filename in filenames)
            {
                infos.Add(new FileInfo(directory+"\\"+filename));
            }
            return infos;
        }

        public bool ConnectToServer()
        {
            try
            {
                client = new TcpClient(server_address, port);
                NetworkStream stream = client.GetStream();
                BinaryFormatter format = new BinaryFormatter();
                string query = "Connect";
                format.Serialize(stream, query);
                string nick = textBoxNick.Text;
                format.Serialize(stream, nick);
                string max_clients = (string)format.Deserialize(stream);
                if (max_clients == "Места заняты") { throw new Exception("На сервере нет мест"); }
                bool isNickBusy = (bool) format.Deserialize(stream);
                if (isNickBusy) throw new Exception("Такой ник уже занят");

                Dictionary<string,int> files = GetFilesDictionary();
                format.Serialize(stream, files);
                List<string> getfiles = (List<string>)format.Deserialize(stream);
                DeleteFiles(TransformToFileInfo(getfiles));
                List<FileInfo> list_files = GetFiles();
                format.Serialize(stream, list_files);
                List<FileInfo> getfiles2 = (List<FileInfo>)format.Deserialize(stream);
                DownloadFiles(stream, getfiles2);


                format.Serialize(stream, "0");
                System.Windows.MessageBox.Show("Соединение установлено", "Успех");
            } catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message,"Ошибка");
                return false;
            }
            return true;
            
        }

        TcpClient request_client;
        async public void SendRequests(string nick)
        {
            try
            {
                cancelTokenSource = new CancellationTokenSource();
                token = cancelTokenSource.Token;
                while (true)
                {
                    await Task.Delay(2000);
                    if (token.IsCancellationRequested) return;
                    request_client = new TcpClient(server_address, port);
                    NetworkStream stream = request_client.GetStream();
                    BinaryFormatter format = new BinaryFormatter();
                    string query = "Whats new";
                    List<string> actions = new List<string>();
                    try
                    {
                        format.Serialize(stream, query);
                        format.Serialize(stream, nick);
                        actions = (List<string>)format.Deserialize(stream);
                        format.Serialize(stream,"0");
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    if (actions.Count != 0)
                    {
                        ParseActions(actions);
                    }

                }
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Сервер был отключен", "Ошибка");
                Dispatcher.BeginInvoke((Action)(() => buttonsDisable()));
            }

        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            server_address = textBoxServerAddress.Text;
            try
            {
                port = Convert.ToInt32(textBoxPort.Text);
                if (port < 1 || port > 65536) throw new Exception();
            } catch (Exception)
            {
                System.Windows.MessageBox.Show("Неверный TCP порт");
                return;
            }
            nick = textBoxNick.Text;
            if (nick.Length >= 30 || String.IsNullOrWhiteSpace(nick))
            {
                System.Windows.MessageBox.Show("Неверный ник");
                return;
            }
            directory = textBoxDirPath.Text;
            if (!ConnectToServer()) return;
            FillTable();
            btnConnect.IsEnabled = false;
            btnDisconnect.IsEnabled = true;
            textBoxNick.IsReadOnly = true;
            textBoxPort.IsReadOnly = true;
            textBoxServerAddress.IsReadOnly = true;
            btnAddfile.IsEnabled = true;
            btnChooseDir.IsEnabled = false;
            btnDeletefile.IsEnabled = true;
            btnRenamefile.IsEnabled = true;
            btnGetTCPbyUDP.IsEnabled = false;
            Task.Run(() => SendRequests(nick));
        }




        private void btnChooseDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxDirPath.Text = dialog.SelectedPath;
            }
        }




        private void btnAddfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filepath = dialog.FileName;
                FileInfo info = new FileInfo(filepath);
                if (info.Length > 10485760)
                {
                    System.Windows.MessageBox.Show("Размер файла не должен превышать 10МБ","Ошибка");
                    return;
                }
                try
                {
                    info.CopyTo(directory + "\\" + info.Name, false);
                } catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Ошибка");
                    return;
                }  
                Task.Run(() => Server_Addfile(info));
            }
        }

        private void Server_Addfile(FileInfo file)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                btnAddfile.IsEnabled = false;
                btnDeletefile.IsEnabled = false;
                btnRenamefile.IsEnabled = false;
                btnDisconnect.IsEnabled = false;
            }));
            try
            {
                client = new TcpClient(server_address, port);
                NetworkStream stream = client.GetStream();
                BinaryFormatter format = new BinaryFormatter();
                string query = "Addfile";
                format.Serialize(stream, query);
                format.Serialize(stream, nick);
                SendFile(file, client, stream);
                string isOk = (string)format.Deserialize(stream);
                format.Serialize(stream, "0");
                if (isOk == "NO") throw new Exception();
            }
            catch (SocketException es)
            {
                cancelTokenSource.Cancel();
                Dispatcher.BeginInvoke((Action)(() => buttonsDisable()));
                System.Windows.MessageBox.Show("Потеряна связь с сервером", "Ошибка");
                return;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Не удалось добавить файл на сервер", "Ошибка");
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    btnAddfile.IsEnabled = true;
                    btnDeletefile.IsEnabled = true;
                    btnRenamefile.IsEnabled = true;
                    btnDisconnect.IsEnabled = true;
                }
                ));
                try {
                    file.Delete();
                } catch (Exception)
                {
                    Disconnect();
                }
               
                return;
            }
            files.Add(new FileRecord(file.Name, file.Length));
            Dispatcher.BeginInvoke((Action)(() =>
            {
                tableFiles.Items.Refresh();
                btnAddfile.IsEnabled = true;
                btnDeletefile.IsEnabled = true;
                btnRenamefile.IsEnabled = true;
                btnDisconnect.IsEnabled = true;
            }));
            
        }

        private void Server_Renamefile(string oldname, string newname)
        {
            try
            {
                client = new TcpClient(server_address, port);
                NetworkStream stream = client.GetStream();
                BinaryFormatter format = new BinaryFormatter();
                string query = "Rename";
                format.Serialize(stream, query);
                string nick = textBoxNick.Text;
                format.Serialize(stream, nick);
                format.Serialize(stream, oldname);
                format.Serialize(stream, newname);
                string isOk = (string)format.Deserialize(stream);
                format.Serialize(stream, "0");
                if (isOk == "NO") throw new Exception();
            }
            catch (SocketException es)
            {
                cancelTokenSource.Cancel();
                Dispatcher.BeginInvoke((Action)(() => buttonsDisable()));
                System.Windows.MessageBox.Show("Потеряна связь с сервером", "Ошибка");
                return;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Не удалось переименовать файл на сервере", "Ошибка");
                try
                {
                    File.Move(directory + "\\" + newname, directory + "\\" + oldname);
                } catch (Exception)
                {
                    Disconnect();
                }
                
                return;
            }
            files.Find(item => item.Name == oldname).Name = newname;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                tableFiles.Items.Refresh();
            }
            ));
        }

        private void Server_Deletefile(string filename)
        {
            try
            {
                client = new TcpClient(server_address, port);
                NetworkStream stream = client.GetStream();
                BinaryFormatter format = new BinaryFormatter();
                string query = "Remove";
                format.Serialize(stream, query);
                format.Serialize(stream, nick);
                format.Serialize(stream, filename);
                string isOk = (string)format.Deserialize(stream);
                format.Serialize(stream, "0");
                if (isOk == "NO") throw new Exception();
            }
            catch (SocketException es)
            {
                cancelTokenSource.Cancel();
                Dispatcher.BeginInvoke((Action)(() => buttonsDisable()));
                System.Windows.MessageBox.Show("Потеряна связь с сервером", "Ошибка");
                return;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Не удалось удалить файл на сервере", "Ошибка");
                Disconnect();
                return;
            }
            files.Remove(files.Find(item => item.Name == filename));
            Dispatcher.BeginInvoke((Action)(() => tableFiles.Items.Refresh()));
        }


        private void btnRenamefile_Click(object sender, RoutedEventArgs e)
        {
            FileRecord file;
            string filename;
            try
            {
                file =  (FileRecord)tableFiles.SelectedItem;
                filename = file.Name;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Ничего не выбрано");
                return;
            }
            string newname;
            try
            {
                string dir = directory;
                newname = InputBox.ShowInputBox("Введите новое имя файла", filename, false);
                if (newname == null) return;
                File.Move(dir + "\\" + filename, dir + "\\" + newname);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Не удалось переименовать файл");
                return;
            }
            Server_Renamefile(filename, newname);
        }

        private void btnDeletefile_Click(object sender, RoutedEventArgs e)
        {
            FileRecord file;
            string filename;
            try
            {
                file = (FileRecord)tableFiles.SelectedItem;
                filename = file.Name;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Ничего не выбрано");
                return;
            }
            try
            {
                File.Delete(directory+"\\"+file.Name);
            } catch (Exception)
            {
                System.Windows.MessageBox.Show("Не удалось удалить файл");
                return;
            }
            Server_Deletefile(filename);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                cancelTokenSource.Cancel();
                client = new TcpClient(server_address, port);
                NetworkStream stream = client.GetStream();
                BinaryFormatter format = new BinaryFormatter();
                string query = "Disconnect";
                format.Serialize(stream, query);
                format.Serialize(stream, nick);
                format.Deserialize(stream);
            } catch (Exception) { }

           
        }


        public void ParseActions(List<string> actions)
        {
            foreach (string action in actions)
            {
                if (action.StartsWith("Разорвать соединение"))
                {
                    Disconnect();
                    System.Windows.MessageBox.Show("Вы были отключены от сервера");
                    //cancelTokenSource.Cancel();
                    //Dispatcher.BeginInvoke((Action)(() => btnConnect.IsEnabled = true));
    
                    break;
                }
                ParseAction(action);
            }
        }



        TcpClient download_client;
        public void ParseAction(string action)
        {
            string filename = action.Substring(action.IndexOf(':') + 2); 
            if (action.StartsWith("Добавление файла:"))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    download_client = new TcpClient(server_address, port);
                    NetworkStream stream = download_client.GetStream();
                    string query = "Download";
                    formatter.Serialize(stream, query);
                    formatter.Serialize(stream, nick);
                    formatter.Serialize(stream, filename);
                    DownloadFile(stream, filename);
                } catch (Exception)
                {
                    Disconnect();
                    return;
                }
            } else if (action.StartsWith("Переименование файла:"))
            {
                string dir = directory;
                string oldname = "";
                string newname = "";
                try
                {
                    char[] sep = {'<','>'};
                    string[] words = action.Split(sep);
                    oldname = words[1];
                    newname = words[3];
                    File.Move(dir + "\\" + oldname, dir + "\\" + newname);
                } catch (Exception) {
                    Disconnect();
                    return;
                };
                files.Find(item => item.Name == oldname).Name = newname;
            } else if (action.StartsWith("Удаление файла:"))
            {
                try
                {

                    File.Delete(directory + "\\" + filename);
                    files.Remove(files.Find(item => item.Name == filename));
                } catch (Exception) {
                    Disconnect();
                    return;
                }
            }
            Dispatcher.BeginInvoke((Action)(() => tableFiles.Items.Refresh())); 

        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();  
        }

        public void Disconnect()
        {
            try
            {
                client = new TcpClient(server_address, port);
                NetworkStream stream = client.GetStream();
                BinaryFormatter format = new BinaryFormatter();
                string query = "Disconnect";
                format.Serialize(stream, query);
                format.Serialize(stream, nick);
                cancelTokenSource.Cancel();
            }
            catch (Exception e)
            {

            }
            Dispatcher.BeginInvoke((Action)(() => buttonsDisable()));
            
        }

        public void buttonsDisable()
        {
            btnConnect.IsEnabled = true;
            textBoxNick.IsReadOnly = false;
            textBoxPort.IsReadOnly = false;
            textBoxServerAddress.IsReadOnly = false;
            btnDeletefile.IsEnabled = false;
            btnAddfile.IsEnabled = false;
            btnChooseDir.IsEnabled = true;
            btnRenamefile.IsEnabled = false;
            btnDisconnect.IsEnabled = false;
            btnGetTCPbyUDP.IsEnabled = true ;
        }

        private void btnGetTCPbyUDP_Click(object sender, RoutedEventArgs e)
        {
            int serv_udp;
            try
            {
                serv_udp = Convert.ToInt32(textBoxUDP.Text);
                if (serv_udp < 1 || serv_udp > 65536) throw new Exception();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Неверный UDP порт");
                return;
            }
            string str_client_udp = InputBox.ShowInputBox("Введите UDPPort для прослушивания", "8000", false);
            int client_udp;
            try
            {
                client_udp = Convert.ToInt32(str_client_udp);
                if (client_udp < 1 || client_udp > 65536) throw new Exception();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Неверный UDP порт");
                return;
            }
            UdpClient sender_message = new UdpClient();
            try
            {
                Task.Run(() => ReceiveMessage(client_udp));
                string message = client_udp.ToString(); // сообщение для отправки
                string ip_broadcast = "";
                byte[] data = Encoding.UTF8.GetBytes(message);

                //получение широковещательного адреса
                IPAddress[] ip = Dns.GetHostAddresses(Dns.GetHostName());
                IPAddress mask = null;

                foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (ip[8].Equals(unicastIPAddressInformation.Address))
                            {
                                Console.WriteLine(unicastIPAddressInformation.IPv4Mask);
                                mask = unicastIPAddressInformation.IPv4Mask;
                            }
                        }
                    }
                }

                byte[] broadcastIPBytes = new byte[4];
                byte[] hostBytes = ip[8].GetAddressBytes();
                byte[] maskBytes = mask.GetAddressBytes();
                for (int i = 0; i < 4; i++)
                {
                    broadcastIPBytes[i] = (byte)(hostBytes[i] | (byte)~maskBytes[i]);
                    ip_broadcast += broadcastIPBytes[i].ToString() + ".";
                }
                ip_broadcast = ip_broadcast.Substring(0, ip_broadcast.Length - 1);

                sender_message.Send(data, data.Length, ip_broadcast, serv_udp); // отправка
            } catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK);
            }
            finally
            {
                sender_message.Close();
            }
            
        }
        private void ReceiveMessage(int port_UDP)
        {
            UdpClient receiver = new UdpClient(port_UDP); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                string message = Encoding.Unicode.GetString(data);

                //Console.WriteLine("Собеседник: {0}", message);
                Dispatcher.BeginInvoke((Action)(() => {
                    textBoxPort.Text = message;
                    textBoxServerAddress.Text = remoteIp.Address.ToString();
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK);
            }
            finally
            {
                receiver.Close();
            }
        }
    }
}
