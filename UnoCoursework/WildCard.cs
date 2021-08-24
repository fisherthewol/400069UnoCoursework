using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UnoCoursework
{
    public class WildCard : Card, IXmlSerializable
    {
        private CardAbility mAbility;
        public CardAbility Ability
        {
            get { return this.mAbility; }
        }

        public WildCard() : this("WildCard", CardAbility.WildCard) { }
        public WildCard(CardAbility pAbility) : this("WildCard", pAbility) { }
        public WildCard(string pCardType, CardAbility pAbility) : base(pCardType)
        {
            this.mAbility = pAbility;
        }

        public override string ToString()
        {
            return this.mAbility.ToString();
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
            return true;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter Writer)
        {
            Writer.WriteStartElement("WildCard");
            Writer.WriteElementString("Ability", this.Ability.ToString());
            Writer.WriteEndElement();
        }

        public void ReadXml(XmlReader Reader)
        {
            Reader.MoveToContent();
            bool isEmptyElement = Reader.IsEmptyElement;
            Reader.ReadStartElement("WildCard", "");
            if (!isEmptyElement)
            {
                // Read Ability node and convert to CardAbility.
                string abilityString = Reader.ReadElementContentAsString("Ability", "");
                bool abilitySuccess = Enum.TryParse(abilityString, out this.mAbility);
                if (!abilitySuccess) { throw new InvalidDataException("Xml in Invalid Format; ability not found."); }
                Reader.ReadEndElement();
            }
        }
    }
}
