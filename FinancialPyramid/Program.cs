using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace FinancialPyramid
{
    internal class Program
    {
        private static readonly XDocument documentPiramida = XDocument.Load(Directory.GetCurrentDirectory() + "\\piramida.xml");
        private static readonly XDocument documentPrzelewy = XDocument.Load(Directory.GetCurrentDirectory() + "\\przelewy.xml");
        private static Dictionary<int, int> commisions;
        private const string nameAttributeId = "id";
        private const string nameParticipant = "uczestnik";
        private const string nameTransfer = "przelew";
        private const string nameFrom = "od";
        private const string nameAmount = "kwota";
        static void Main(string[] args)
        {
            IEnumerable<XElement> participantCollection = from element in documentPiramida.Root.Descendants(nameParticipant) select element;
            IEnumerable<XElement> transferCollection = from element in documentPrzelewy.Root.Descendants(nameTransfer) select element;
            commisions = CalculateCommisions(participantCollection, transferCollection);

            foreach (XElement item in participantCollection)
                Console.WriteLine($"{GetParticipantId(item)} {GetParticipantDepth(item)} {GetParticipantsWithoutSubparticipants(item).Count()} {commisions[GetParticipantId(item)]}");
            Console.Read();
        }
        
        private static IEnumerable<XElement> GetParticipantsWithoutSubparticipants(XElement participant) => participant.Descendants().Where(e => e.Descendants().Count() == 0);
        
        private static int GetParticipantId(XElement participant) => (int)participant.Attribute(nameAttributeId);
        
        private static int GetParticipantDepth(XElement participant) => participant.Ancestors(nameParticipant).Count();
        
        private static Dictionary<int, int> CalculateCommisions(IEnumerable<XElement> participantCollection, IEnumerable<XElement> transferCollection)
        {
            Dictionary<int, int> _commisions = new Dictionary<int, int>();
            foreach (XElement item in participantCollection)
                _commisions.Add(GetParticipantId(item), 0);
            foreach (XElement item in transferCollection)
            {
                int amount = (int)item.Attribute(nameAmount);
                IEnumerable<XElement> beneficiaries = participantCollection.Where(e => GetParticipantId(e) == (int)item.Attribute(nameFrom)).Ancestors(nameParticipant).Reverse();
                foreach (XElement item2 in beneficiaries)
                {
                    _commisions[GetParticipantId(item2)] += beneficiaries.Last() == item2 ? amount : amount / 2;
                    amount /= 2;
                }
            }
            return _commisions;
        }
    }
}
