﻿using System;

namespace serverTCC.Models
{
    public enum TipoMovimentacao
    {
        Receita,
        Despesa,
        Transferencia
    }

    public class Movimentacao
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public int ContaContabilId { get; set; }
        public int ContaDestinoId { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public int QtdTempo { get; set; }
        public int GrupoMovimentacoesId { get; set; }
        public DateTime Data { get; set; }
        public int MoedaId { get; set; }
        public string UsuarioId { get; set; }
        public EscalaTempo EscalaTempo { get; set; }
        public TipoMovimentacao TipoMovimentacao { get; set; }
        public ContaContabil ContaContabil { get; set; }
        public ContaContabil ContaDestino { get; set; }
        public Moeda Moeda { get; set; }
        public Usuario Usuario { get; set; }
        public GrupoMovimentacoes GrupoMovimentacoes { get; set; }
    }
}