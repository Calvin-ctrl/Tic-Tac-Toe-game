// new

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
        //Disctionary for waiting players
        Dictionary<Socket, Player> waiting_players = new Dictionary<Socket, Player>();
        int sequence_number = 0;
        int current_player_idx;
        private Socket currentPlayerSocket;
        private PlayerSymbol disconnectedPlayerSymbol;


        bool listening = false;
        bool terminating = false;
        bool game = false;
        bool game_over = false;
        private PlayerSymbol currentPlayerSymbol;
        private int rematchRequests = 0;
        private bool rematchInProgress = false;




        //getting the list of players from the socket_player dictionary
        private List<Player> get_players()
        {
            return new List<Player>(active_players.Values);
        }
        private List<Player> get_waiting_list_players()
        {
            return new List<Player>(waiting_players.Values);
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
                    if (socket_player.Count < 4)
                    {

                        socket_player.Add(new_client, new Player());

                        //this maybe commented out later as well 
                        logs.AppendText("A new client is connected to the game.\n");




                        Thread receiveThread = new Thread(() => receive(new_client)); // updated
                        receiveThread.Start();
                    }
                    else
                    {
                        new_client.Send(Encoding.Default.GetBytes("The game is full. Please try again later."));
                        new_client.Close();
                    }
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

                            foreach (Player p in get_waiting_list_players())
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
                                this_client.Send(Encoding.Default.GetBytes("Player: " + player.Username + ", has succesfully joined the game"));


                                if (active_players.Count < 2)
                                {
                                    // If there are less than two active players, 
                                    active_players.Add(this_client, socket_player[this_client]);
                                    //socket_player[this_client].symbol = (PlayerSymbol)active_players.Count - 1;

                                    string message = "Player: " + socket_player[this_client].Username + ", you have successfully joined the game";
                                    this_client.Send(Encoding.Default.GetBytes(message));
                                    logs.AppendText(socket_player[this_client].Username + " has successfully joined the game\n");

                                    if (active_players.Count == 2)
                                    {
                                        game = true;
                                        start_game();
                                    }
                                }
                                // add to the waiting list
                                else if (waiting_players.Count < 2)
                                {
                                    waiting_players.Add(this_client, socket_player[this_client]);
                                    string message = "Player: " + socket_player[this_client].Username + ", you are in waiting room";
                                    this_client.Send(Encoding.Default.GetBytes(message));
                                    logs.AppendText(socket_player[this_client].Username + " is in waiting room\n");

                                }

                            }
                            else
                            {
                                this_client.Send(Encoding.Default.GetBytes("Dear player this username already exists, you need to have a unique username"));
                                connected  = false;
                                socket_player.Remove(this_client);
                                this_client.Close();

                                //throw new Exception(); No need for throwing exception 
                                // catch block will be used only for the players that could already join the game and have unique username 
                                logs.AppendText("A client has disconnected\n"); //server rich text box will also show that the client is disconnected 


                            }
                        }

                        else if (game)
                        {
                            if (!game_over)
                            {
                                if (player.symbol == currentPlayerSymbol)
                                {
                                    int cell;
                                    if (Int32.TryParse(incomingMessage, out cell) && cell >= 1 && cell <= 9)
                                    {
                                        if (board.IsCellFree(cell))
                                        {
                                            board.MakeMove(player.symbol, cell);
                                            string outcome = board.check_winneer();

                                            //updated code
                                            if (outcome == "winner") {
                                                game_over = true;
                                                game = false;
                                                string winner_info = player.Username + " has won the game. Type 'rematch' if you want to play again.";
                                                send_board(winner_info);
                                                player.Wins++;
                                                player.GamesPlayed++;
                                                var otherPlayer = socket_player.FirstOrDefault(p => p.Key != this_client);
                                                if (otherPlayer.Key != null)
                                                {
                                                    otherPlayer.Value.Losses++;
                                                    otherPlayer.Value.GamesPlayed++;
                                                }
                                                sendStats();
                                            }
                                            else if (outcome == "draw")
                                            {
                                                game_over = true;
                                                game = false;
                                                send_board("It was a draw. Thank you for playing our game. Type 'rematch' if you want to play again.");
                                                foreach (var active_player in socket_player.Values)
                                                {
                                                    active_player.Draws++;
                                                    active_player.GamesPlayed++;
                                                }
                                                sendStats();
                                            }
                                            else
                                            {
                                                current_player_idx = (current_player_idx + 1) % 2;
                                                currentPlayerSymbol = (PlayerSymbol)current_player_idx;

                                                KeyValuePair<Socket, Player> current_player = new List<KeyValuePair<Socket, Player>>(active_players)[current_player_idx];
                                                current_player.Value.symbol = currentPlayerSymbol;
                                                string game_info = "It is " + current_player.Value.Username + "'s turn with " + currentPlayerSymbol.ToString();
                                                send_board(game_info);
                                            }
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
                            else if (incomingMessage.ToLower() == "rematch")
                            {
                                player.WantsRematch = true;
                                this_client.Send(Encoding.Default.GetBytes("You requested a rematch. Waiting for the other player..."));

                                // Check if both players requested a rematch
                                if (active_players.All(p => p.Value.WantsRematch))
                                {
                                    game_over = false;
                                    game = true;
                                    start_game();
                                }
                            }


                        }
                        //add the condition if the user does not type rematch
                        //warn the user about the mistake
                        //and wait for the response from the user 
                        else if (game_over)
                        {
                            if (incomingMessage.ToLower() == "rematch")
                            {
                                player.WantsRematch = true;
                                this_client.Send(Encoding.Default.GetBytes("You requested a rematch. Waiting for the other player..."));

                                // Check if both players requested a rematch
                                if (active_players.All(p => p.Value.WantsRematch))
                                {
                                    game_over = false;
                                    game = true;
                                    start_game();
                                }
                            }
                            //logic for the messages that are not "rematch"
                            else
                            {
                                this_client.Send(Encoding.Default.GetBytes("You could not request a rematch. If you want to reuqest a rematch, please type rematch."));
                            }
                        }
                    }
                    else if (incomingMessage == "Rematch")
                    {
                        // Only process rematch request if game is over
                        if (!game && !rematchInProgress)
                        {
                            rematchRequests++;
                            if (rematchRequests == 2)
                            {
                                rematchInProgress = true;
                                rematch();
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
                        PlayerSymbol disconnectedPlayerSymbol = player.symbol;
                        logs.AppendText("A client has disconnected\n");

                        this_client.Close();
                        socket_player.Remove(this_client);
                        active_players.Remove(this_client);

                        if (game && active_players.Count == 1)
                        {
                            game = false;
                            var remainingPlayer = active_players.First();
                            remainingPlayer.Key.Send(Encoding.Default.GetBytes("The other player has disconnected. Checking the waiting list for other players...."));

                            // Assign the turn to the remaining player
                            currentPlayerSocket = remainingPlayer.Key;
                            currentPlayerSymbol = remainingPlayer.Value.symbol;

                            if (waiting_players.Count > 0) // If there's a player in the waiting room
                            {
                                // Getting the next player from the waiting room
                                KeyValuePair<Socket, Player> nextPlayerPair = waiting_players.First(); // Get the first player from the waiting room
                                Socket nextPlayerSocket = nextPlayerPair.Key;
                                Player nextPlayer = nextPlayerPair.Value;

                                // Resetting player's score
                                nextPlayer.ResetStats();

                                active_players.Add(nextPlayerSocket, nextPlayer); // Add this player to active players
                                waiting_players.Remove(nextPlayerSocket); // Remove this player from the waiting room

                                // Determine the next player's symbol, it should be different from the remaining player's symbol
                                nextPlayer.symbol = disconnectedPlayerSymbol;

                                // Starting a new game with the newly active player
                                start_game();

                                // New part: allow the remaining player to take a turn
                                game = true; // Ensure that the game state is active
                                current_player_idx = 0; // Reset current player index
                                currentPlayerSymbol = (PlayerSymbol)current_player_idx; // Reset current player symbol

                                // Send the message to the newly joined player from the waiting room
                                nextPlayerSocket.Send(Encoding.Default.GetBytes("You have been added to the game. It is now your turn."));
                            }
                        }

                        connected = false;
                    }
                }

            }
        }

        private void sendStats()
        {
            foreach (KeyValuePair<Socket, Player> pair in active_players)
            {
                string stats = "\nStatistics:\n" +
                    pair.Value.Username + ":\n" +
                    "Games Played: " + pair.Value.GamesPlayed + "\n" +
                    "Wins: " + pair.Value.Wins + "\n" +
                    "Losses: " + pair.Value.Losses + "\n" +
                    "Draws: " + pair.Value.Draws;
                pair.Key.Send(Encoding.Default.GetBytes(stats));
            }
        }

        private void rematch()
        {
            // Reset game variables
            rematchRequests = 0;
            rematchInProgress = false;
            game = true;
            board = new Board(); // Assuming that this resets the board

            // Send a message to all players that a new game is starting
            foreach (KeyValuePair<Socket, Player> pair in active_players)
            {
                pair.Key.Send(Encoding.Default.GetBytes("A new game is starting!"));
            }

            // Start the new game
            start_game();
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
                    // Reset the board and rematch requests
                    board = new Board();
                    foreach (var player in active_players.Values)
                    {
                        player.WantsRematch = false;
                    }
                    string log_text = "Game started with " + active_players.Count + " players.\n";
                    logs.AppendText(log_text);
                    foreach (KeyValuePair<Socket, Player> pair in active_players)
                    {
                        pair.Key.Send(Encoding.Default.GetBytes(log_text));
                       
                    }


                    Random random = new Random();
                    current_player_idx = random.Next(0, 2);
                    currentPlayerSymbol = (PlayerSymbol)current_player_idx;
                    // Get the current player socket
                    KeyValuePair<Socket, Player> current_player = new List<KeyValuePair<Socket, Player>>(active_players)[current_player_idx];
                    currentPlayerSocket = current_player.Key;

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