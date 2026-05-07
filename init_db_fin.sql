-- Créer la base de TEST
CREATE DATABASE IF NOT EXISTS tourneefutee_test;

-- Sélectionner la base de TEST
USE tourneefutee_test;

-- Supprimer les tables dans le bon ordre
DROP TABLE IF EXISTS EtapeTournee;
DROP TABLE IF EXISTS Tournee;
DROP TABLE IF EXISTS Arc;
DROP TABLE IF EXISTS Sommet;
DROP TABLE IF EXISTS Graphe;

-- Créer les tables
CREATE TABLE Graphe (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    est_oriente TINYINT(1) NOT NULL DEFAULT 0,
    nom VARCHAR(100) NULL,
    nb_sommets INT UNSIGNED NOT NULL DEFAULT 0,
    date_creation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE TABLE Sommet (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    graphe_id INT UNSIGNED NOT NULL,
    nom VARCHAR(50) NOT NULL,
    valeur FLOAT NULL,
    indice INT UNSIGNED NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE Arc (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    graphe_id INT UNSIGNED NOT NULL,
    sommet_source INT UNSIGNED NOT NULL,
    sommet_dest INT UNSIGNED NOT NULL,
    poids FLOAT NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_source) REFERENCES Sommet(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_dest) REFERENCES Sommet(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE Tournee (
    id           INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id    INT UNSIGNED    NOT NULL,
    cout_total   FLOAT           NOT NULL,
    date_calcul  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    nb_etapes    INT UNSIGNED    NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE EtapeTournee (
    tournee_id      INT UNSIGNED    NOT NULL,
    numero_ordre    INT UNSIGNED    NOT NULL,
    sommet_id       INT UNSIGNED    NOT NULL,
    PRIMARY KEY (tournee_id, numero_ordre),
    UNIQUE KEY uq_etape_sommet (tournee_id, sommet_id),
    FOREIGN KEY (tournee_id) REFERENCES Tournee(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_id)  REFERENCES Sommet(id)  ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Vérifier que les tables sont créées
SHOW TABLES;

ALTER TABLE EtapeTournee DROP INDEX uq_etape_sommet;