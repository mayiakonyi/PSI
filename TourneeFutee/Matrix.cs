namespace TourneeFutee
{
    public class Matrix
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private List<List<float>> donnee;
        private float defaultValue;


        /* Crée une matrice de dimensions `nbRows` x `nbColums`.
         * Toutes les cases de cette matrice sont remplies avec `defaultValue`.
         * Lève une ArgumentOutOfRangeException si une des dimensions est négative
         */
        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            // TODO : implémenter
            if(nbRows < 0 || nbColumns < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.defaultValue = defaultValue;
            this.donnee = new List<List<float>>();

           
            for (int i = 0; i < nbRows; i++)
            {
                
                List<float> nouvelleLigne = new List<float>();

                
                for (int j = 0; j < nbColumns; j++)
                {
                    nouvelleLigne.Add(defaultValue);
                }

                
                donnee.Add(nouvelleLigne);
            }
        }

        // Propriété : valeur par défaut utilisée pour remplir les nouvelles cases
        // Lecture seule
        public float DefaultValue
        {
            get { return defaultValue; } 
            // TODO : implémenter
            // pas de set
        }

        // Propriété : nombre de lignes
        // Lecture seule
        public int NbRows
        {
            get { return donnee.Count; }
            // TODO : implémenter
                 // pas de set
        }

        // Propriété : nombre de colonnes
        // Lecture seule
        public int NbColumns
        {
            get
            {
                if (donnee.Count > 0)
                return donnee[0].Count;
                return 0;
            } 
            // TODO : implémenter
              // pas de set
        }

        /* Insère une ligne à l'indice `i`. Décale les lignes suivantes vers le bas.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `i` = NbRows, insère une ligne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
         */
        public void AddRow(int i)
        {
            
            
            if (i < 0 || i > donnee.Count)
            {
                throw new ArgumentOutOfRangeException(); 
            }

            
            List<float> nouvelleLigne = new List<float>();

            
            int nbColonnesActuel = 0;
            if (donnee.Count > 0)
            {
                nbColonnesActuel = donnee[0].Count;
            }

            
            for (int j = 0; j < nbColonnesActuel; j++)
            {
                nouvelleLigne.Add(defaultValue);
            }

            
            donnee.Insert(i, nouvelleLigne);
        }

        /* Insère une colonne à l'indice `j`. Décale les colonnes suivantes vers la droite.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `j` = NbColums, insère une colonne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
         */
        public void AddColumn(int j)
        {
           
            if (j < 0 || j > NbColumns)
            throw new ArgumentOutOfRangeException();

            
            foreach (List<float> ligne in donnee)
            {
                ligne.Insert(j, defaultValue);
            }
        }

        // Supprime la ligne à l'indice `i`. Décale les lignes suivantes vers le haut.
        // Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
        public void RemoveRow(int i)
        {
            if (i < 0 || i >= NbRows)
            {
                throw new ArgumentOutOfRangeException();
            }
            donnee.RemoveAt(i);

        }

        // Supprime la colonne à l'indice `j`. Décale les colonnes suivantes vers la gauche.
        // Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
        public void RemoveColumn(int j)
        {
            if (j < 0 || j >= NbColumns)
            {
                throw new ArgumentOutOfRangeException();
            }
            
            foreach (List<float> ligne in donnee)
            {
                ligne.RemoveAt(j);
            }
        }

        // Renvoie la valeur à la ligne `i` et colonne `j`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public float GetValue(int i, int j)
        {
            if (i < 0 || i >= NbRows || j < 0 || j >= NbColumns)
            {
                throw new ArgumentOutOfRangeException();
            }
            return donnee[i][j];
        }

        // Affecte la valeur à la ligne `i` et colonne `j` à `v`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public void SetValue(int i, int j, float v)
        {
            // TODO : implémenter
            if (i < 0 || i >= NbRows || j < 0 || j >= NbColumns)
            {
                throw new ArgumentOutOfRangeException();
            }
            donnee[i][j] = v;
        }

        // Affiche la matrice
        public void Print()
        {
            // TODO : implémenter
            for (int i = 0; i < NbRows; i++)
            {
                for (int j = 0; j < NbColumns; j++)
                {
                    Console.Write(donnee[i][j] + "\t");
                }
                Console.WriteLine();
            }
        }


        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }


}
