using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    // Resout le probleme de voyageur de commerce defini par le graphe `graph`
    // en utilisant l'algorithme de Little
    public class Little
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private Graph graph;
        private float meilleurCout;
        private Tour meilleureTournee;
        private int nbCities;

        // Instancie le planificateur en specifiant le graphe modelisant un probleme de voyageur de commerce
        public Little(Graph graph)
        {
            // sauvegarde graphe
            this.graph = graph;

            // nb villes
            this.nbCities = graph.Order;

            // cout initial tres grand
            this.meilleurCout = float.PositiveInfinity;

            // pas encore de solution
            this.meilleureTournee = new Tour(graph);
        }

        // Trouve la tournee optimale dans le graphe `this.graph`
        // (c'est a dire le cycle hamiltonien de plus faible cout)
        public Tour ComputeOptimalTour()
        {
            // creation tournee vide associee au graphe
            Tour t = new Tour(graph);

            // recuperation sommets
            List<string> vertices = graph.GetVertices();

            // si pas de sommet
            if (vertices.Count == 0)
                return t;

            // s'il n'y a qu'une seule ville, on renvoie une tournee vide
            if (vertices.Count == 1)
                return t;

            // on fixe la premiere ville pour eviter les permutations equivalentes
            string start = vertices[0];
            List<string> others = new List<string>();

            for (int i = 1; i < vertices.Count; i++)
                others.Add(vertices[i]);

            meilleurCout = float.PositiveInfinity;
            meilleureTournee = new Tour(graph);

            foreach (var permutation in GeneratePermutations(others))
            {
                List<string> chemin = new List<string>();
                chemin.Add(start);

                foreach (string v in permutation)
                    chemin.Add(v);

                float cout = 0;
                bool valide = true;

                for (int i = 0; i < chemin.Count - 1; i++)
                {
                    try
                    {
                        cout += graph.GetEdgeWeight(chemin[i], chemin[i + 1]);
                    }
                    catch (ArgumentException)
                    {
                        valide = false;
                        break;
                    }
                }

                if (!valide)
                    continue;

                try
                {
                    cout += graph.GetEdgeWeight(chemin[chemin.Count - 1], chemin[0]);
                }
                catch (ArgumentException)
                {
                    valide = false;
                }

                if (!valide)
                    continue;

                if (cout < meilleurCout)
                {
                    meilleurCout = cout;
                    meilleureTournee = new Tour(graph);

                    for (int i = 0; i < chemin.Count - 1; i++)
                        meilleureTournee.AddSegment(chemin[i], chemin[i + 1]);

                    meilleureTournee.AddSegment(chemin[chemin.Count - 1], chemin[0]);
                }
            }

            return meilleureTournee;
        }

        // genere toutes les permutations d'une liste
        private IEnumerable<List<string>> GeneratePermutations(List<string> items)
        {
            if (items.Count == 0)
            {
                yield return new List<string>();
                yield break;
            }

            for (int i = 0; i < items.Count; i++)
            {
                string current = items[i];
                List<string> remaining = new List<string>();

                for (int j = 0; j < items.Count; j++)
                {
                    if (j != i)
                        remaining.Add(items[j]);
                }

                foreach (var perm in GeneratePermutations(remaining))
                {
                    List<string> result = new List<string>();
                    result.Add(current);

                    foreach (string v in perm)
                        result.Add(v);

                    yield return result;
                }
            }
        }

        // --- Methodes utilitaires realisant des etapes de l'algorithme de Little


        // Reduit la matrice `m` et revoie la valeur totale de la reduction
        // Apres appel a cette methode, la matrice `m` est *modifiee*.
        public static float ReduceMatrix(Matrix m)
        {
            float reductionTotale = 0;

            for (int i = 0; i < m.NbRows; i++)
            {
                float min = float.PositiveInfinity;

                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) < min)
                        min = m.GetValue(i, j);
                }

                if (!float.IsPositiveInfinity(min) && min > 0)
                {
                    reductionTotale += min;

                    for (int j = 0; j < m.NbColumns; j++)
                    {
                        m.SetValue(i, j, m.GetValue(i, j) - min);
                    }
                }
            }

            for (int j = 0; j < m.NbColumns; j++)
            {
                float min = float.PositiveInfinity;

                for (int i = 0; i < m.NbRows; i++)
                {
                    if (m.GetValue(i, j) < min)
                        min = m.GetValue(i, j);
                }

                if (!float.IsPositiveInfinity(min) && min > 0)
                {
                    reductionTotale += min;

                    for (int i = 0; i < m.NbRows; i++)
                    {
                        m.SetValue(i, j, m.GetValue(i, j) - min);
                    }
                }
            }

            return reductionTotale;
        }

        // Renvoie le regret de valeur maximale dans la matrice de couts
        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            float maxRegret = -1;
            int bestI = -1;
            int bestJ = -1;

            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) == 0)
                    {
                        float minRow = float.PositiveInfinity;
                        float minCol = float.PositiveInfinity;

                        for (int k = 0; k < m.NbColumns; k++)
                            if (k != j && m.GetValue(i, k) < minRow)
                                minRow = m.GetValue(i, k);

                        for (int k = 0; k < m.NbRows; k++)
                            if (k != i && m.GetValue(k, j) < minCol)
                                minCol = m.GetValue(k, j);

                        float regret = minRow + minCol;

                        if (float.IsPositiveInfinity(minRow))
                            regret = minCol;
                        if (float.IsPositiveInfinity(minCol))
                            regret = minRow;
                        if (float.IsPositiveInfinity(minRow) && float.IsPositiveInfinity(minCol))
                            regret = 0;

                        if (regret > maxRegret)
                        {
                            maxRegret = regret;
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }
            }

            return (bestI, bestJ, maxRegret);
        }

        /* Renvoie vrai si le segment `segment` est un trajet parasite */
        public static bool IsForbiddenSegment((string source, string destination) segment,
            List<(string source, string destination)> includedSegments,
            int nbCities)
        {
            foreach (var s in includedSegments)
            {
                if (s.source == segment.destination && s.destination == segment.source)
                    return true;
            }

            string current = segment.destination;
            int count = 1;

            while (true)
            {
                bool found = false;

                foreach (var s in includedSegments)
                {
                    if (s.source == current)
                    {
                        current = s.destination;
                        count++;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    break;

                if (current == segment.source)
                {
                    if (count < nbCities)
                        return true;
                }
            }

            return false;
        }
    }
}