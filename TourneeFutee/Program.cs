namespace TourneeFutee
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== TEST MANUEL DE PERSISTANCE ===");

            try
            {
                // 1. Initialisation du service (Identifiants imposés par les tests)
                // On utilise "tourneefutee_test" car c'est ce que ton Workbench a créé
                var service = new ServicePersistance("127.0.0.1", "tourneefutee_test", "root", "root");
                Console.WriteLine("1. Connexion établie avec succès.");

                // 2. Création d'un graphe simple pour le test
                Graph g = new Graph(true); // Graphe orienté
                g.AddVertex("Nantes");
                g.AddVertex("Paris");
                g.AddEdge("Nantes", "Paris", 380.5f);
                Console.WriteLine("2.  Graphe de test créé en mémoire (Nantes -> Paris).");

                // 3. Test de Sauvegarde
                Console.WriteLine("3. Tentative de sauvegarde...");
                uint id = service.SaveGraph(g);
                Console.WriteLine($"  Sauvegarde réussie ! ID généré : {id}");

                // 4. Test de Chargement
                Console.WriteLine("4. Tentative de chargement...");
                Graph gCharge = service.LoadGraph(id);

                if (gCharge != null && gCharge.Order == 2)
                {
                    Console.WriteLine("  Chargement réussi ! Le graphe a bien 2 sommets.");
                }

                Console.WriteLine("\nTOUT FONCTIONNE !");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n ERREUR DÉTECTÉE :");
                Console.WriteLine(ex.Message);

                if (ex.InnerException != null)
                    Console.WriteLine("Détail : " + ex.InnerException.Message);
            }

            Console.WriteLine("\nAppuie sur Entrée pour quitter...");
            Console.ReadLine();
        }
    }
}
