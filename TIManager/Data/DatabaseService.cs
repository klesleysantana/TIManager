using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using TIManager.Models;

namespace TIManager.Data
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private readonly string _basePath;
        private readonly string _chamadosPath;
        private readonly string _documentosPath;
        private readonly string _acompanhamentosPath;

        private readonly JsonSerializerOptions _jsonOptions;

        private DatabaseService()
        {
            _basePath = @"\\192.168.10.152\ciras\TI\.TIManagerData";
            _chamadosPath = Path.Combine(_basePath, "Chamados.json");
            _documentosPath = Path.Combine(_basePath, "Documentos.json");
            _acompanhamentosPath = Path.Combine(_basePath, "Acompanhamentos.json");

            _jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Initialize()
        {
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            string docPath = Path.Combine(_basePath, "Documentos");
            if (!Directory.Exists(docPath)) Directory.CreateDirectory(docPath);

            string thumbPath = Path.Combine(_basePath, "Thumbnails");
            if (!Directory.Exists(thumbPath)) Directory.CreateDirectory(thumbPath);

            string imagePath = Path.Combine(_basePath, "Imagens");
            if (!Directory.Exists(imagePath)) Directory.CreateDirectory(imagePath);

            // Initialize empty files if they don't exist
            if (!File.Exists(_chamadosPath)) WriteJson(_chamadosPath, new List<Chamado>());
            if (!File.Exists(_documentosPath)) WriteJson(_documentosPath, new List<Documento>());
            if (!File.Exists(_acompanhamentosPath)) WriteJson(_acompanhamentosPath, new List<Acompanhamento>());
        }

        private T ReadJson<T>(string path) where T : new()
        {
            if (!File.Exists(path)) return new T();

            int retries = 5;
            while (retries > 0)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs))
                    {
                        string json = sr.ReadToEnd();
                        if (string.IsNullOrWhiteSpace(json)) return new T();
                        return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
                    }
                }
                catch (IOException)
                {
                    retries--;
                    if (retries == 0) throw;
                    Thread.Sleep(200); // Retry after short delay
                }
            }
            return new T();
        }

        private void WriteJson<T>(string path, T data)
        {
            int retries = 5;
            while (retries > 0)
            {
                try
                {
                    // Use FileShare.None to acquire exclusive write lock
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var json = JsonSerializer.Serialize(data, _jsonOptions);
                        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                        fs.Write(bytes, 0, bytes.Length);
                        return;
                    }
                }
                catch (IOException)
                {
                    retries--;
                    if (retries == 0) throw;
                    Thread.Sleep(200);
                }
            }
        }

        // --- Chamados ---

        public List<Chamado> GetAllChamados()
        {
            var chamados = ReadJson<List<Chamado>>(_chamadosPath);
            return chamados.OrderByDescending(c => c.DataAbertura).ToList();
        }

        public int InsertChamado(Chamado chamado)
        {
            var chamados = ReadJson<List<Chamado>>(_chamadosPath);
            int newId = chamados.Any() ? chamados.Max(c => c.Id) + 1 : 1;
            chamado.Id = newId;
            chamados.Add(chamado);
            WriteJson(_chamadosPath, chamados);
            return newId;
        }

        public void UpdateChamado(Chamado chamado)
        {
            var chamados = ReadJson<List<Chamado>>(_chamadosPath);
            var index = chamados.FindIndex(c => c.Id == chamado.Id);
            if (index != -1)
            {
                chamados[index] = chamado;
                WriteJson(_chamadosPath, chamados);
            }
        }

        public void DeleteChamado(int id)
        {
            // Delete related acompanhamentos
            var acompanhamentos = ReadJson<List<Acompanhamento>>(_acompanhamentosPath);
            acompanhamentos.RemoveAll(a => a.ChamadoId == id);
            WriteJson(_acompanhamentosPath, acompanhamentos);

            // Delete chamado
            var chamados = ReadJson<List<Chamado>>(_chamadosPath);
            var index = chamados.FindIndex(c => c.Id == id);
            if (index != -1)
            {
                chamados.RemoveAt(index);
                WriteJson(_chamadosPath, chamados);
            }
        }

        // --- Documentos ---

        public List<Documento> GetAllDocumentos(string categoria)
        {
            var docs = ReadJson<List<Documento>>(_documentosPath);
            return docs.Where(d => d.Categoria == categoria).OrderByDescending(d => d.DataUpload).ToList();
        }

        public int InsertDocumento(Documento doc)
        {
            var docs = ReadJson<List<Documento>>(_documentosPath);
            int newId = docs.Any() ? docs.Max(d => d.Id) + 1 : 1;
            doc.Id = newId;
            docs.Add(doc);
            WriteJson(_documentosPath, docs);
            return newId;
        }

        // --- Acompanhamentos ---

        public List<Acompanhamento> GetAcompanhamentos(int chamadoId)
        {
            var acomps = ReadJson<List<Acompanhamento>>(_acompanhamentosPath);
            return acomps.Where(a => a.ChamadoId == chamadoId).OrderBy(a => a.Data).ToList();
        }

        public void InsertAcompanhamento(Acompanhamento ac)
        {
            var acomps = ReadJson<List<Acompanhamento>>(_acompanhamentosPath);
            int newId = acomps.Any() ? acomps.Max(a => a.Id) + 1 : 1;
            ac.Id = newId;
            acomps.Add(ac);
            WriteJson(_acompanhamentosPath, acomps);
        }
    }
}
