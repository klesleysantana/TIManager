using System;

namespace TIManager.Models
{
    public class Acompanhamento
    {
        public int Id { get; set; }
        public int ChamadoId { get; set; }
        public string Texto { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = "Acompanhamento";

        public Acompanhamento()
        {
            Data = DateTime.Now;
        }
    }
}
