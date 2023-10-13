using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe_Server
{
    public class Player
    {
        public string Username { get; set; }
        public int Answer { get; set; }
        public PlayerSymbol symbol { get; set; }
        public bool AnswerReceived { get; set; }
        public double Score { get; set; }

        public bool WantsRematch { get; set; }

        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }

        public Player(string username = "")
        {
            this.Username = username;
            symbol = PlayerSymbol.X;
            AnswerReceived = false;
            Score = 0;
            GamesPlayed = 0;
            Wins = 0;
            Losses = 0;
            Draws = 0;
        }

        public bool has_username()
        {
            return this.Username != "";
        }
        // Inside the Player class
        public void ResetStats()
        {
            this.Wins = 0;
            this.Losses = 0;
            this.Draws = 0;
            //... reset any other stats as needed
        }

    }
}
