using Microsoft.VisualBasic.Logging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TicTacToe_Server

{
    public partial class Form1 : Form
    {
        
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Board board = new Board();
        //Dictionary for saving socket_player pair
        Dictionary<Socket, Player> socket_player = new Dictionary<Socket, Player>();
        //Dictionary for active players 
        Dictionary<Socket, Player> active_players = new Dictionary<Socket, Player>();
        int sequence_number = 0;
        int current_player_idx;
        
        bool listening = false;
        bool terminating = false;
        bool game = false;
        bool game_over = false;
        private PlayerSymbol currentPlayerSymbol;

        //getting the list of players from the socket_player dictionary
        private List<Player> get_players()
        {
            return new List<Player>(active_players.Values);
        }

        private int return_enum_index(char player_symbol)
        {
            if (player_symbol == 'X')
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void send_board(string game_info)
        {
            string boardString = "\n" + board.GetBoardString() + game_info;
            foreach (KeyValuePair<Socket, Player> pair in active_players)
            {
                pair.Key.Send(Encoding.Default.GetBytes(boardString));
                pair.Value.AnswerReceived = false;
            }

        }

        private void listen_port_button_Click(object sender, EventArgs e)
        {

            int serverPort;
            if (Int32.TryParse(port_text_box.Text, out serverPort))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(3);

                listening = true;
                listen_port_button.Enabled = false;
                port_text_box.Enabled = false;
                Thread acceptThread = new Thread(accept_connections);
                acceptThread.Start();
                logs.AppendText("Started listening on port: " + serverPort + "\n");
                


            }
            else
            {
                logs.AppendText("Please check port number \n");
            }
        }

        private void accept_connections()
        {
            while (listening)
            {
                try
                {
                    Socket new_client = serverSocket.Accept();
                    socket_player.Add(new_client, new Player());

                    //this maybe commented out later as well 
                    logs.AppendText("A new client is connected to the game.\n");

                    Thread receiveThread = new Thread(() => receive(new_client)); // updated
                    receiveThread.Start();
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        logs.AppendText("The socket stopped working.\n");
                    }

                }
            }
        }

        private void receive(Socket this_client)
        {
            bool connected = true;
            Player player = socket_player[this_client];

            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[128];
                    this_client.Receive(buffer);

                    //player sends its username 
                    string incomingMessage = Encoding.Default.GetString(buffer);
                    int nullIndex = incomingMessage.IndexOf('\0');
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0')); //incoming message should be the name ? 

                    //just to see what was the username 
                    //we can comment out this part later as well
                    //because we dont want user to give us a empty username 
                    //or username that already exists
                    //logs.AppendText("Clients username: " + incomingMessage + "\n");

                    //checking whether the incoming message was empty 
                    if (incomingMessage != "")
                    {
                        //we could do this at client part as well
                        //I think that when the user first connects to a port
                        // the username box enables, and user passes its username
                        //after sending the username it should get disabled
                        //so we just would not check if the player has a username or not 
                        if (!player.has_username())
                        {
                            //cheking whether the username was taken before 
                            //by other player
                            bool username_exists = false;


                            foreach (Player p in get_players())
                            {
                                if (p.Username == incomingMessage)
                                {
                                    username_exists = true;
                                    break;
                                }
                            }
                            //if the username was not used before, then the player succesffully joins the game
                            if (!username_exists)
                            {
                                player.Username = incomingMessage;
                                this_client.Send(Encoding.Default.GetBytes("Player: " + player.Username + ", you have successfully joined to the game"));
                                active_players.Add(this_client, player);
                                logs.AppendText(player.Username + " has succesfullly joined to the game\n");

                                if (active_players.Count == 2)
                                {
                                    game = true;
                                    start_game();
                                }

                            }
                            else
                            {
                                this_client.Send(Encoding.Default.GetBytes("Dear player this username already exists, you need to have a unique username"));
                                connected  = false;
                                socket_player.Remove(this_client);
                                this_client.Close();
                                throw new Exception();

                            }
                        }

                        else if (game)
                        {
                            if (player.symbol == currentPlayerSymbol)
                            {
                                int cell;
                                if (Int32.TryParse(incomingMessage, out cell) && cell >= 1 && cell <= 9)
                                {
                                    if (board.IsCellFree(cell))
                                    {
                                        board.MakeMove(player.symbol, cell);
                                        string winner = board.check_winneer();
                                        if (winner != "no winner" && winner != "draw")
                                        {
                                            string winner_info = player.Username + " has won the game";
                                            send_board(winner_info);
                                            connected = false;
                                            break;
                                        }
                                        else if (winner == "draw")
                                        {
                                            connected = false;
                                            send_board("It was a draw. Thank you for playing our game.");
                                            break;
                                        }

                                        current_player_idx = (current_player_idx + 1) % 2;
                                        currentPlayerSymbol = (PlayerSymbol)current_player_idx;

                                        KeyValuePair<Socket, Player> current_player = new List<KeyValuePair<Socket, Player>>(active_players)[current_player_idx];
                                        current_player.Value.symbol = currentPlayerSymbol;
                                        string game_info = "It is " + current_player.Value.Username + "'s turn with " + currentPlayerSymbol.ToString();
                                        send_board(game_info);
                                    }
                                    else
                                    {
                                        this_client.Send(Encoding.Default.GetBytes("Cell is already taken. Please choose another cell."));
                                    }
                                }
                                else
                                {
                                    this_client.Send(Encoding.Default.GetBytes("Invalid input. Please enter a number between 1 and 9."));
                                }
                            }
                            else
                            {
                                this_client.Send(Encoding.Default.GetBytes("It's not your turn. Please wait for your turn."));
                            }
                        }
                    }
                    else
                    {
                        this_client.Send(Encoding.Default.GetBytes("Dear player you cannot have an empty username"));
                        this_client.Close();
                        socket_player.Remove(this_client);
                        connected = false;
                    }

                }
                

                catch
                {
                    if (!terminating)
                    {
                        // add more functionality to this side 
                        //who disconnected 
                        //what was the username 
                        //did the player had the username ? 
                        logs.AppendText("A client has disconnected\n");
                        //if user disconnects 

                        if (game)
                        {
                            game = false;
                            var otherPlayer = active_players.FirstOrDefault(p => p.Key != this_client);
                            if (otherPlayer.Key != null)
                            {
                                otherPlayer.Key.Send(Encoding.Default.GetBytes("The other player has disconnected. You win."));
                            }
                        }
                    }
                    this_client.Close();
                    socket_player.Remove(this_client);
                    active_players.Remove(this_client);
                    connected = false;
                
                }
            }
        }
        private void Form1_FormClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            //listening = false;
            //terminating = true;
            //Environment.Exit(0);
            if (active_players.Count == 1)
            {
                Socket remainingPlayerSocket = active_players.Keys.GetEnumerator().Current;
                remainingPlayerSocket.Send(Encoding.Default.GetBytes("The other player has disconnected. You win!"));
            }
        }
        private void start_game()
        {
            if (game)
            {
                if (active_players.Count == 2)
                {
                    string log_text = "Game started with " + active_players.Count + " players.\n";
                    logs.AppendText(log_text);
                    foreach (KeyValuePair<Socket, Player> pair in active_players)
                    {
                        pair.Key.Send(Encoding.Default.GetBytes(log_text));
                       
                    }


                    Random random = new Random();
                    current_player_idx = random.Next(0, 2);
                    currentPlayerSymbol = (PlayerSymbol)current_player_idx;
                    KeyValuePair<Socket, Player> current_player = new List<KeyValuePair<Socket, Player>>(active_players)[current_player_idx];
                    current_player.Value.symbol = currentPlayerSymbol;
                    //firstPlayer.Key.Send(Encoding.Default.GetBytes("You start the game."));
                    string game_info = "It is " + current_player.Value.Username + "'s turn with " + currentPlayerSymbol.ToString();
                    logs.AppendText(current_player.Value.Username + " starts the game.\n");
                    send_board(game_info);
                }
                else
                {
                    logs.AppendText("Not enough players to start a game.\n");
                }
            }
        }
    }
}