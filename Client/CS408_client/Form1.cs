using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CS408_client
{
    public partial class Rematch : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        public Rematch()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();

            // Bind the disconnect button click event
            this.button_disconnect.Click += new System.EventHandler(this.button_disconnect_Click);
            this.button_rematch.Click += new System.EventHandler(this.button_rematch_Click); // new event handler for rematch button
            SetRematchButtonState(false);
        }

        private void SetRematchButtonState(bool state)
        {
            if (button_rematch.InvokeRequired)
            {
                button_rematch.Invoke(new Action(() => button_rematch.Enabled = state));
            }
            else
            {
                button_rematch.Enabled = state;
            }
        }


        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            string IP = textBox_IP.Text;
            int portNumber;

            if (Int32.TryParse(textBox_Port.Text, out portNumber))
            {
                try
                {
                    if (string.IsNullOrEmpty(IP))
                    {
                        richTextBox1.AppendText("Please enter an IP address. \n");
                        return;
                    }

                    clientSocket.Connect(IP, portNumber);
                    string username = textBox_name.Text;

                    if (string.IsNullOrEmpty(username))
                    {
                        richTextBox1.AppendText("Please enter a username. \n");
                        return;
                    }

                    // Send the username to the server
                    SendMessage(username);

                    // Receive server response
                    string serverResponse = ReceiveMessage();
                    if (serverResponse.StartsWith("Player:"))
                    {
                        connected = true;
                        button_connect.Enabled = false;
                        textBox_Message.Enabled = true;
                        button_send.Enabled = true;
                        button_disconnect.Enabled = true; // Enable the disconnect button
                        richTextBox1.AppendText("Connected to the server. \n");

                        Thread receiveThread = new Thread(Receive);
                        receiveThread.Start();
                    }
                    else
                    {
                        richTextBox1.AppendText("Couldn't connect to the server: " + serverResponse + "\n");
                    }
                }
                catch
                {
                    richTextBox1.AppendText("Couldn't connect to the server. \n");
                }
            }
            else
            {
                richTextBox1.AppendText("Check the port for a mistake \n");
            }

            SetRematchButtonState(false); // Rematch can't be requested as soon as the client is connected
        }


        private void button_disconnect_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                // Close the client socket and stop the receiving thread.
                clientSocket.Close();
                connected = false;
                terminating = true; // if you want to terminate the whole application

                // Update the UI
                button_connect.Enabled = true;
                textBox_Message.Enabled = false;
                button_send.Enabled = false;
                button_disconnect.Enabled = false;
                SetRematchButtonState(false); // Disable the rematch button

                richTextBox1.AppendText("Disconnected from the server.\n");
            }
            else
            {
                richTextBox1.AppendText("Not currently connected.\n");
            }
        }


        private void button_rematch_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                SendMessage("Rematch");
                richTextBox1.AppendText("Request for a rematch has been sent.\n");
            }
            else
            {
                richTextBox1.AppendText("You need to be connected to request a rematch.\n");
            }

            SetRematchButtonState(false); // Rematch can't be requested immediately after a request has been made
        }

        private void Receive()
        {
            while (connected)
            {
                try
                {
                    string incomingMessage = ReceiveMessage();
                    int nullIndex = incomingMessage.IndexOf('\0');

                    

                    if (nullIndex != -1)
                    {
                        incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));
                    }

                    if (incomingMessage != "")
                    {
                        richTextBox1.AppendText("Server: " + incomingMessage + "\n");

                        if (incomingMessage.Contains("Type 'rematch' if you want to play again."))
                        {
                            SetRematchButtonState(true); // Rematch can be requested if the game is over
                        }

                        if (incomingMessage == "Username Exists.")
                        {
                            throw new Exception();
                        }
                    }
                    //if (incomingMessage.StartsWith("DISCONNECT:"))
                    //{
                    //    string reason = incomingMessage.Substring("DISCONNECT:".Length);
                    //    richTextBox1.AppendText("Disconnected by the server: " + reason + "\n");

                    //    button_connect.Enabled = true;
                    //    textBox_Message.Enabled = false;
                    //    button_send.Enabled = false;

                    //    connected = false;
                    //    clientSocket.Close();
                    //}
                    //else
                    //{
                    //    richTextBox1.AppendText("Server: " + incomingMessage + "\n");
                    //}
                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("The server has disconnected. \n");
                        button_connect.Enabled = true;
                        textBox_Message.Enabled = false;
                        button_send.Enabled = false;
                    }

                    clientSocket.Close();
                    connected = false;
                }
            }
        }

        private string ReceiveMessage()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        private void SendMessage(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            clientSocket.Send(buffer);
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string message = textBox_Message.Text;
            if (message != "" && message.Length <= 64)
            {
                SendMessage(message);
            }
            else
            {
                richTextBox1.AppendText("The message is too long! \n");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}




