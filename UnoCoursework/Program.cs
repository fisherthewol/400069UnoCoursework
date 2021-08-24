using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace UnoCoursework
{
    class Program
    {

        /// <summary>
        /// Gets response to a menu from user.
        /// </summary>
        /// <param name="pOptions">Menu options, delimited by pDelim.</param>
        /// <param name="pDelim">Delimiting character.</param>
        /// <param name="pLower">Inclusive lower bound.</param>
        /// <param name="pUpper">Inclusive upper bound.</param>
        /// <returns>Integer response from user.</returns>
        public static int IntegerFromOptions(string pOptions, char pDelim, int pLower, int pUpper)
        {
            if (pLower >= pUpper) { throw new ArgumentException("pLower is greater than pUpper; programmer has made a mistake."); }
            string[] menuOptions = pOptions.Split(pDelim);
            int response = int.MinValue;
            do
            {
                foreach(string option in menuOptions)
                {
                    if (option != "") { Console.WriteLine(option); }
                }
                string answer = Console.ReadLine();
                bool valid = int.TryParse(answer, out response);
                if (valid)
                {
                    if(response > pUpper || response < pLower)
                    {
                        Console.WriteLine("Response was out of range, try again!");
                    }
                }
                else { Console.WriteLine("Input was invalid, try again!"); }
            } while (response > pUpper || response < pLower);
            return response;
        }

        /// <summary>
        /// Saves/Serialises a GameState to a .xml file of the player's choice.
        /// </summary>
        /// <param name="saveGame">GameState to save.</param>
        /// <returns>Boolean representing success of action.</returns>
        private static bool SaveGame(GameState saveGame)
        {
            bool endsxml = false;
            string filename = "";
            do
            {
                Console.WriteLine("Enter a path to save to (ending .xml):");
                filename = Console.ReadLine();
                if (!filename.EndsWith(".xml")) { Console.WriteLine("That doesn't end .xml!"); }
                endsxml = true;
            } while (!endsxml);


            using (StreamWriter SaveWriter = new StreamWriter(filename))
            {
                XmlWriter xmlWriter = XmlWriter.Create(SaveWriter);
                xmlWriter.WriteStartDocument();
                saveGame.WriteXml(xmlWriter);
                xmlWriter.WriteEndDocument();
            }
            return true;
        }

        /// <summary>
        /// Deserializes a .xml file into a GameState, to resume play.
        /// </summary>
        /// <returns>A GameState representing a previously saved game.</returns>
        private static GameState LoadGameFromXml()
        {
            GameState loadedGame = new GameState();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml");
            Console.WriteLine("Select an xml file to load from:");
            int file = IntegerFromOptions(string.Join(",", files), ',', 0, files.Length - 1);
            string filename = files[file];
            using (StreamReader SaveReader = new StreamReader(filename))
            {
                XmlReader xmlReader = XmlReader.Create(SaveReader);
                try
                {
                    loadedGame.ReadXml(xmlReader);
                }
                catch (XmlException e)
                {
                    Console.WriteLine($"Error with loading file: {e.Message}. Exiting");
                    throw;
                }
            }
            return loadedGame;
        }

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Welcome to Uno!");
            Console.WriteLine("Make a choice:");
            int response = IntegerFromOptions("1. Start New Game.,2. Load from savefile.", ',', 1, 2);
            if (response == 1)
            {
                GameState newGame = new GameState(new List<Player>(), new Deck(true), new Deck(false), true);
                newGame.FillNewGame();
                int gameExit = newGame.RunGameLoop();
                switch (gameExit)
                {
                    case 0: // Player has won.
                        Console.WriteLine($"Thanks for Playing!");
                        break;
                    case 1: // Player has selected save and Exit.
                        bool success = false;
                        do
                        {
                            success = SaveGame(newGame);
                            if (!success)
                            {
                                Console.WriteLine("Error while saving game. Try again?");
                                int tryAgain = IntegerFromOptions("1. Yes,2. No", ',', 1, 2);
                                if (tryAgain == 1)
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Saving game was successful.");
                                success = true;
                                break;
                            }
                        } while (!success);
                        break;
                }
            }
            else if (response == 2)
            {
                GameState loadedGame = LoadGameFromXml();
                loadedGame.RunGameLoop();
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        
    }
}
