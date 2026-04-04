namespace TourneeFutee
{
    // Résout le problème de voyageur de commerce défini par le graphe `graph`
    // en utilisant l'algorithme de Little
    public class Little
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private Graph graph;
        private float meilleurCout;
        private Tour meilleureTournee;
        private int nbCities;


        // Instancie le planificateur en spécifiant le graphe modélisant un problème de voyageur de commerce
        public Little(Graph graph)
        {
            // TODO : implémenter
            this.graph = graph;
            this.nbCities = graph.Order;
            this.meilleurCout = float.PositiveInfinity;
            this.meilleureTournee = null;
        }

        // Trouve la tournée optimale dans le graphe `this.graph`
        // (c'est à dire le cycle hamiltonien de plus faible coût)
        public Tour ComputeOptimalTour()
        {
            // TODO : implémenter
            return new Tour();
        }

        // --- Méthodes utilitaires réalisant des étapes de l'algorithme de Little


        // Réduit la matrice `m` et revoie la valeur totale de la réduction
        // Après appel à cette méthode, la matrice `m` est *modifiée*.
        public static float ReduceMatrix(Matrix m)
        {
            float reductionTotale = 0;

            for (int i = 0; i < m.NbRows; i++)
            {
                float min = float.PositiveInfinity;
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if(m.GetValue(i, j) < min)
                    {
                        min = m.GetValue(i, j);
                    }
                }
                if (!float.IsPositiveInfinity(min) && min > 0)
                {
                    reductionTotale += min;
                    for(int j = 0;j < m.NbColumns; j++)
                    {
                        m.SetValue(i,j, m.GetValue(i, j) - min);
                    }
                }
            }

            for (int j = 0; j < m.NbColumns; j++)
            {
                float min = float.PositiveInfinity;
                for (int i = 0; i < m.NbRows; i++)
                {
                    if (m.GetValue(i, j) < min)
                    {
                        min = m.GetValue(i, j);
                    }
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

        // Renvoie le regret de valeur maximale dans la matrice de coûts `m` sous la forme d'un tuple `(int i, int j, float value)`
        // où `i`, `j`, et `value` contiennent respectivement la ligne, la colonne et la valeur du regret maximale
        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            
                float maxRegret = -1;
                int bestI = -1, bestJ = -1;

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

        /* Renvoie vrai si le segment `segment` est un trajet parasite, c'est-à-dire s'il ferme prématurément la tournée incluant les trajets contenus dans `includedSegments`
         * Une tournée est incomplète si elle visite un nombre de villes inférieur à `nbCities`
         */
        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {

    
                // 1. interdit si on a déjà l'inverse
                foreach (var s in includedSegments)
                {
                    if (s.source == segment.destination && s.destination == segment.source)
                        return true;
                }

                // 2. suivre le chemin pour voir si on crée une boucle
                string current = segment.destination;

                int count = 1; // nombre de villes parcourues

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

                    // si on revient au point de départ → cycle
                    if (current == segment.source)
                    {
                        // cycle trop petit → interdit
                        if (count < nbCities)
                            return true;
                    }
                }

                return false;
            }
        

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }
}
