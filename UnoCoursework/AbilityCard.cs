using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UnoCoursework
{
    public class AbilityCard : StandardCard, IXmlSerializable
    {
        private CardAbility mAbility;
        public CardAbility Ability
        {
            get { return this.mAbility; }
        }
        
        public AbilityCard() : this(CardColour.Blue, CardAbility.ChangeDirection) { }
        public AbilityCard(CardColour pColour, CardAbility pAbility) : this("AbilityCard", pColour, pAbility) { }
        public AbilityCard(string pCardType, CardColour pColour, CardAbility pAbility) : base(pCardType, pColour)
        {
            this.mAbility = pAbility;
        }

        public override string ToString()
        {
            return $"{this.mColour.ToString()} {this.mAbility.ToString()}";
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
                    return this.mColour == pCurrentColour;
                case AbilityCard tCard:
                    return this.mColour == tCard.Colour && this.mColour == pCurrentColour;
                case NumberCard tCard:
                    return this.mColour == tCard.Colour && this.mColour == pCurrentColour;
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
            Writer.WriteStartElement("AbilityCard");
            Writer.WriteElementString("Colour", this.Colour.ToString());
            Writer.WriteElementString("Ability", this.Ability.ToString());
            Writer.WriteEndElement();
        }

        public void ReadXml(XmlReader Reader)
        {
            Reader.MoveToContent();
            bool isEmptyElement = Reader.IsEmptyElement;
            Reader.ReadStartElement("AbilityCard","");
            if (!isEmptyElement)
            {
                // Read Colour node and convert to CardColour.
                string colourString = Reader.ReadElementContentAsString("Colour", "");
                bool colourSuccess = Enum.TryParse(colourString, out this.mColour);
                if(!colourSuccess) { throw new InvalidDataException("Xml in Invalid Format; colour not found"); }
                // Read Ability node and convert to CardAbility.
                string abilityString = Reader.ReadElementContentAsString("Ability", "");
                bool abilitySuccess = Enum.TryParse(abilityString, out this.mAbility);
                if (!abilitySuccess) { throw new InvalidDataException("Xml in Invalid Format; ability not found."); }
                Reader.ReadEndElement();
            }
        }
    }
}
