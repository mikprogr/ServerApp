using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Text;
using System.IO;
using System.Net.Security;
using System.Threading.Tasks;

namespace ServerApp
{
    public partial class Form1 : Form
    {
        private TcpListener listener;
        private TcpClient activeClient;
        private SslStream sslStream;
        private bool serverRunning = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serverRunning)
            {
                MessageBox.Show("Server is already running");
                return;
            }

            Task.Run(() => StartServer());
        }

        private void StartServer()
        {
            try
            {
                var certificate = new X509Certificate2(@"C:\Users\Mikołaj\Desktop\ServerApp\server.pfx", "password");

                listener = new TcpListener(IPAddress.Any, 12345);
                listener.Start();
                serverRunning = true;

                this.Invoke(new Action(() =>
                {
                    if (!string.IsNullOrWhiteSpace(textBox3.Text))
                    {
                        textBox3.Clear();
                    }
                    textBox3.AppendText("Server is listening..." + Environment.NewLine);
                }));

                while (serverRunning)
                {
                    activeClient = listener.AcceptTcpClient();
                    sslStream = new SslStream(activeClient.GetStream(), false);

                    try
                    {
                        sslStream.AuthenticateAsServer(certificate, false, SslProtocols.Tls12, true);

                        this.Invoke(new Action(() =>
                        {
                            textBox3.AppendText("Client connected and SSL/TLS authentication succeeded" + Environment.NewLine);
                        }));

                        // Obsługuje komunikację z klientem w osobnym wątku
                        Task.Run(() => HandleClientCommunication());
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            textBox3.AppendText("Error: " + ex.Message + Environment.NewLine);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    textBox3.AppendText("Error starting server: " + ex.Message + Environment.NewLine);
                }));
            }
        }

        private void HandleClientCommunication()
        {
            try
            {
                if (sslStream.CanRead)
                {
                    using (var reader = new StreamReader(sslStream, Encoding.UTF8))
                    {
                        string message;
                        while ((message = reader.ReadLine()) != null)
                        {
                            this.Invoke(new Action(() =>
                            {
                                textBox1.AppendText("Received message: " + message + Environment.NewLine);
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    textBox3.AppendText("Error reading from client: " + ex.Message + Environment.NewLine);
                }));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!serverRunning)
            {
                textBox3.AppendText("Server is not running" + Environment.NewLine);
                return;
            }

            try
            {
                serverRunning = false;

                if (listener != null)
                {
                    listener.Stop();
                }

                this.Invoke(new Action(() =>
                {
                    if (!string.IsNullOrWhiteSpace(textBox3.Text))
                    {
                        textBox3.Clear();
                    }
                    textBox3.AppendText("Server stopped" + Environment.NewLine);
                }));
            }
            catch (Exception ex)
            {
                textBox3.AppendText("Error stopping server: " + ex.Message + Environment.NewLine);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (activeClient == null || !activeClient.Connected)
            {
                textBox3.AppendText("No active client or client is disconnected." + Environment.NewLine);
                return;
            }

            string messageToSend = textBox2.Text;
            if (string.IsNullOrWhiteSpace(messageToSend))
            {
                textBox3.AppendText("Please enter a message to send." + Environment.NewLine);
                return;
            }

            try
            {
                // Używamy przechowywanego SslStream do wysyłania wiadomości
                if (sslStream != null && sslStream.CanWrite)
                {
                    using (var writer = new StreamWriter(sslStream, Encoding.UTF8, 1024, leaveOpen: true))
                    {
                        writer.WriteLine(messageToSend);
                        writer.Flush();
                        textBox3.AppendText("Message sent: " + messageToSend + Environment.NewLine);
                    }
                }
                else
                {
                    textBox3.AppendText("SSL Stream is not available for writing." + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                textBox3.AppendText("Error sending message: " + ex.Message + Environment.NewLine);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
