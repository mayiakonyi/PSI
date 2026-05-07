using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    public class ServicePersistance
    {
        private readonly string _connectionString;

        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
            // Construction de la chaîne de connexion
            _connectionString = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";

            try
            {
                // Test de connexion immédiat pour valider les identifiants
                using (var conn = OpenConnection())
                {
                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Impossible de se connecter à la base de données : " + ex.Message);
            }
        }

        public uint SaveGraph(Graph g)
        {
            using (var conn = OpenConnection())
            {
                // 1. Insertion du Graphe
                string sqlG = "INSERT INTO Graphe(nb_sommets, est_oriente, nom) VALUES (@nb, @or, @nom); SELECT LAST_INSERT_ID();";
                var cmdG = new MySqlCommand(sqlG, conn);
                cmdG.Parameters.AddWithValue("@nb", g.Order);
                cmdG.Parameters.AddWithValue("@or", g.Directed ? 1 : 0);
                cmdG.Parameters.AddWithValue("@nom", "Graphe_" + DateTime.Now.ToString("HHmmss"));
                uint graphId = Convert.ToUInt32(cmdG.ExecuteScalar());

                // Dictionnaire pour mapper Nom C# -> ID Base de données
                Dictionary<string, uint> mapSommets = new Dictionary<string, uint>();

                // 2. Insertion des Sommets 
                foreach (string nom in g.GetVertices())
                {
                    string sqlS = "INSERT INTO Sommet(nom, graphe_id) VALUES (@nom, @gid); SELECT LAST_INSERT_ID();";
                    var cmdS = new MySqlCommand(sqlS, conn);
                    cmdS.Parameters.AddWithValue("@nom", nom);
                    cmdS.Parameters.AddWithValue("@gid", graphId);
                    mapSommets.Add(nom, Convert.ToUInt32(cmdS.ExecuteScalar()));
                }

                // 3. Insertion des Arcs (Correction de l'ArgumentException)
                var villes = g.GetVertices();
                foreach (string srcNom in villes)
                {
                    foreach (string dstNom in villes)
                    {
                        try
                        {
                            float p = g.GetEdgeWeight(srcNom, dstNom);
                            if (p > 0 && !float.IsPositiveInfinity(p))
                            {
                                string sqlA = "INSERT INTO Arc(sommet_source, sommet_dest, poids, graphe_id) VALUES (@s, @d, @p, @gid);";
                                var cmdA = new MySqlCommand(sqlA, conn);
                                cmdA.Parameters.AddWithValue("@s", mapSommets[srcNom]);
                                cmdA.Parameters.AddWithValue("@d", mapSommets[dstNom]);
                                cmdA.Parameters.AddWithValue("@p", p);
                                cmdA.Parameters.AddWithValue("@gid", graphId);
                                cmdA.ExecuteNonQuery();
                            }
                        }
                        catch (ArgumentException)
                        {
                            // On ignore les arcs inexistants (fréquent en asymétrique)
                            continue;
                        }
                    }
                }
                return graphId;
            }
        }

        public Graph LoadGraph(uint id)
        {
            using (var conn = OpenConnection())
            {
                // 1. Déterminer si le graphe est orienté
                var cmdG = new MySqlCommand("SELECT est_oriente FROM Graphe WHERE id = @id", conn);
                cmdG.Parameters.AddWithValue("@id", id);
                var orientedObj = cmdG.ExecuteScalar();
                if (orientedObj == null) throw new Exception("Graphe introuvable.");

                bool oriented = Convert.ToBoolean(orientedObj);
                Graph g = new Graph(oriented);

                // 2. Charger TOUS les sommets d'abord 
                // On les charge tous avant de passer aux arcs
                var cmdS = new MySqlCommand("SELECT nom FROM Sommet WHERE graphe_id = @id", conn);
                cmdS.Parameters.AddWithValue("@id", id);

                using (var readerS = cmdS.ExecuteReader())
                {
                    while (readerS.Read())
                    {
                        g.AddVertex(readerS["nom"].ToString().Trim());
                    }
                } // Le readerS se ferme ICI obligatoirement

                // 3. Charger les arcs seulement après
                string sqlA = @"SELECT s.nom as src, d.nom as dst, a.poids 
                        FROM Arc a 
                        JOIN Sommet s ON a.sommet_source = s.id 
                        JOIN Sommet d ON a.sommet_dest = d.id 
                        WHERE a.graphe_id = @id";

                var cmdA = new MySqlCommand(sqlA, conn);
                cmdA.Parameters.AddWithValue("@id", id);

                using (var readerA = cmdA.ExecuteReader())
                {
                    while (readerA.Read())
                    {
                        string s = readerA["src"].ToString().Trim();
                        string d = readerA["dst"].ToString().Trim();
                        float p = Convert.ToSingle(readerA["poids"]);

                        // On ajoute l'arc
                        g.AddEdge(s, d, p);
                    }
                }
                return g;
            }
        }

        public uint SaveTour(uint graphId, Tour t)
        {
            using (var conn = OpenConnection())
            {
                // 1. Insertion de la tournée
                var cmdT = new MySqlCommand("INSERT INTO Tournee(cout_total, graphe_id) VALUES (@c, @g); SELECT LAST_INSERT_ID();", conn);
                cmdT.Parameters.AddWithValue("@c", t.Cost);
                cmdT.Parameters.AddWithValue("@g", graphId);
                uint tourId = Convert.ToUInt32(cmdT.ExecuteScalar());

                // 2. Insertion des étapes
                int ordre = 1;
                foreach (string ville in t.Vertices)
                {
                    // On utilise TRIM() en SQL pour être super prudent
                    string sqlE = @"INSERT INTO EtapeTournee(tournee_id, numero_ordre, sommet_id) 
                            SELECT @tid, @ord, id FROM Sommet 
                            WHERE TRIM(nom) = TRIM(@v) AND graphe_id = @gid 
                            LIMIT 1;";

                    var cmdE = new MySqlCommand(sqlE, conn);
                    cmdE.Parameters.AddWithValue("@tid", tourId);
                    cmdE.Parameters.AddWithValue("@ord", ordre++);
                    cmdE.Parameters.AddWithValue("@v", ville);
                    cmdE.Parameters.AddWithValue("@gid", graphId);

                    int rows = cmdE.ExecuteNonQuery();
                    if (rows == 0)
                    {
                        // Si ça arrive ici, c'est que la ville dans la Tournée n'existe pas dans la table Sommet
                        throw new Exception($"Erreur : La ville '{ville}' n'existe pas pour le graphe {graphId}.");
                    }
                }
                return tourId;
            }
        }

        public Tour LoadTour(uint id)
        {
            using (var conn = OpenConnection())
            {
                // 1. Récupérer le coût
                var cmdT = new MySqlCommand("SELECT cout_total FROM Tournee WHERE id = @id", conn);
                cmdT.Parameters.AddWithValue("@id", id);
                object res = cmdT.ExecuteScalar();
                if (res == null) throw new Exception("Tournée introuvable.");
                float coutTotal = Convert.ToSingle(res);

                // 2. Récupérer TOUTES les étapes dans l'ordre
                List<string> sommets = new List<string>();
                string sqlE = @"SELECT s.nom 
                        FROM EtapeTournee e 
                        JOIN Sommet s ON e.sommet_id = s.id 
                        WHERE e.tournee_id = @id 
                        ORDER BY e.numero_ordre ASC";

                var cmdE = new MySqlCommand(sqlE, conn);
                cmdE.Parameters.AddWithValue("@id", id);

                using (var reader = cmdE.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sommets.Add(reader["nom"].ToString());
                    }
                }

                // Si sommets.Count est différent de ce qui est attendu, 
                // le problème venait du SaveTour (étape 2 ci-dessus)
                return new Tour(sommets, coutTotal);
            }
        }


        private MySqlConnection OpenConnection()
        {
            MySqlConnection conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
