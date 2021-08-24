using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UnoCoursework
{
    public class Player : IXmlSerializable, IComparable
    {
        private string mName;
        private Deck mHand;
        private int mScore;

        public string Name
        {
            get { return this.mName; }
        }

        public Deck Hand
        {
            get { return this.mHand; }
        }

        public int Score
        {
            get { return this.mScore; }
        }

        public Player() : this("", new Deck()) { }
        public Player(string pName, Deck pHand, int pScore = 0)
        {
            this.mName = pName;
            this.mHand = pHand;
            this.mScore = pScore;
        }

        /// <summary>
        /// Generates the player's menu from their hand.
        /// </summary>
        /// <returns>Menu as a string of comma-seperated options.</returns>
        public string GenerateChoices()
        {
            string returnstring = "";
            returnstring += this.Hand.ToString();
            returnstring += $"{this.Hand.Cards.Count}. Take New Card,";
            returnstring += $"{this.Hand.Cards.Count + 1}. Save and Exit";
            return returnstring;
        }

        public override string ToString()
        {
            return $"{this.mName}: {this.mScore}";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter Writer)
        {
            Writer.WriteStartElement("Player");
            Writer.WriteElementString("Name", this.mName);
            Writer.WriteElementString("Score", this.mScore.ToString());
            XmlSerializer HandSerializer = new XmlSerializer(typeof(Deck));
            HandSerializer.Serialize(Writer, this.mHand);
            Writer.WriteEndElement();
        }

        public void ReadXml(XmlReader Reader)
        {
            Reader.MoveToContent();
            bool isEmptyElement = Reader.IsEmptyElement;
            Reader.ReadStartElement("Player", "");
            if (!isEmptyElement)
            {
                this.mName = Reader.ReadElementContentAsString("Name", "");
                this.mScore = Reader.ReadElementContentAsInt("Score", "");
                XmlSerializer HandSerializer = new XmlSerializer(typeof(Deck));
                this.mHand = (Deck)HandSerializer.Deserialize(Reader);
                Reader.ReadEndElement();
            }
        }

        public int CompareTo(object pObject)
        {
            switch (pObject)
            {
                case Player otherPlayer:
                    return this.Score - otherPlayer.Score;
                default:
                    return 0;
            }
        }
    }
}