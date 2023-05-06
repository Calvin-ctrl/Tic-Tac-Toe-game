using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS408_client
{
    public partial class Form1 : Form
    {
        //boolean for termination and connection status of a user
        bool terminating = false;
        bool connected = false;

        //client socket
        Socket clientSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;//no need to write delegates
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }


        
        //connection button function
        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            string IP = textBox_IP.Text;
            int portNumber;

            //checking if 'port' is an int
            if (Int32.TryParse(textBox_Port.Text, out portNumber))
            {
                try//if so we connect
                {
                    clientSocket.Connect(IP, portNumber);
                    button_connect.Enabled = false;
                    textBox_Message.Enabled = true;
                    button_send.Enabled = true;

                    //connected status is true
                    connected = true;
                    richTextBox1.AppendText("Connected to the server. \n");

                    //
                    Thread receiveThread = new Thread(Receive);
                    receiveThread.Start();
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
        }
        //
        private void Receive()
        {
            while (connected)
            {
                try
                {
                    //converting the bytes of max 64 size (can change to more) of message to one string variable.
                    Byte[] buffer = new Byte[64]; //upperboud size
                    clientSocket.Receive(buffer);

                    //converting to string
                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    richTextBox1.AppendText("server:" + incomingMessage + "\n");
                }
                catch
                {
                    if (!terminating)
                    {
                        //enabling connect button and disabling text box and button
                        richTextBox1.AppendText("The server has diconnected \n");
                        button_connect.Enabled = true;
                        textBox_Message.Enabled = false;
                        button_send.Enabled = false;
                    }

                    clientSocket.Close();//closing the client socket
                    connected = false;
                }
            }
        }


        private void Form1_FormClosing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string message = textBox_Message.Text;
            if(message !="" && message.Length <= 64)
            {
                Byte[] buffer = Encoding.Default.GetBytes(message);//convert string message to byte
                clientSocket.Send(buffer);

            }
            else
            {
                richTextBox1.AppendText("The message is too long! \n");
            }
        }
    }
}
