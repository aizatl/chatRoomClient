using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFClient
{
    public partial class MainWindow : Window
    {
        private NetworkStream stream;
        private string clientName = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            UserInput.IsEnabled = false;
            SendButton.IsEnabled = false;
            ExitButton.IsEnabled = false;
        }
        private void OkayNameBtnSumbit(object sender, RoutedEventArgs e)
        {
            clientName = clientNameTxt.Text.ToString();
            if (!string.IsNullOrEmpty(clientName))
            {
                errorMessage.Visibility = Visibility.Collapsed;
                clientNameTxt.IsEnabled = false;
                okayNameBtn.IsEnabled = false;
                ConnectWithServer();
                SetName(clientName);
            }
            else {
                errorMessage.Visibility = Visibility.Visible;
                clientNameTxt.Focus();
            }
        }
        private void ConnectWithServer()
        {
            try
            {
                TcpClient client = new TcpClient("192.168.100.11", 12345);// laptop ipconfig
                stream = client.GetStream();
                Thread readThread = new Thread(ReadFromServer);
                readThread.Start();
                UserInput.IsEnabled = true;
                SendButton.IsEnabled = true;
                ExitButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                AddMessageToChat($"Error: {ex.Message}", false);
            }
        }

        private void ReadFromServer()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {   if (stream != null) {
                    int byteCount = stream.Read(buffer, 0, buffer.Length);
                    string responseFromServer = Encoding.ASCII.GetString(buffer, 0, byteCount);

                    if (responseFromServer.ToLower() == "exit")
                    {
                        break;
                    }

                    if (responseFromServer.StartsWith("You are "))
                    {
                        Dispatcher.Invoke(() => AddMessageToChat(responseFromServer, false, true));
                    }
                    else
                    {
                        Dispatcher.Invoke(() => AddMessageToChat(responseFromServer, false));
                    }
                }
                
            }
        }
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SendBtnClicked(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                byte[] data = Encoding.ASCII.GetBytes("exit");
                stream.Write(data, 0, data.Length);
                Application.Current.Shutdown();
                stream.Close();
            }
        }
        private void SendBtnClicked(object sender, RoutedEventArgs e)
        {
            string userInput = UserInput.Text.ToString();
            if (string.IsNullOrEmpty(userInput)) return;
            if (userInput.ToLower().ToLower() == "exit")
            {
                Application.Current.Shutdown();
            }
            
            AddMessageToChat(userInput, true);//add the mess into ui

            byte[] data = Encoding.ASCII.GetBytes(userInput);
            stream.Write(data, 0, data.Length);//send message to server
            if (userInput.ToLower() == "exit")
            {
                stream.Close();
            }
            UserInput.Clear();
        }
        private void SetName(string name)
        {
            byte[] data = Encoding.ASCII.GetBytes(name);
            stream.Write(data, 0, data.Length);//send name to server
        }

        private void AddMessageToChat(string message, bool isClient, bool justConnect = false)
        {
            TextBlock messageBlock = new TextBlock();
            if (justConnect)
            {
                messageBlock = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(15),
                    TextWrapping = TextWrapping.Wrap,
                    Background = new SolidColorBrush(Colors.Black),
                    Foreground = Brushes.Yellow,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(1),
                    MaxWidth = 300,
                    FontSize = 10
                };
            }
            else
            {
                messageBlock = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(5),
                    TextWrapping = TextWrapping.Wrap,
                    Background = new SolidColorBrush(Colors.LightBlue),
                    HorizontalAlignment = isClient ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    Padding = new Thickness(1),
                    MaxWidth = 300
                };
            }

            ChatDisplay.Children.Add(messageBlock);
            ChatScrollViewer.ScrollToEnd();
        }

        private void ExitBtnClicked(object sender, RoutedEventArgs e)
        {
            byte[] data = Encoding.ASCII.GetBytes("exit");
            stream.Write(data, 0, data.Length);
            Application.Current.Shutdown();
            stream.Close();
        }
    }
}
