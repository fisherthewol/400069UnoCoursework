using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UnoCoursework
{
    public class NumberCard : StandardCard, IXmlSerializable
    {
        private int mValue;
        public int Value
        {
            get { return this.mValue; }
        }

        public NumberCard() : this(CardColour.Blue, 0) { }
        public NumberCard(CardColour pColour, int pValue) : this("NumberCard", pColour, pValue) { }
        public NumberCard(string pCardType, CardColour pColour, int pValue) : base(pCardType, pColour)
        {
            this.mValue = pValue;
        }
        
        public override string ToString()
        {
            return $"{this.mColour.ToString()} {this.mValue}";
        }

        /// <summary>
        /// Checks if this card can be played on top of a given card.
        /// </summary>
        /// <param name="pDrawDeckCard">Card to check.</param>
        /// <param name="pCurrentColour">Represents the current colour that can be played.</param>
        /// <param name="pCurrentValue">Represents the current value that can be played.</param>
        /// <returns>Whether the cards can be played.</returns>
        public override bool CanBePlayedOnDeck(Card pDrawDeckCard, CardColour pCurrentColour, string pCurrentValue)
        {
            switch (pDrawDeckCard)
            {
                case WildCard tCard:
                    return this.mColour == pCurrentColour || (this.mValue.ToString() == pCurrentValue);
                case AbilityCard tCard:
                    return this.mColour == tCard.Colour;
                case NumberCard tCard:
                    return (this.mColour == tCard.Colour) || (this.mValue == tCard.mValue);
                default:
                    return false;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter Writer)
        {
            Writer.WriteStartElement("NumberCard");
            Writer.WriteElementString("Colour", this.Colour.ToString());
            Writer.WriteElementString("Value", this.Value.ToString());
            Writer.WriteEndElement();
        }

        public void ReadXml(XmlReader Reader)
        {
            Reader.MoveToContent();
            bool isEmptyElement = Reader.IsEmptyElement;
            Reader.ReadStartElement("NumberCard", "");
            if (!isEmptyElement)
            {
                // Read Colour node and convert to CardColour.
                string colourString = Reader.ReadElementContentAsString("Colour", "");
                bool colourSuccess = Enum.TryParse(colourString, out this.mColour);
                if(!colourSuccess) { throw new InvalidDataException("Xml in Invalid Format; colour not found"); }

                this.mValue = Reader.ReadElementContentAsInt("Value", "");
                Reader.ReadEndElement();
            }
        }
    }
}