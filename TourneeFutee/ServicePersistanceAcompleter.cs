using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    /// <summary>
    /// Service de persistance permettant de sauvegarder et charger
    /// des graphes et des tournées dans une base de données MySQL.
    /// </summary>
    public class ServicePersistance
    {
        // ─────────────────────────────────────────────────────────────────────
        // Attributs privés
        // ─────────────────────────────────────────────────────────────────────

        private readonly string _connectionString;

        // TODO : si vous avez besoin de maintenir une connexion ouverte,
        //        ajoutez un attribut MySqlConnection ici.

        // ─────────────────────────────────────────────────────────────────────
        // Constructeur
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Instancie un service de persistance et se connecte automatiquement
        /// à la base de données <paramref name="dbname"/> sur le serveur
        /// à l'adresse IP <paramref name="serverIp"/>.
        /// Les identifiants sont définis par <paramref name="user"/> (utilisateur)
        /// et <paramref name="pwd"/> (mot de passe).
        /// </summary>
        /// <param name="serverIp">Adresse IP du serveur MySQL.</param>
        /// <param name="dbname">Nom de la base de données.</param>
        /// <param name="user">Nom d'utilisateur.</param>
        /// <param name="pwd">Mot de passe.</param>
        /// <exception cref="Exception">Levée si la connexion échoue.</exception>
        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
          // TODO : initialiser et ouvrir la connexion à la base de données
        // Exemple :
            _connectionString = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";

            try
            {
                using (var conn = OpenConnection())
                {
                    // Connexion réussie, elle sera fermée automatiquement ici
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Échec de la connexion : " + ex.Message);
            }


        }

        


        // ─────────────────────────────────────────────────────────────────────
        // Méthodes publiques
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sauvegarde le graphe <paramref name="g"/> en base de données
        /// (sommets et arcs inclus) et renvoie son identifiant.
        /// </summary>
        /// <param name="g">Le graphe à sauvegarder.</param>
        /// <returns>Identifiant du graphe en base de données (AUTO_INCREMENT).</returns>
        public uint SaveGraph(Graph g)
        {
            using (var conn = OpenConnection())
            {
                // 1. Insertion du Graphe [cite: 84]
                // Dans SaveGraph, assure-toi que l'INSERT ressemble à ça :
                string sqlG = "INSERT INTO Graphe(nb_sommets, est_oriente, nom) VALUES (@nb, @or, @nom); SELECT LAST_INSERT_ID();";
                var cmdG = new MySqlCommand(sqlG, conn);
                cmdG.Parameters.AddWithValue("@nb", g.Order); // IMPORTANT : not null dans ton SQL
                cmdG.Parameters.AddWithValue("@or", g.Directed ? 1 : 0);
                cmdG.Parameters.AddWithValue("@nom", "Graphe_Test");
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

                // 3. Insertion des Arcs 
                List<string> villes = g.GetVertices();
                for (int i = 0; i < g.Order; i++)
                {
                    for (int j = 0; j < g.Order; j++)
                    {
                        float p = g.GetEdgeWeight(villes[i], villes[j]);
                        if (p != 0 && !float.IsPositiveInfinity(p))
                        {
                            string sqlA = "INSERT INTO Arc(sommet_source, sommet_dest, poids, graphe_id) VALUES (@s, @d, @p, @gid);";
                            var cmdA = new MySqlCommand(sqlA, conn);
                            cmdA.Parameters.AddWithValue("@s", mapSommets[villes[i]]);
                            cmdA.Parameters.AddWithValue("@d", mapSommets[villes[j]]);
                            cmdA.Parameters.AddWithValue("@p", p);
                            cmdA.Parameters.AddWithValue("@gid", graphId);
                            cmdA.ExecuteNonQuery();
                        }
                    }
                }
                return graphId;
            }
        }

        /// <summary>
        /// Charge depuis la base de données le graphe identifié par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Graph"/>.
        /// </summary>
        /// <param name="id">Identifiant du graphe à charger.</param>
        /// <returns>Instance de <see cref="Graph"/> reconstituée.</returns>
        public Graph LoadGraph(uint id)
        {
            using (var conn = OpenConnection())
            {
                // 1. Charger les infos de base [cite: 87]
                var cmdG = new MySqlCommand("SELECT est_oriente FROM Graphe WHERE id = @id", conn);
                cmdG.Parameters.AddWithValue("@id", id);
                bool oriented = Convert.ToBoolean(cmdG.ExecuteScalar());

                Graph g = new Graph(oriented);

                // 2. Charger les Sommets [cite: 88]
                var cmdS = new MySqlCommand("SELECT nom FROM Sommet WHERE graphe_id = @id ORDER BY id ASC", conn);
                cmdS.Parameters.AddWithValue("@id", id);
                using (var reader = cmdS.ExecuteReader())
                {
                    while (reader.Read()) g.AddVertex(reader["nom"].ToString());
                }

                // 3. Charger les Arcs [cite: 89]
                string sqlA = "SELECT s.nom as src, d.nom as dst, a.poids FROM Arc a " +
                              "JOIN Sommet s ON a.sommet_source = s.id JOIN Sommet d ON a.sommet_dest = d.id " +
                              "WHERE a.graphe_id = @id";
                var cmdA = new MySqlCommand(sqlA, conn);
                cmdA.Parameters.AddWithValue("@id", id);
                using (var reader = cmdA.ExecuteReader())
                {
                    while (reader.Read()) g.AddEdge(reader["src"].ToString(), reader["dst"].ToString(), Convert.ToSingle(reader["poids"]));
                }
                return g;
            }
        }

        /// <summary>
        /// Sauvegarde la tournée <paramref name="t"/> (effectuée dans le graphe
        /// identifié par <paramref name="graphId"/>) en base de données
        /// et renvoie son identifiant.
        /// </summary>
        /// <param name="graphId">Identifiant BdD du graphe dans lequel la tournée a été calculée.</param>
        /// <param name="t">La tournée à sauvegarder.</param>
        /// <returns>Identifiant de la tournée en base de données (AUTO_INCREMENT).</returns>
        public uint SaveTour(uint graphId, Tour t)
        {
            using (var conn = OpenConnection())
            {
               // 1. Insertion Tournée [cite: 91]
                var cmdT = new MySqlCommand("INSERT INTO Tournee(cout_total, graphe_id) VALUES (@c, @g); SELECT LAST_INSERT_ID();", conn);
                cmdT.Parameters.AddWithValue("@c", t.Cost);
                cmdT.Parameters.AddWithValue("@g", graphId);
                uint tourId = Convert.ToUInt32(cmdT.ExecuteScalar());

                // 2. Insertion Étapes [cite: 92]
                int ordre = 1;
                foreach (string ville in t.Vertices)
                {
                    string sqlE = "INSERT INTO EtapeTournee(tournee_id, numero_ordre, sommet_id) " +
                                  "VALUES (@tid, @ord, (SELECT id FROM Sommet WHERE nom = @v AND graphe_id = @gid));";
                    var cmdE = new MySqlCommand(sqlE, conn);
                    cmdE.Parameters.AddWithValue("@tid", tourId);
                    cmdE.Parameters.AddWithValue("@ord", ordre++);
                    cmdE.Parameters.AddWithValue("@v", ville);
                    cmdE.Parameters.AddWithValue("@gid", graphId);
                    cmdE.ExecuteNonQuery();
                }
                return tourId;
            }
        }

        /// <summary>
        /// Charge depuis la base de données la tournée identifiée par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Tour"/>.
        /// </summary>
        /// <param name="id">Identifiant de la tournée à charger.</param>
        /// <returns>Instance de <see cref="Tour"/> reconstituée.</returns>
        
        
            public Tour LoadTour(uint id)
            {
                using (var conn = OpenConnection())
                {
                    // 1. Charger les infos de la tournée
                    var cmdT = new MySqlCommand("SELECT cout_total, graphe_id FROM Tournee WHERE id = @id", conn);
                    cmdT.Parameters.AddWithValue("@id", id);

                    float coutTotal;
                    uint grapheId;

                    using (var reader = cmdT.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception($"Tournée avec id={id} introuvable.");

                        coutTotal = Convert.ToSingle(reader["cout_total"]);
                        grapheId = Convert.ToUInt32(reader["graphe_id"]);
                    }

                    // 2. Charger les étapes dans l'ordre
                    List<string> sommets = new List<string>();

                    string sqlE = "SELECT s.nom FROM EtapeTournee e " +
                                  "JOIN Sommet s ON e.sommet_id = s.id " +
                                  "WHERE e.tournee_id = @id " +
                                  "ORDER BY e.numero_ordre ASC";

                    var cmdE = new MySqlCommand(sqlE, conn);
                    cmdE.Parameters.AddWithValue("@id", id);

                    using (var reader = cmdE.ExecuteReader())
                    {
                        while (reader.Read())
                            sommets.Add(reader["nom"].ToString());
                    }

                    // 3. Créer et retourner l'instance Tour
                    return new Tour(sommets, coutTotal);
                }
            }
        

        // ─────────────────────────────────────────────────────────────────────
        // Méthodes utilitaires privées (à compléter selon vos besoins)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée et retourne une nouvelle connexion MySQL ouverte.
        /// Encadrez toujours l'appel dans un bloc using pour garantir la fermeture.
        /// </summary>
        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
