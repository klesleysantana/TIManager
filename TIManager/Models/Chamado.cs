using System;

namespace TIManager.Models
{
    public class Chamado
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Solicitante { get; set; }
        public string Setor { get; set; }
        public string Prioridade { get; set; }
        public string Descricao { get; set; }
        public string Status { get; set; }
        public string CaminhoImagem { get; set; }
        public DateTime DataAbertura { get; set; }
        public DateTime? DataFinalizacao { get; set; }

        public Chamado()
        {
            Status = "Aberto";
            DataAbertura = DateTime.Now;
        }
    }
}
