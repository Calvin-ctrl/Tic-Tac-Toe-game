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

        public Player(string username = "")
        {
            this.Username = username;
            symbol = PlayerSymbol.X;
            AnswerReceived = false;
            Score = 0;
        }

        public bool has_username()
        {
            return this.Username != "";
        }

    }
}
