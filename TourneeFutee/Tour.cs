using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    // Modelise une tournee dans le cadre du probleme du voyageur de commerce
    public class Tour
    {
        // liste des segments de la tournee
        private List<(string source, string destination)> segments;

        // reference au graphe pour recuperer les poids
        private Graph graph;

        // constructeur
        public Tour(Graph g)
        {
            graph = g; // sauvegarde graphe
            segments = new List<(string, string)>(); // init liste segments
        }

        // ajoute un segment dans la tournee
        public void AddSegment(string source, string destination)
        {
            segments.Add((source, destination));
        }

        // proprietes

        // Cout total de la tournee
        public float Cost
        {
            get
            {
                float total = 0;

                foreach (var s in segments)
                {
                    total += graph.GetEdgeWeight(s.source, s.destination);
                }

                return total;
            }
        }

        // Nombre de trajets dans la tournee
        public int NbSegments
        {
            get
            {
                return segments.Count;
            }
        }

        // Renvoie vrai si la tournee contient le trajet source->destination
        public bool ContainsSegment((string source, string destination) segment)
        {
            foreach (var s in segments)
            {
                if (s.source == segment.source && s.destination == segment.destination)
                    return true;
            }

            return false;
        }

        // Affiche les informations sur la tournee : cout total et trajets
        public void Print()
        {
            Console.WriteLine("Cout total : " + Cost);

            foreach (var s in segments)
            {
                Console.WriteLine(s.source + " -> " + s.destination);
            }
        }

        // renvoie la liste des segments
        public List<(string source, string destination)> GetSegments()
        {
            return new List<(string, string)>(segments);
        }
    }
}