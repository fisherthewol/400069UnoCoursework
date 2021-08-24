using System;

namespace UnoCoursework
{
    public enum CardColour
    {
        Blue = 0,
        Green = 1,
        Red = 2,
        Yellow = 3
    }

    public class StandardCard : Card
    {
        protected CardColour mColour;
        public CardColour Colour
        {
            get { return this.mColour; }
        }

        public StandardCard(CardColour pColour): this("StandardCard", pColour) { }
        public StandardCard(string pCardType, CardColour pColour) : base(pCardType)
        {
            this.mColour = pColour;
        }
    }
}