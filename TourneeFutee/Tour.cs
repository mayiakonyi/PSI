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

        // Constructeur utilisé par les tests de persistance
        public Tour(float cost, List<string> vertices)
        {
            this.segments = new List<(string, string)>();
            // On ignore graph ici car le coût est passé directement
            this.coutManuel = cost;

            // On transforme la liste de sommets en segments (A, B, C -> A-B, B-C, C-A)
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                this.AddSegment(vertices[i], vertices[i + 1]);
            }

            // On ferme la boucle si on a au moins deux sommets
            if (vertices.Count > 1)
            {
                this.AddSegment(vertices[vertices.Count - 1], vertices[0]);
            }
        }

        // ajoute un segment dans la tournee
        public void AddSegment(string source, string destination)
        {
            segments.Add((source, destination));
        }

        // proprietes

        // Cout total de la tournee
        private float coutManuel = -1;

        public float Cost
        {
            get
            {
                // Si un coût a été fixé manuellement (chargement BdD), on le renvoie
                if (coutManuel >= 0) return coutManuel;

                // Sinon, on calcule classiquement via le graphe
                float total = 0;
                foreach (var s in segments)
                {
                    total += graph.GetEdgeWeight(s.source, s.destination);
                }
                return total;
            }
            set { coutManuel = value; } // Permet de fixer le coût lors du LoadTour
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

        // Renvoie la liste ordonnée des sommets de la tournée
        public List<string> Vertices
        {
            get
            {
                List<string> liste = new List<string>();
                foreach (var s in segments)
                {
                    liste.Add(s.source);
                }
                return liste;
            }
        }
    }
}