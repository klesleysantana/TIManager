using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using TIManager.Models;

namespace TIManager.Data
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private readonly string _dbPath;
        private readonly string _connectionString;

        private DatabaseService()
        {
            string folderPath = @"C:\TIManagerData";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            _dbPath = Path.Combine(folderPath, "timanager.db");
            _connectionString = $"Data Source={_dbPath}";
        }

        public void Initialize()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Chamados (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Titulo TEXT NOT NULL,
                        Solicitante TEXT NOT NULL,
                        Setor TEXT,
                        Prioridade TEXT,
                        Descricao TEXT,
                        Status TEXT,
                        CaminhoImagem TEXT,
                        DataAbertura TEXT NOT NULL,
                        DataFinalizacao TEXT
                    );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createDocTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Documentos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nome TEXT NOT NULL,
                        Categoria TEXT NOT NULL,
                        CaminhoArquivo TEXT NOT NULL,
                        CaminhoMiniatura TEXT,
                        DataUpload TEXT NOT NULL
                    );";

                using (var command = new SqliteCommand(createDocTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createAcoTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Acompanhamentos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ChamadoId INTEGER NOT NULL,
                        Texto TEXT NOT NULL,
                        Data TEXT NOT NULL,
                        Tipo TEXT NOT NULL DEFAULT 'Acompanhamento',
                        FOREIGN KEY(ChamadoId) REFERENCES Chamados(Id) ON DELETE CASCADE
                    );";

                using (var command = new SqliteCommand(createAcoTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Migração Acompanhamentos: Verifica se a coluna Tipo existe
                bool hasTipoColumn = false;
                using (var command = new SqliteCommand("PRAGMA table_info(Acompanhamentos);", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1).Equals("Tipo", StringComparison.OrdinalIgnoreCase))
                        {
                            hasTipoColumn = true;
                            break;
                        }
                    }
                }

                if (!hasTipoColumn)
                {
                    using (var command = new SqliteCommand("ALTER TABLE Acompanhamentos ADD COLUMN Tipo TEXT NOT NULL DEFAULT 'Acompanhamento';", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Migração: Verifica se a coluna CaminhoMiniatura existe e adiciona se necessário
                bool hasMiniaturaColumn = false;
                using (var command = new SqliteCommand("PRAGMA table_info(Documentos);", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1).Equals("CaminhoMiniatura", StringComparison.OrdinalIgnoreCase))
                        {
                            hasMiniaturaColumn = true;
                            break;
                        }
                    }
                }

                if (!hasMiniaturaColumn)
                {
                    using (var command = new SqliteCommand("ALTER TABLE Documentos ADD COLUMN CaminhoMiniatura TEXT;", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Migração Chamados: Verifica se a coluna DataFinalizacao existe
                bool hasFinalizacaoColumn = false;
                using (var command = new SqliteCommand("PRAGMA table_info(Chamados);", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1).Equals("DataFinalizacao", StringComparison.OrdinalIgnoreCase))
                        {
                            hasFinalizacaoColumn = true;
                            break;
                        }
                    }
                }

                if (!hasFinalizacaoColumn)
                {
                    using (var command = new SqliteCommand("ALTER TABLE Chamados ADD COLUMN DataFinalizacao TEXT;", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                string docPath = @"C:\TIManagerData\Documentos";
                if (!Directory.Exists(docPath)) Directory.CreateDirectory(docPath);

                string thumbPath = @"C:\TIManagerData\Thumbnails";
                if (!Directory.Exists(thumbPath)) Directory.CreateDirectory(thumbPath);

                string imagePath = @"C:\TIManagerData\Imagens";
                if (!Directory.Exists(imagePath)) Directory.CreateDirectory(imagePath);
            }
        }

        public List<Chamado> GetAllChamados()
        {
            var chamados = new List<Chamado>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Chamados ORDER BY DataAbertura DESC";

                using (var command = new SqliteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var chamado = new Chamado
                        {
                            Id = reader.GetInt32(0),
                            Titulo = reader.GetString(1),
                            Solicitante = reader.GetString(2),
                            Setor = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Prioridade = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Descricao = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Status = reader.IsDBNull(6) ? null : reader.GetString(6),
                            CaminhoImagem = reader.IsDBNull(7) ? null : reader.GetString(7),
                            DataAbertura = DateTime.Parse(reader.GetString(8)),
                            DataFinalizacao = reader.IsDBNull(9) ? (DateTime?)null : DateTime.Parse(reader.GetString(9))
                        };
                        chamados.Add(chamado);
                    }
                }
            }

            return chamados;
        }

        public int InsertChamado(Chamado chamado)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO Chamados (Titulo, Solicitante, Setor, Prioridade, Descricao, Status, CaminhoImagem, DataAbertura, DataFinalizacao)
                    VALUES (@Titulo, @Solicitante, @Setor, @Prioridade, @Descricao, @Status, @CaminhoImagem, @DataAbertura, @DataFinalizacao);
                    SELECT last_insert_rowid();";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Titulo", chamado.Titulo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Solicitante", chamado.Solicitante ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Setor", chamado.Setor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Prioridade", chamado.Prioridade ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Descricao", chamado.Descricao ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", chamado.Status ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CaminhoImagem", chamado.CaminhoImagem ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DataAbertura", chamado.DataAbertura.ToString("o"));
                    command.Parameters.AddWithValue("@DataFinalizacao", chamado.DataFinalizacao?.ToString("o") ?? (object)DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void UpdateChamado(Chamado chamado)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    UPDATE Chamados 
                    SET Titulo = @Titulo, 
                        Solicitante = @Solicitante, 
                        Setor = @Setor, 
                        Prioridade = @Prioridade, 
                        Descricao = @Descricao, 
                        Status = @Status, 
                        CaminhoImagem = @CaminhoImagem,
                        DataFinalizacao = @DataFinalizacao
                    WHERE Id = @Id;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", chamado.Id);
                    command.Parameters.AddWithValue("@Titulo", chamado.Titulo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Solicitante", chamado.Solicitante ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Setor", chamado.Setor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Prioridade", chamado.Prioridade ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Descricao", chamado.Descricao ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", chamado.Status ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CaminhoImagem", chamado.CaminhoImagem ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DataFinalizacao", chamado.DataFinalizacao?.ToString("o") ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteChamado(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Delete related acompanhamentos first
                string deleteAcoQuery = "DELETE FROM Acompanhamentos WHERE ChamadoId = @Id;";
                using (var command = new SqliteCommand(deleteAcoQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }

                string query = "DELETE FROM Chamados WHERE Id = @Id;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Documento> GetAllDocumentos(string categoria)
        {
            var docs = new List<Documento>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Documentos WHERE Categoria = @Categoria ORDER BY DataUpload DESC";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Categoria", categoria);
                    using (var reader = command.ExecuteReader())
                    {
                        int idIdx = reader.GetOrdinal("Id");
                        int nomeIdx = reader.GetOrdinal("Nome");
                        int catIdx = reader.GetOrdinal("Categoria");
                        int arqIdx = reader.GetOrdinal("CaminhoArquivo");
                        int miniIdx = reader.GetOrdinal("CaminhoMiniatura");
                        int dataIdx = reader.GetOrdinal("DataUpload");

                        while (reader.Read())
                        {
                            docs.Add(new Documento
                            {
                                Id = reader.GetInt32(idIdx),
                                Nome = reader.GetString(nomeIdx),
                                Categoria = reader.GetString(catIdx),
                                CaminhoArquivo = reader.GetString(arqIdx),
                                CaminhoMiniatura = reader.IsDBNull(miniIdx) ? null : reader.GetString(miniIdx),
                                DataUpload = DateTime.Parse(reader.GetString(dataIdx))
                            });
                        }
                    }
                }
            }
            return docs;
        }

        public int InsertDocumento(Documento doc)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO Documentos (Nome, Categoria, CaminhoArquivo, CaminhoMiniatura, DataUpload)
                    VALUES (@Nome, @Categoria, @CaminhoArquivo, @CaminhoMiniatura, @DataUpload);
                    SELECT last_insert_rowid();";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nome", doc.Nome ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Categoria", doc.Categoria ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CaminhoArquivo", doc.CaminhoArquivo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CaminhoMiniatura", doc.CaminhoMiniatura ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DataUpload", doc.DataUpload.ToString("o"));
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public List<Acompanhamento> GetAcompanhamentos(int chamadoId)
        {
            var lista = new List<Acompanhamento>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, ChamadoId, Texto, Data, Tipo FROM Acompanhamentos WHERE ChamadoId = @ChamadoId ORDER BY Data ASC";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChamadoId", chamadoId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Acompanhamento
                            {
                                Id = reader.GetInt32(0),
                                ChamadoId = reader.GetInt32(1),
                                Texto = reader.GetString(2),
                                Data = DateTime.Parse(reader.GetString(3)),
                                Tipo = reader.IsDBNull(4) ? "Acompanhamento" : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public void InsertAcompanhamento(Acompanhamento ac)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO Acompanhamentos (ChamadoId, Texto, Data, Tipo)
                    VALUES (@ChamadoId, @Texto, @Data, @Tipo);";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChamadoId", ac.ChamadoId);
                    command.Parameters.AddWithValue("@Texto", ac.Texto);
                    command.Parameters.AddWithValue("@Data", ac.Data.ToString("o"));
                    command.Parameters.AddWithValue("@Tipo", ac.Tipo);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
