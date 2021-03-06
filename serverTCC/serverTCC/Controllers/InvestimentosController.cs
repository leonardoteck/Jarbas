using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using serverTCC.Data;
using serverTCC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace serverTCC.Controllers
{
    [Produces("application/json")]
    [Route("api/Investimentos")]
    [Authorize]
    public class InvestimentosController : Controller
    {
        private JarbasContext context;

        public InvestimentosController(JarbasContext ctx)
        {
            context = ctx;
        }

        /// <summary>
        /// Cria um novo investimento
        /// POST api/Investimentos
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Investimento investimento)
        {
            try
            {
                bool usuarioExists = await context.Usuario.AnyAsync(u => u.Id.Equals(investimento.UsuarioId));

                if (usuarioExists)
                {
                    investimento.UltimaAtualizacao = investimento.DataInicio;
                    context.Investimento.Add(investimento);
                    await context.SaveChangesAsync();
                    return CreatedAtAction("Create", investimento);
                }
                else
                {
                    ModelState.AddModelError("Usuario", "Usu�rio n�o cadastrado no sistema.");
                    return NotFound(ModelState.Values.SelectMany(e => e.Errors));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Busca todas os investimentos do usu�rio
        /// GET api/Investimentos/Usuario/userId
        /// </summary>
        [HttpGet("Usuario/{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] string userId)
        {
            try
            {
                var investimentos = context.Investimento
                    .Include(i => i.Moeda)
                    .Include(i => i.TipoInvestimento)
                    .Include(i => i.ValoresInseridos)
                    .Where(i => i.UsuarioId.Equals(userId));

                foreach (var investimento in investimentos)
                {
                    investimento.ValorAtual = AtualizarValor(investimento, DateTime.Now);
                    investimento.ValorAtual = CalcularInseridos(investimento);
                }

                context.Investimento.UpdateRange(investimentos);

                await context.SaveChangesAsync();

                return Ok(investimentos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Busca um investimento especifico
        /// GET api/Investimentos/id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            try
            {
                var investimento = await context.Investimento
                    .Include(i => i.Moeda)
                    .Include(i => i.TipoInvestimento)
                    .Include(i => i.ValoresInseridos)
                    .FirstOrDefaultAsync(i => i.Id.Equals(id));


                if (investimento != null)
                {
                    investimento.ValorAtual = AtualizarValor(investimento, DateTime.Now);
                    investimento.ValorAtual = CalcularInseridos(investimento);
                    context.Investimento.Update(investimento);
                    await context.SaveChangesAsync();
                    return Ok(investimento);
                }
                else
                {
                    ModelState.AddModelError("Investimento", "Investimento n�o encontrado");
                    return NotFound(ModelState.Values.SelectMany(v => v.Errors));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Edita um investimento existente
        /// PUT api/Investimentos/ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] Investimento investimento)
        {
            try
            {
                var investimentoExists = await context.Investimento.AnyAsync(i => i.Id.Equals(id));

                if (investimentoExists)
                {
                    context.Investimento.Update(investimento);

                    await context.SaveChangesAsync();

                    return Ok(investimento);
                }
                else
                {
                    ModelState.AddModelError("Investimento", "Investimento n�o encontrado");
                    return NotFound(ModelState.Values.SelectMany(v => v.Errors));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Deleta um investimento existente
        /// DELETE api/Investimentos/ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var investimento = await context.Investimento.FirstOrDefaultAsync(i => i.Id.Equals(id));

                if (investimento != null)
                {
                    context.Investimento.Remove(investimento);

                    await context.SaveChangesAsync();

                    return Ok();
                }
                else
                {
                    ModelState.AddModelError("Investimento", "Investimento n�o encontrado");
                    return NotFound(ModelState.Values.SelectMany(v => v.Errors));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Prev� o valor de um investimento
        /// POST api/Investimentos/Prever/id
        /// </summary>
        [HttpPost("Prever/{id}")]
        public async Task<IActionResult> PreverValor([FromRoute]int id, [FromBody]DateTime data)
        {
            try
            {
                var investimento = await context.Investimento
                    .Include(i => i.Moeda)
                    .Include(i => i.TipoInvestimento)
                    .FirstOrDefaultAsync(i => i.Id.Equals(id));

                if (investimento != null)
                {
                    //O metodo para atualizar valor pode ser usado tamb�m para a previs�o,
                    //pois ele retorna o valor em decimal referente a uma data passada por par�metro 
                    decimal valorFuturo = AtualizarValor(investimento, data);
                    //Retorna o investimento e o valor futuro
                    return Ok(new { investimento, valorFuturo });
                }
                else
                {
                    ModelState.AddModelError("Investimento", "Investimento n�o encontrado");
                    return NotFound(ModelState.Values.SelectMany(v => v.Errors));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        /// <summary>
        /// Adiciona mais dinheiro a um investimento
        /// POST api/Investimentos/Inserir/id
        /// </summary>
        [HttpPost("Inserir/{id}")]
        public async Task<IActionResult> InserirDinheiro([FromRoute]int id, [FromBody]Decimal valor)
        {
            try
            {
                var investimento = await context.Investimento
                    .Include(i => i.Moeda)
                    .Include(i => i.TipoInvestimento)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (investimento == null)
                {
                    ModelState.AddModelError("Investimento", "Investimento n�o encontrado");
                    return NotFound(ModelState.Values.SelectMany(v => v.Errors));
                }

                investimento.ValorAtual += valor;
                var aux = new ValoresInseridos
                {
                    InvestimentoId = investimento.Id,
                    Data = DateTime.Now,
                    Valor = valor
                };
                context.ValoresInseridos.Add(aux);
      
                context.Investimento.Update(investimento);
                await context.SaveChangesAsync();
                return Ok(investimento);
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        /// <summary>
        /// Calcula o rendimento de valores inseridos ap�s o inicio do investimento
        /// </summary>
        private decimal CalcularInseridos(Investimento investimento)
        {
            int dias;
            decimal valorAux = 0m;
            double valorD;
            IList<ValoresInseridos> sairao = new List<ValoresInseridos>();
            if (!investimento.ValoresInseridos.Any())
                return investimento.ValorAtual;

            if (investimento.EscalaTempo == EscalaTempo.Diario)
            {
                foreach(var aux in investimento.ValoresInseridos)
                {
                    dias = TempoEmDias(aux.Data, DateTime.Now);
                    valorD = (double)aux.Valor * (Math.Pow(1 + investimento.TipoInvestimento.Taxa, (double)dias) - 1);
                    valorAux += GarantirPrecisao(valorD);
                    sairao.Add(aux);
                }
            }
            else if (investimento.EscalaTempo == EscalaTempo.Semanal)
            {
                foreach(var aux in investimento.ValoresInseridos)
                {
                    dias = TempoEmDias(aux.Data, DateTime.Now);
                    if(dias >= 7)
                    {
                        var semanas = (double)dias / 7.00;
                        valorD = (double)aux.Valor * (Math.Pow(1 + investimento.TipoInvestimento.Taxa, semanas) - 1);
                        valorAux += GarantirPrecisao(valorD);
                        sairao.Add(aux);
                    }
                }
            }
            else if (investimento.EscalaTempo == EscalaTempo.Quinzenal)
            {
                foreach (var aux in investimento.ValoresInseridos)
                {
                    dias = TempoEmDias(aux.Data, DateTime.Now);
                    if(dias >= 15)
                    {
                        var quinzenas = (double)dias / 15.00;
                        valorD = (double)aux.Valor * (Math.Pow(1 + investimento.TipoInvestimento.Taxa, quinzenas) - 1);
                        valorAux += GarantirPrecisao(valorD);
                        sairao.Add(aux);
                    }
                }
            }
            else if (investimento.EscalaTempo == EscalaTempo.Mensal)
            {
                foreach (var aux in investimento.ValoresInseridos)
                {
                    dias = TempoEmDias(aux.Data, DateTime.Now);
                    if (dias >= 30)
                    {
                        var meses = (double)dias / 30.00;
                        valorD = (double)aux.Valor * (Math.Pow(1 + investimento.TipoInvestimento.Taxa, meses) - 1);
                        valorAux += GarantirPrecisao(valorD);
                        sairao.Add(aux);
                    }
                }
            }
            else if(investimento.EscalaTempo == EscalaTempo.Anual)
            {
                foreach (var aux in investimento.ValoresInseridos)
                {
                    dias = TempoEmDias(aux.Data, DateTime.Now);
                    if(dias >= 360)
                    {
                        var anos = (double)dias / 360.00;
                        valorD = (double)aux.Valor * (Math.Pow(1 + investimento.TipoInvestimento.Taxa, anos) - 1);
                        valorAux += GarantirPrecisao(valorD);
                        sairao.Add(aux);
                    }
                }
            }
            investimento.ValorAtual += valorAux;
            context.ValoresInseridos.RemoveRange(sairao);
            return investimento.ValorAtual;
        }

        /// <summary>
        /// Transfere dinheiro de uma conta para um investimento
        /// POST api/Investimentos/TransferirFromConta/{contaId}/{investimentoId}
        /// </summary>
        [HttpPost("TransferirFromConta/{contaId}/{investimentoId}")]
        public async Task<IActionResult> TransferirFromConta([FromRoute] int contaId, [FromRoute] int investimentoId, [FromBody] decimal valor)
        {
            try
            {
                var contaController = new ContasContabeisController(context);
                var movController = new MovimentacoesController(context);

                var aux = await contaController.Get(contaId);
                if(!(aux is OkObjectResult contaObject))
                {
                    return aux;
                }
                var conta = contaObject.Value as ContaContabil;

                aux = await Get(investimentoId);
                if(!(aux is OkObjectResult investimentoObject))
                {
                    return aux;
                }
                var investimento = investimentoObject.Value as Investimento;

                if (!movController.VerificarSaldo(conta, valor))
                {
                    ModelState.AddModelError("Conta", "Saldo insuficiente.");
                    return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
                }

                conta.Saldo -= valor;
                await InserirDinheiro(investimento.Id, valor);
                context.ContaContabil.Update(conta);
                await context.SaveChangesAsync();
                return Ok(new { Investimento = investimento, Conta = conta });
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        /// <summary>
        /// Transfere dinheiro de um investimento para uma conta
        /// POST api/Investimentos/TransferirToConta/{contaId}/{investimentoId}
        /// </summary>
        [HttpPost("TransferirToConta/{contaId}/{investimentoId}")]
        public async Task<IActionResult> TransferirToConta([FromRoute] int contaId, [FromRoute] int investimentoId, [FromBody] decimal valor)
        {
            try
            {
                var contaController = new ContasContabeisController(context);
                var movController = new MovimentacoesController(context);

                var aux = await contaController.Get(contaId);
                if (!(aux is OkObjectResult contaObject))
                {
                    return aux;
                }
                var conta = contaObject.Value as ContaContabil;

                aux = await Get(investimentoId);
                if (!(aux is OkObjectResult investimentoObject))
                {
                    return aux;
                }
                var investimento = investimentoObject.Value as Investimento;

                if (!VerificarSaldo(investimento, valor))
                {
                    ModelState.AddModelError("Conta", "Saldo insuficiente.");
                    return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
                }

                conta.Saldo += valor;
                investimento.ValorAtual -= valor;

                context.ContaContabil.Update(conta);
                context.Investimento.Update(investimento);
                await context.SaveChangesAsync();
                return Ok(new { Investimento = investimento, Conta = conta });
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        /// <summary>
        /// Passa o valor de double para decimal, sem perder dados
        /// </summary>
        private decimal GarantirPrecisao(double valorD)
        {
            //Os passos abaixo s�o feitos para garantir a precis�o, o procedimento utilizado garante 2 digitos de precis�o
            valorD *= 100;

            //Trunca o valor para que o mesmo n�o seja arredondado, causando erro no valor real
            int valorI = (int)Math.Truncate(valorD);

            //Passa o valor para decimal e divide por 100 para voltar ao valor certo
            decimal valorM = new Decimal(valorI);
            valorM = decimal.Divide(valorM, 100);
            return valorM;
        }

        /// <summary>
        /// Atualiza o valor atual de um investimento
        /// </summary>
        private Decimal AtualizarValor(Investimento investimento, DateTime data)
        {
            int tempo = TempoEmDias(investimento.UltimaAtualizacao, data);

            switch (investimento.EscalaTempo)
            {
                case EscalaTempo.Semanal:
                    tempo = (int)(tempo / 7);
                    break;
                case EscalaTempo.Quinzenal:
                    tempo = (int)(tempo / 15);
                    break;
                case EscalaTempo.Mensal:
                    tempo = (int)(tempo / 30);
                    break;
                case EscalaTempo.Anual:
                    tempo = (int)(tempo / 360);
                    break;
            }

            if (tempo > 0)
            {
                investimento.UltimaAtualizacao = DateTime.Now;
                return CalcularValorFuturo(investimento, tempo);
            }
            else
            {
                return investimento.ValorAtual;
            }
        }

        private int TempoEmDias(DateTime data1, DateTime data2)
        {
            return (data2.Date - data1.Date).Days;
        }

        /// <summary>
        /// Calcula o valor do investimento para uma data especifica (� usado na previs�o e na atualiza��o dos valores do investimento)
        /// </summary>
        private Decimal CalcularValorFuturo(Investimento investimento, int tempo, bool isGet = false)
        {
            double tempoD = (double)tempo;
            double valorD;
            valorD = (double)investimento.ValorAtual * Math.Pow(1 + investimento.TipoInvestimento.Taxa, tempoD);
            return GarantirPrecisao(valorD);
        }

        /// <summary>
        /// Verifica o saldo de um investimento
        /// </summary>
        public bool VerificarSaldo(Investimento investimento, decimal valor)
        {
            if ((investimento.ValorAtual - valor) < 0)
            {
                return false;
            }
            return true;
        }
    }
}