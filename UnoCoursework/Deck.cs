using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UnoCoursework
{
    public class Deck : IXmlSerializable
    {
        private string mName;
        private List<Card> mCards;
        private int mTopCardIndex;

        public string Name
        {
            get { return this.mName; }
        }

        public List<Card> Cards
        {
            get { return this.mCards; }
        }

        public int TopCardIndex
        {
            get { return mTopCardIndex; }
        }

        public Deck() : this(false) { }
        public Deck(bool pFresh) : this("BlankDeck", new List<Card>(), pFresh) { }
        public Deck(string pName, bool pFresh) : this(pName, new List<Card>(), pFresh) { }
        public Deck(string pName, List<Card> pCards, bool pFresh)
        {
            this.mName = pName;
            this.mCards = pCards;
            if (pFresh) { this.InitialiseDeck(); }
            this.mTopCardIndex = this.mCards.Count - 1;
        }

        /// <summary>
        /// Initialises a shuffled standard UNO deck.
        /// </summary>
        /// <returns>Boolean representing success of action.</returns>
        private bool InitialiseDeck()
        {
            // For each colour, create 10 number cards, and 2 ability cards per ability.
            for (int colour = 0; colour < 4; colour++)
            {
                // Generate number cards.
                for (int value = 0; value < 10; value++)
                {
                    NumberCard curCard = new NumberCard((CardColour)colour, value);
                    this.mCards.Add(curCard);
                }
                // Generate ability cards.
                for (int ability = 0; ability < 3; ability++)
                {
                    for (int count = 0; count < 2; count++) // Done in loop to allow for changing number of ability cards per deck.
                    {
                        AbilityCard abilityCard = new AbilityCard((CardColour)colour, (CardAbility)ability);
                        this.mCards.Add(abilityCard);
                    }
                }
            }
            // Create wild cards.
            for (int ability = 3; ability < 5; ability++)
            {
                for (int count = 0; count < 4; count++)
                {
                    WildCard wildCard = new WildCard((CardAbility)ability);
                    this.mCards.Add(wildCard);
                }
            }
            this.ShuffleDeck();
            return true;
        }

        /// <summary>
        /// Shuffles deck using a Knuth (Fisher-Yates) shuffle.
        /// </summary>
        /// <returns>Boolean representing success of action.</returns>
        private bool ShuffleDeck()
        // Code is adapted from https://www.rosettacode.org/wiki/Knuth_shuffle
        {
            System.Random randomGen = new System.Random();
            randomGen.Next();
            for (int i = 0; i < this.mCards.Count; i++)
            {
                int j = randomGen.Next(i, this.mCards.Count);
                Card temp = this.mCards[i];
                this.mCards[i] = this.mCards[j];
                this.mCards[j] = temp;
            }
            return true;
        }

        /// <summary>
        /// Adds a card to the Deck and updates the "top" card's index appropriately.
        /// </summary>
        /// <param name="pCurrentCard">Card to add to the deck.</param>
        public void Add(Card pCurrentCard)
        {
            this.mCards.Add(pCurrentCard);
            this.mTopCardIndex = this.mCards.Count - 1;
        }

        /// <summary>
        /// Removes Card from the deck at a given index and returns said Card to caller.
        /// </summary>
        /// <param name="pIndex">Index to pop from deck.</param>
        /// <returns>Card popped from deck.</returns>
        public Card Pop(int pIndex)
        {
            if (pIndex >= this.Cards.Count)
            {
                throw new ArgumentException("pIndex greater than number of cards in deck.");
            }
            Card returnCard = this.mCards[pIndex];
            this.mCards.RemoveAt(pIndex);
            this.mTopCardIndex = this.mCards.Count - 1;
            return returnCard;
        }

        /// <summary>
        /// Removes a card from the deck.
        /// </summary>
        /// <param name="pCard">Card to remove from Deck.</param>
        /// <returns>Boolean representing success of action.</returns>
        public bool RemoveCard(Card pCard)
        {
            return this.mCards.Remove(pCard);
        }

        public override string ToString()
        {
            StringBuilder returnString = new StringBuilder();
            for (int i = 0; i < this.mCards.Count; i++)
            {
                Card card = this.mCards[i];
                returnString.Append($"{i}. {card.ToString()},");
            }
            return returnString.ToString();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter Writer)
        {
            Writer.WriteStartElement("Deck");
            Writer.WriteAttributeString("name", this.Name);
            XmlSerializer wildSerializer = new XmlSerializer(typeof(WildCard));
            XmlSerializer abilitySerializer = new XmlSerializer(typeof(AbilityCard));
            XmlSerializer numberSerializer = new XmlSerializer(typeof(NumberCard));
            foreach (Card card in this.Cards)
            {
                switch (card)
                {
                    case WildCard tCard:
                        wildSerializer.Serialize(Writer, tCard);
                        break;
                    case AbilityCard tCard:
                        abilitySerializer.Serialize(Writer, tCard);
                        break;
                    case NumberCard tCard:
                        numberSerializer.Serialize(Writer, tCard);
                        break;
                }
            }
            Writer.WriteEndElement();
        }

        public void ReadXml(XmlReader Reader)
        {
            this.mCards.Clear();
            Reader.MoveToContent();
            bool isEmptyElement = Reader.IsEmptyElement;
            Reader.ReadStartElement("Deck","");
            if (!isEmptyElement)
            {
                XmlSerializer wildSerializer = new XmlSerializer(typeof(WildCard));
                XmlSerializer abilitySerializer = new XmlSerializer(typeof(AbilityCard));
                XmlSerializer numberSerializer = new XmlSerializer(typeof(NumberCard));
                Reader.ReadStartElement();
                while (!Reader.EOF && (Reader.LocalName == "AbilityCard" || Reader.LocalName == "NumberCard" || Reader.LocalName == "WildCard")) 
                {
                    switch (Reader.LocalName)
                    {
                     case "AbilityCard":
                         AbilityCard ACard = (AbilityCard)abilitySerializer.Deserialize(Reader);
                         this.Add(ACard);
                         break;
                     case "NumberCard":
                         NumberCard NCard = (NumberCard)numberSerializer.Deserialize(Reader);
                         this.Add(NCard);
                         break;
                     case "WildCard":
                         WildCard WCard = (WildCard)wildSerializer.Deserialize(Reader);
                         this.Add(WCard);
                         break;
                    }
                }
                Reader.ReadEndElement();
            }
        }
    }
}