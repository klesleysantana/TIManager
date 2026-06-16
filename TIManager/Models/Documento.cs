using System;

namespace TIManager.Models
{
    public class Documento
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Categoria { get; set; } // "Wiki" ou "Relatorio"
        public string CaminhoArquivo { get; set; }
        public string CaminhoMiniatura { get; set; }
        public DateTime DataUpload { get; set; }

        public Documento()
        {
            DataUpload = DateTime.Now;
        }
    }
}
