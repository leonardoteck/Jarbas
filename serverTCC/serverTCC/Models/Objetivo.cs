﻿
using System;
using System.Collections.Generic;

namespace serverTCC.Models
{
    public class Objetivo
    {
        public int Id { get; set; }

        public Usuario Usuario { get; set; }
        public string UsuarioId { get; set; }

        public decimal Valor { get; set; }
        public Moeda Moeda { get; set; }
        public int MoedaId { get; set; }

        public DateTime DataInicial { get; set; }

        public string Descricao { get; set; }

        public bool Arquivar { get; set; }

        public IEnumerable<HistoricoObjetivo> HistoricoObjetivo { get; set; }
    }
}
