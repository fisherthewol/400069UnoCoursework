using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UnoCoursework
{
    class GameState : IXmlSerializable
    {
        private static int MinPlayers = 2;
        private static int MaxPlayers = 10;
        private List<Player> mPlayers;
        private Deck mDrawDeck;
        private Deck mDropDeck;
        private bool mForwardDirection;
        private int mCurrentPlayer;
        private CardColour mCurrentColour;
        private string mCurrentValue;
        
        public GameState() : this(new List<Player>(), new Deck("DrawDeck", false), new Deck("DropDeck", false), true)
        {
        }

        public GameState(List<Player> pPlayers, Deck pDrawPile, Deck pDropPile, bool pForwardDirection)
        {
            this.mPlayers = pPlayers;
            this.mDrawDeck = pDrawPile;
            this.mDropDeck = pDropPile;
            this.mForwardDirection = pForwardDirection;
            this.mCurrentPlayer = 0;
            if (this.mDropDeck.Cards.Count != 0)
            {
                Card curCard = this.mDropDeck.Cards[0];
                switch (curCard)
                {
                    case AbilityCard tCard:
                        this.mCurrentColour = tCard.Colour;
                        break;
                    case NumberCard tCard:
                        this.mCurrentColour = tCard.Colour;
                        this.mCurrentValue = tCard.Value.ToString();
                        break;
                    default:
                        this.mCurrentColour = CardColour.Blue;
                        break;
                }
            }
            else
            {
                this.mCurrentColour = CardColour.Blue;
                this.mCurrentValue = "any";
            }
        }

        /// <summary>
        /// Prepares a GameState for a new game.
        /// </summary>
        public void FillNewGame()
        {
            this.mDrawDeck = new Deck(true);

            int playerCount = Program.IntegerFromOptions($"How many players? ({MinPlayers} - {MaxPlayers})", '.', MinPlayers, MaxPlayers);
            for (int i = 0; i < playerCount; i++)
            {
                Console.WriteLine($"Enter name for player {i}:");
                string name = Console.ReadLine();
                Deck currentHand = new Deck(false);
                for (int card = 0; card < 7; card++)
                {
                    Card currentCard = this.mDrawDeck.Pop(0);
                    currentHand.Add(currentCard);
                }

                Player newPlayer = new Player(name, currentHand);
                this.mPlayers.Add(newPlayer);
            }
        }

        /// <summary>
        /// Returns next player's index.
        /// </summary>
        private int NextPlayer()
        {
            int nextPlayer = this.mCurrentPlayer;
            if (this.mForwardDirection)
            {
                nextPlayer++;
                if (nextPlayer >= this.mPlayers.Count)
                {
                    return 0;
                }
            }
            else
            {
                nextPlayer--;
                if (nextPlayer < 0)
                {
                    return this.mPlayers.Count - 1;
                }
            }
            return nextPlayer;
        }

        /// <summary>
        /// Take turn for the player that is mCurrentPlayer.
        /// </summary>
        /// <returns>Integer representing outcome of the turn.</returns>
        private int TakePlayerTurn()
        {
            int status = int.MinValue;
            Player CurrentPlayer = this.mPlayers[mCurrentPlayer];
            string choices = CurrentPlayer.GenerateChoices();

            Console.Clear();
            Console.WriteLine($"Turn for Player {this.mPlayers[mCurrentPlayer].Name}:");

            Console.WriteLine($"Current colour is {this.mCurrentColour} and current value is {this.mCurrentValue}.");
            int playerChoice = Program.IntegerFromOptions(choices, ',', 0, CurrentPlayer.Hand.Cards.Count + 1);
            if (playerChoice >= CurrentPlayer.Hand.Cards.Count)
            {
                playerChoice -= CurrentPlayer.Hand.Cards.Count;
                if (playerChoice >= 0)
                {
                    return playerChoice;
                }
                else
                {
                    throw new ArithmeticException("Developer error in Gamestate.TakePlayerTurn().");
                }
            }
            else
            {
                status = 2;
                Card chosenCard = CurrentPlayer.Hand.Cards[playerChoice];
                if (this.mDropDeck.Cards.Count == 0)
                {
                    this.mDropDeck.Add(chosenCard);
                }
                else
                {
                    if (chosenCard.CanBePlayedOnDeck(this.mDropDeck.Cards[this.mDropDeck.TopCardIndex], this.mCurrentColour, this.mCurrentValue))
                    {
                        this.mDropDeck.Add(chosenCard);
                    }
                    else
                    {
                        Console.WriteLine("Card cannot be played on this deck. Try Again.");
                        System.Threading.Thread.Sleep(2000);
                        return 3;
                    }
                }

                if (chosenCard is AbilityCard || chosenCard is WildCard)
                {
                    this.HandleCardAbility(chosenCard);
                }

                if (chosenCard is NumberCard)
                {
                    NumberCard card = chosenCard as NumberCard;
                    this.mCurrentColour = card.Colour;
                    this.mCurrentValue = card.Value.ToString();
                }
                CurrentPlayer.Hand.RemoveCard(chosenCard);
            }
            return status;
        }

        /// <summary>
        /// If card has an ability, take the relevant action for it.
        /// </summary>
        /// <param name="pChosenCard">Card to handle ability for.</param>
        private void HandleCardAbility(Card pChosenCard)
        {
            dynamic tCard = null;
            switch (pChosenCard)
            {
                case AbilityCard aCard:
                    tCard = aCard;
                    break;
                case WildCard wCard:
                    tCard = wCard;
                    break;
            }

            switch (tCard.Ability)
            {
                case CardAbility.TurnMiss:
                    this.mCurrentPlayer = this.NextPlayer();
                    break;
                case CardAbility.TakeTwo:
                    this.TakeCards(2, this.mPlayers[this.NextPlayer()]);
                    break;
                case CardAbility.ChangeDirection:
                    this.ChangeDirection();
                    break;
                case CardAbility.WildCard:
                    this.ChangeColour();
                    break;
                case CardAbility.TakeFour:
                    this.TakeCards(4, this.mPlayers[this.NextPlayer()]);
                    break;
            }
        }

        /// <summary>
        /// Inverts Direction of play.
        /// </summary>
        private void ChangeDirection()
        {
            this.mForwardDirection = !this.mForwardDirection;
        }

        /// <summary>
        /// Adds cards from DrawDeck to a player.
        /// </summary>
        /// <param name="pCount">Number of cards to take.</param>
        /// <param name="targetPlayer">Player to add cards to.</param>
        private void TakeCards(int pCount, Player targetPlayer)
        {
            for (int i = 0; i < pCount; i++)
            {
                targetPlayer.Hand.Add(this.mDrawDeck.Pop(0));
            }
        }

        /// <summary>
        /// Change current playable colour. Used when a wildcard is played.
        /// </summary>
        private void ChangeColour()
        {
            Console.WriteLine("Choose a colour to change to:");
            string options = "0. Blue,1. Green,2. Red,3. Yellow";
            int choice = Program.IntegerFromOptions(options, ',', 0, 3);
            this.mCurrentColour = (CardColour)choice;
            this.mCurrentValue = "any";
        }

        /// <summary>
        /// Main Game loop. Enables game to run.
        /// </summary>
        /// <returns>Integer representing if a player has won or if a player has chosen to save the game and exit.</returns>
        public int RunGameLoop()
        {
            int status = int.MinValue;
            while (true)
            {
                int choice = this.TakePlayerTurn();
                foreach (Player CurrentPlayer in this.mPlayers)
                {
                    if (CurrentPlayer.Hand.Cards.Count == 0/* || CurrentPlayer.Score >= 500*/)
                    {
                        this.WinningPlayer(CurrentPlayer);
                        status = 0;
                        return status;
                    }
                }
                
                switch (choice) // Should be using an enum for this but integer for now.
                {
                    case 0: // MyEnum.TakeCard
                        this.TakeCards(1, this.mPlayers[mCurrentPlayer]);
                        break;
                    case 1: // MyEnum.SaveAndExit
                        return choice;
                    case 2: //MyEnum.CardPlayed
                        this.mCurrentPlayer = this.NextPlayer();
                        break;
                    case 3: //MyEnum.InvalidCard
                        break;
                }
            }
        }

        /// <summary>
        /// Handles displaying scores when a player has won.
        /// </summary>
        /// <param name="winningPlayer">Player who has won.</param>
        private void WinningPlayer(Player winningPlayer)
        {
            Console.Clear();
            Console.WriteLine($"Congratulations, {winningPlayer.Name}. You won!");
            Console.WriteLine("The Players in order of score are:");
            List<Player> tempPlayers = new List<Player>(this.mPlayers);
            tempPlayers.Sort();
            for(int i = 1; i <= tempPlayers.Count; i++)
            {
                Console.WriteLine($"{i}. {tempPlayers[i-1].ToString()}");
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter Writer)
        {
            Writer.WriteStartElement("GameState");
            Writer.WriteElementString("ForwardDirection", this.mForwardDirection.ToString().ToLower());
            Writer.WriteElementString("CurrentPlayer", this.mCurrentPlayer.ToString());
            Writer.WriteElementString("CurrentColour", this.mCurrentColour.ToString());
            Writer.WriteElementString("CurrentValue", this.mCurrentValue);

            XmlSerializer DeckSerializer = new XmlSerializer(typeof(Deck));
            DeckSerializer.Serialize(Writer, this.mDropDeck);
            DeckSerializer.Serialize(Writer, this.mDrawDeck);

            XmlSerializer PlayerSerializer = new XmlSerializer(typeof(Player));
            foreach (Player player in this.mPlayers)
            {
                PlayerSerializer.Serialize(Writer, player);
            }
            Writer.WriteFullEndElement();
        }

        public void ReadXml(XmlReader Reader)
        {
            this.mPlayers.Clear();
            Reader.MoveToContent();
            bool isEmptyElement = Reader.IsEmptyElement;
            Reader.ReadStartElement("GameState", "");
            if (!isEmptyElement)
            {
                this.mForwardDirection = Reader.ReadElementContentAsBoolean("ForwardDirection", "");
                this.mCurrentPlayer = Reader.ReadElementContentAsInt("CurrentPlayer", "");
                // try to read current colour.
                string ColourString = Reader.ReadElementContentAsString("CurrentColour", "");
                bool ColourSuccess = Enum.TryParse(ColourString, out this.mCurrentColour);
                if (!ColourSuccess) { throw new InvalidDataException("Current Colour not found; xml is in invalid format."); }

                this.mCurrentValue = Reader.ReadElementContentAsString("CurrentValue", "");
                // Deserialize decks.
                XmlSerializer DeckSerializer = new XmlSerializer(typeof(Deck));
                this.mDropDeck = (Deck)DeckSerializer.Deserialize(Reader);
                this.mDrawDeck = (Deck)DeckSerializer.Deserialize(Reader);
                //Deserialize Players.
                XmlSerializer PlayerSerializer = new XmlSerializer(typeof(Player));
                Reader.ReadStartElement();
                while((!Reader.EOF) && (Reader.LocalName == "Player"))
                {
                    Player tempPlayer = (Player)PlayerSerializer.Deserialize(Reader);
                    this.mPlayers.Add(tempPlayer);
                }
                Reader.ReadEndElement();
            }
        }
    }
}
