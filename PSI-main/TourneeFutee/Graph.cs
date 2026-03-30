using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Graph
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents
        private readonly bool direction; // indique si le graphe est oriente ou pas
        private readonly float pasarcval; // valeur pour signaler l'absence d'un arc
        private readonly Matrix matriceadj; // matrice d'adjacence pour stocker les arcs
        private readonly Dictionary<string, int> nomversindex; // associe un nom de sommet a son index
        private readonly List<float> stockvalsom; // stocke les valeurs des sommets
        private readonly List<string> indexversnom; // permet de retrouver le nom d'un sommet via son index

        // --- Construction du graphe ---
        // Contruit un graphe (directed=true => orienté)
        // La valeur noEdgeValue est le poids modélisant l'absence d'un arc (0 par défaut)
        public Graph(bool directed, float noEdgeValue = 0)
        {
            direction = directed; // sauvegarde si le graphe est oriente
            pasarcval = noEdgeValue; // sauvegarde valeur pour pas d'arc
            matriceadj = new Matrix(0, 0, noEdgeValue); // init matrice vide
            nomversindex = new Dictionary<string, int>(); // init dictionnaire noms->index
            stockvalsom = new List<float>(); // init liste valeurs sommets
            indexversnom = new List<string>(); // init liste noms par index
        }

        // --- Proprietes ---

        // Propriete : ordre du graphe
        // Lecture seule
        public int Order
        {
            get { return stockvalsom.Count; } // le nb de sommets
            // pas de set
        }

        // Propriete : graphe oriente ou non
        // Lecture seule
        public bool Directed
        {
            get { return direction; } // renvoie si le graphe est oriente
            // pas de set
        }

        // --- Gestion des sommets ---

        // Ajoute le sommet de nom name et de valeur value (0 par defaut) dans le graphe
        // Lève une ArgumentException s'il existe deja un sommet avec le meme nom dans le graphe
        public void AddVertex(string name, float value = 0)
        {
            if (nomversindex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet existe deja

            int newIndex = Order; // nouvel index = nb actuel de sommets

            nomversindex[name] = newIndex; // map nom->index
            stockvalsom.Add(value); // ajoute valeur sommet
            indexversnom.Add(name); // ajoute nom dans liste index->nom

            matriceadj.AddRow(newIndex); // agrandit matrice avec nouvelle ligne
            matriceadj.AddColumn(newIndex); // agrandit matrice avec nouvelle colonne
        }

        // Supprime le sommet de nom name du graphe (et tous les arcs associes)
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public void RemoveVertex(string name)
        {
            if (!nomversindex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet inexistant

            int index = nomversindex[name]; // recuperer index du sommet

            matriceadj.RemoveRow(index); // supprime ligne dans matrice
            matriceadj.RemoveColumn(index); // supprime colonne dans matrice

            stockvalsom.RemoveAt(index); // supprime valeur du sommet
            indexversnom.RemoveAt(index); // supprime nom du sommet
            nomversindex.Remove(name); // supprime mapping nom->index

            // mise a jour des index pour les sommets apres celui supprime
            for (int i = index; i < indexversnom.Count; i++)
            {
                nomversindex[indexversnom[i]] = i;
            }
        }

        // Renvoie la valeur du sommet de nom name
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public float GetVertexValue(string name)
        {
            if (!nomversindex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet inexistant

            return stockvalsom[nomversindex[name]]; // renvoie valeur
        }

        // Affecte la valeur du sommet de nom name a value
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public void SetVertexValue(string name, float value)
        {
            if (!nomversindex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet inexistant

            stockvalsom[nomversindex[name]] = value; // met a jour valeur
        }

        // Renvoie la liste des noms des voisins du sommet de nom vertexName
        // (si ce sommet n'a pas de voisins, la liste sera vide)
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public List<string> GetNeighbors(string vertexName)
        {
            if (!nomversindex.ContainsKey(vertexName))
                throw new ArgumentException(); // erreur si sommet inexistant

            List<string> neighborNames = new List<string>(); // liste pour stocker voisins
            int i = nomversindex[vertexName]; // index du sommet

            for (int j = 0; j < Order; j++)
            {
                // si valeur dans matrice != noEdgeValue alors arc existe
                if (matriceadj.GetValue(i, j) != pasarcval)
                    neighborNames.Add(indexversnom[j]); // ajoute nom du voisin
            }

            return neighborNames; // retourne liste voisins
        }

        // --- Gestion des arcs ---

        /* Ajoute un arc allant du sommet nomme sourceName au sommet nomme destinationName, avec le poids weight (1 par defaut)
         * Si le graphe n'est pas oriente, ajoute aussi l'arc inverse, avec le meme poids
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas ete trouve dans le graphe (source et/ou destination)
         * - il existe deja un arc avec ces extremites */
        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            if (!nomversindex.ContainsKey(sourceName) || !nomversindex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = nomversindex[sourceName]; // index source
            int j = nomversindex[destinationName]; // index dest

            if (matriceadj.GetValue(i, j) != pasarcval)
                throw new ArgumentException(); // erreur si arc existe deja

            matriceadj.SetValue(i, j, weight); // ajoute arc

            if (!direction)
                matriceadj.SetValue(j, i, weight); // ajoute arc inverse si non oriente
        }

        /* Supprime l'arc allant du sommet nomme sourceName au sommet nomme destinationName du graphe
         * Si le graphe n'est pas oriente, supprime aussi l'arc inverse
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas ete trouve dans le graphe (source et/ou destination)
         * - l'arc n'existe pas */
        public void RemoveEdge(string sourceName, string destinationName)
        {
            if (!nomversindex.ContainsKey(sourceName) || !nomversindex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = nomversindex[sourceName]; // index source
            int j = nomversindex[destinationName]; // index dest

            if (matriceadj.GetValue(i, j) == pasarcval)
                throw new ArgumentException(); // erreur si arc inexistant

            matriceadj.SetValue(i, j, pasarcval); // supprime arc

            if (!direction)
                matriceadj.SetValue(j, i, pasarcval); // supprime arc inverse si non oriente
        }

        /* Renvoie le poids de l'arc allant du sommet nomme sourceName au sommet nomme destinationName
         * Si le graphe n'est pas oriente, GetEdgeWeight(A, B) = GetEdgeWeight(B, A)
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas ete trouve dans le graphe (source et/ou destination)
         * - l'arc n'existe pas */
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            if (!nomversindex.ContainsKey(sourceName) || !nomversindex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = nomversindex[sourceName]; // index source
            int j = nomversindex[destinationName]; // index dest

            float weight = matriceadj.GetValue(i, j); // recup poids

            if (weight == pasarcval)
                throw new ArgumentException(); // erreur si pas d'arc

            return weight; // retourne poids
        }

        /* Affecte le poids l'arc allant du sommet nomme sourceName au sommet nomme destinationName a weight
         * Si le graphe n'est pas oriente, affecte le meme poids a l'arc inverse
         * Lève une ArgumentException si un des sommets n'a pas ete trouve dans le graphe (source et/ou destination) */
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            if (!nomversindex.ContainsKey(sourceName) || !nomversindex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = nomversindex[sourceName]; // index source
            int j = nomversindex[destinationName]; // index dest

            matriceadj.SetValue(i, j, weight); // modifie poids arc

            if (!direction)
                matriceadj.SetValue(j, i, weight); // modifie arc inverse si non oriente
        }

        // TODO : ajouter toutes les methodes que vous jugerez pertinentes
        // on pourra rajouter des fonctions style degre sommet, parcours bfs/dfs, plus court chemin, etc
    }
}
