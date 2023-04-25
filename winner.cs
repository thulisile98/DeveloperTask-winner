using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

interface ICard
{
    int Suit { get; set; }
    int Value { get; set; }
}

interface IPlayer
{
    string Name { get; set; }
    ICard[] Cards { get; set; }
    int SuitScore { get; set; }
    int Score { get; set; }
}

class Card : ICard
{
    public int Suit { get; set; }
    public int Value { get; set; }
    public Card(string cardString)
    {
        string valueString = cardString.Substring(0, cardString.Length - 1);
        string suitString = cardString.Substring(cardString.Length - 1);

        switch (valueString.ToUpper())
        {
            case "A": Value = 11; break;
            case "J": Value = 11; break;
            case "Q": Value = 12; break;
            case "K": Value = 13; break;
            default: Value = int.Parse(valueString); break;
        }

        switch (suitString.ToUpper())
        {
            case "C": Suit = 1; break;
            case "D": Suit = 2; break;
            case "H": Suit = 3; break;
            case "S": Suit = 4; break;
            default: throw new ArgumentException("Invalid card suit.");
        }
    }
}

class Player : IPlayer
{
    public string Name { get; set; }
    public ICard[] Cards { get; set; }
    public int SuitScore { get; set; }
    public int Score { get; set; }

    public Player(string name, ICard[] cards)
    {
        Name = name;
        Cards = cards;
        SuitScore = cards.Aggregate(1, (acc, c) => acc * c.Suit);
        Score = cards.Sum(c => c.Value);
    }
}

class Program
{
    static void Main(string[] args)
    {
        string inputFileName = null;
        string outputFileName = null;
        // Parse the command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--in" && i < args.Length - 1)
            {
                inputFileName = args[i + 1];
            }
            else if (args[i] == "--out" && i < args.Length - 1)
            {
                outputFileName = args[i + 1];
            }
        }

        // Check if the input and output file names are valid
        if (string.IsNullOrEmpty(inputFileName) || string.IsNullOrEmpty(outputFileName))
        {
            Console.WriteLine("Error: Invalid command line arguments. Usage: winner.exe --in input.txt --out output.txt");
            return;
        }

        try
        {
            // Read the input file
            string[] inputLines = File.ReadAllLines(inputFileName);
            //checks if The input file must contain exactly 5 players
            if(inputLines.Length !=5)
            {
                Console.WriteLine("Error: Invalid input file. The input file must contain exactly 5 lines.");
                  return;
            }
            
            List<IPlayer> players = new List<IPlayer>();
            foreach (string line in inputLines)
            {
                string[] parts = line.Split(':');
                string playerName = parts[0].Trim();
                string[] cardStrings = parts[1].Split(',');
                
        // checks that each line in the input file contains exactly 5 cards before creating a new Player object
                 if (cardStrings.Length != 5)
                  {
                   Console.WriteLine(playerName+" does not have exactly 5 cards.");
                    return;
                  }
                ICard[] cards = cardStrings.Select(s => (ICard)new Card(s.Trim())).ToArray();
                IPlayer player = new Player(playerName, cards);
                players.Add(player);
            }

            // Find the winners
            int maxScore = players.Max(p => p.Score);
            var winners = players.Where(p => p.Score == maxScore);

            if (winners.Count() > 1)
            {
                // Recalculate scores for tied players based on suit score
                var tiedPlayers = winners.ToList();
                foreach (var player in tiedPlayers)
                {
                    player.SuitScore = player.Cards.Aggregate(1, (acc, c) => acc * c.Suit);
                }

                // Find the winner based on suit score
                int maxSuitScore = tiedPlayers.Max(p => p.SuitScore);
                winners = tiedPlayers.Where(p => p.SuitScore == maxSuitScore);
            }

            // Writing the result to the output file
            using (StreamWriter writer = new StreamWriter(outputFileName))
            {
                if (winners.Count() == 1)
                {
                    IPlayer winner = winners.First();
                    writer.WriteLine(winner.Name + ":" + winner.Score);
                }
                else
                {
                    string winnerNames = string.Join(",", winners.Select(p => p.Name));
                    int suitScore = players.Where(p => winners.Contains(p)).Max(p => p.SuitScore);
                    writer.WriteLine(winnerNames + ":" + (suitScore));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:" + ex.Message);
        }
    }
}


