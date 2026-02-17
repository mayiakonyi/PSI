using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Graph
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents
        private readonly bool _directed; // indique si le graphe est oriente ou pas
        private readonly float _noEdgeValue; // valeur pour signaler l'absence d'un arc
        private readonly Matrix _adjacencyMatrix; // matrice d'adjacence pour stocker les arcs
        private readonly Dictionary<string, int> _nameToIndex; // associe un nom de sommet a son index
        private readonly List<float> _vertexValues; // stocke les valeurs des sommets
        private readonly List<string> _indexToName; // permet de retrouver le nom d'un sommet via son index

        // --- Construction du graphe ---
        // Contruit un graphe (directed=true => orienté)
        // La valeur noEdgeValue est le poids modélisant l'absence d'un arc (0 par défaut)
        public Graph(bool directed, float noEdgeValue = 0)
        {
            _directed = directed; // sauvegarde si le graphe est oriente
            _noEdgeValue = noEdgeValue; // sauvegarde valeur pour pas d'arc
            _adjacencyMatrix = new Matrix(0, 0, noEdgeValue); // init matrice vide
            _nameToIndex = new Dictionary<string, int>(); // init dictionnaire noms->index
            _vertexValues = new List<float>(); // init liste valeurs sommets
            _indexToName = new List<string>(); // init liste noms par index
        }

        // --- Proprietes ---

        // Propriete : ordre du graphe
        // Lecture seule
        public int Order
        {
            get { return _vertexValues.Count; } // le nb de sommets
            // pas de set
        }

        // Propriete : graphe oriente ou non
        // Lecture seule
        public bool Directed
        {
            get { return _directed; } // renvoie si le graphe est oriente
            // pas de set
        }

        // --- Gestion des sommets ---

        // Ajoute le sommet de nom name et de valeur value (0 par defaut) dans le graphe
        // Lève une ArgumentException s'il existe deja un sommet avec le meme nom dans le graphe
        public void AddVertex(string name, float value = 0)
        {
            if (_nameToIndex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet existe deja

            int newIndex = Order; // nouvel index = nb actuel de sommets

            _nameToIndex[name] = newIndex; // map nom->index
            _vertexValues.Add(value); // ajoute valeur sommet
            _indexToName.Add(name); // ajoute nom dans liste index->nom

            _adjacencyMatrix.AddRow(newIndex); // agrandit matrice avec nouvelle ligne
            _adjacencyMatrix.AddColumn(newIndex); // agrandit matrice avec nouvelle colonne
        }

        // Supprime le sommet de nom name du graphe (et tous les arcs associes)
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public void RemoveVertex(string name)
        {
            if (!_nameToIndex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet inexistant

            int index = _nameToIndex[name]; // recuperer index du sommet

            _adjacencyMatrix.RemoveRow(index); // supprime ligne dans matrice
            _adjacencyMatrix.RemoveColumn(index); // supprime colonne dans matrice

            _vertexValues.RemoveAt(index); // supprime valeur du sommet
            _indexToName.RemoveAt(index); // supprime nom du sommet
            _nameToIndex.Remove(name); // supprime mapping nom->index

            // mise a jour des index pour les sommets apres celui supprime
            for (int i = index; i < _indexToName.Count; i++)
            {
                _nameToIndex[_indexToName[i]] = i;
            }
        }

        // Renvoie la valeur du sommet de nom name
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public float GetVertexValue(string name)
        {
            if (!_nameToIndex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet inexistant

            return _vertexValues[_nameToIndex[name]]; // renvoie valeur
        }

        // Affecte la valeur du sommet de nom name a value
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public void SetVertexValue(string name, float value)
        {
            if (!_nameToIndex.ContainsKey(name))
                throw new ArgumentException(); // erreur si sommet inexistant

            _vertexValues[_nameToIndex[name]] = value; // met a jour valeur
        }

        // Renvoie la liste des noms des voisins du sommet de nom vertexName
        // (si ce sommet n'a pas de voisins, la liste sera vide)
        // Lève une ArgumentException si le sommet n'a pas ete trouve dans le graphe
        public List<string> GetNeighbors(string vertexName)
        {
            if (!_nameToIndex.ContainsKey(vertexName))
                throw new ArgumentException(); // erreur si sommet inexistant

            List<string> neighborNames = new List<string>(); // liste pour stocker voisins
            int i = _nameToIndex[vertexName]; // index du sommet

            for (int j = 0; j < Order; j++)
            {
                // si valeur dans matrice != noEdgeValue alors arc existe
                if (_adjacencyMatrix.GetValue(i, j) != _noEdgeValue)
                    neighborNames.Add(_indexToName[j]); // ajoute nom du voisin
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
            if (!_nameToIndex.ContainsKey(sourceName) || !_nameToIndex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = _nameToIndex[sourceName]; // index source
            int j = _nameToIndex[destinationName]; // index dest

            if (_adjacencyMatrix.GetValue(i, j) != _noEdgeValue)
                throw new ArgumentException(); // erreur si arc existe deja

            _adjacencyMatrix.SetValue(i, j, weight); // ajoute arc

            if (!_directed)
                _adjacencyMatrix.SetValue(j, i, weight); // ajoute arc inverse si non oriente
        }

        /* Supprime l'arc allant du sommet nomme sourceName au sommet nomme destinationName du graphe
         * Si le graphe n'est pas oriente, supprime aussi l'arc inverse
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas ete trouve dans le graphe (source et/ou destination)
         * - l'arc n'existe pas */
        public void RemoveEdge(string sourceName, string destinationName)
        {
            if (!_nameToIndex.ContainsKey(sourceName) || !_nameToIndex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = _nameToIndex[sourceName]; // index source
            int j = _nameToIndex[destinationName]; // index dest

            if (_adjacencyMatrix.GetValue(i, j) == _noEdgeValue)
                throw new ArgumentException(); // erreur si arc inexistant

            _adjacencyMatrix.SetValue(i, j, _noEdgeValue); // supprime arc

            if (!_directed)
                _adjacencyMatrix.SetValue(j, i, _noEdgeValue); // supprime arc inverse si non oriente
        }

        /* Renvoie le poids de l'arc allant du sommet nomme sourceName au sommet nomme destinationName
         * Si le graphe n'est pas oriente, GetEdgeWeight(A, B) = GetEdgeWeight(B, A)
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas ete trouve dans le graphe (source et/ou destination)
         * - l'arc n'existe pas */
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            if (!_nameToIndex.ContainsKey(sourceName) || !_nameToIndex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = _nameToIndex[sourceName]; // index source
            int j = _nameToIndex[destinationName]; // index dest

            float weight = _adjacencyMatrix.GetValue(i, j); // recup poids

            if (weight == _noEdgeValue)
                throw new ArgumentException(); // erreur si pas d'arc

            return weight; // retourne poids
        }

        /* Affecte le poids l'arc allant du sommet nomme sourceName au sommet nomme destinationName a weight
         * Si le graphe n'est pas oriente, affecte le meme poids a l'arc inverse
         * Lève une ArgumentException si un des sommets n'a pas ete trouve dans le graphe (source et/ou destination) */
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            if (!_nameToIndex.ContainsKey(sourceName) || !_nameToIndex.ContainsKey(destinationName))
                throw new ArgumentException(); // erreur si sommets manquants

            int i = _nameToIndex[sourceName]; // index source
            int j = _nameToIndex[destinationName]; // index dest

            _adjacencyMatrix.SetValue(i, j, weight); // modifie poids arc

            if (!_directed)
                _adjacencyMatrix.SetValue(j, i, weight); // modifie arc inverse si non oriente
        }

        // TODO : ajouter toutes les methodes que vous jugerez pertinentes
        // on pourra rajouter des fonctions style degre sommet, parcours bfs/dfs, plus court chemin, etc
    }
}
