using System;

namespace UnoCoursework
{
    public enum CardAbility
    {
        TurnMiss = 0,
        TakeTwo = 1,
        ChangeDirection = 2,
        WildCard = 3,
        TakeFour = 4
    }
    public abstract class Card
    {
        protected string mCardType;
        public string CardType
        {
            get { return this.mCardType; }
        }

        public Card(): this("BaseCard") { }
        public Card(string pCardType)
        {
            this.mCardType = pCardType;
        }

        public override string ToString()
        {
            return this.mCardType;
        }

        public virtual bool CanBePlayedOnDeck(Card pDrawDeckCard, CardColour pCurrentColour, string pCurrentValue)
        {
            return false;
        }
    }
}