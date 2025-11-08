using AutoMapper;
using FaturamentoAPI.Data;
using FaturamentoAPI.DTOs;
using FaturamentoAPI.Models;
using FaturamentoAPI.Models.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FaturamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotasFiscaisController : ControllerBase
    {
        private readonly FaturamentoContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string EstoqueApiBase = "https://localhost:7105/api/Produtos/";

        public NotasFiscaisController(FaturamentoContext context, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/NotasFiscais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotaFiscal>>> GetNotasFiscais()
        {
            return await _context.NotasFiscais.Include(n => n.Itens).ToListAsync();
        }

        // GET: api/NotasFiscais/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NotaFiscal>> GetNotaFiscal(int id)
        {
            var notaFiscal = await _context.NotasFiscais.Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id);

            if (notaFiscal == null)
                return NotFound();

            return notaFiscal;
        }

        // PUT: api/NotasFiscais/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotaFiscal(int id, NotaFiscalInputDto notaFiscalDto)
        {
            if (notaFiscalDto == null)
                return BadRequest();

            // Carrega nota e itens rastreados pelo EF
            var notaFiscal = await _context.NotasFiscais
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notaFiscal == null)
                return NotFound();

            // Opcional: só permite alterações quando a nota estiver aberta
            if (notaFiscal.Status != Status.Aberta)
                return BadRequest("Somente notas com status 'Aberta' podem ser alteradas.");

            // Mapa de itens existentes por ProdutoCodigo (ou por Id se preferir)
            var itensExistentesPorProduto = notaFiscal.Itens.ToDictionary(i => i.ProdutoCodigo);

            // Novos itens/quantidades vindos do cliente
            var itensRecebidos = notaFiscalDto.Itens ?? new List<ItemNotaFiscalInputDto>();

            // Lista para rastrear quais itens devem ser removidos
            var produtosRecebidos = itensRecebidos.Select(i => i.ProdutoCodigo).ToHashSet();
            var itensParaRemover = notaFiscal.Itens.Where(i => !produtosRecebidos.Contains(i.ProdutoCodigo)).ToList();

            // Remove itens que o cliente não enviou mais
            foreach (var itemRemover in itensParaRemover)
            {
                // Remove do DB e da coleção
                _context.Remove(itemRemover);
                notaFiscal.Itens.Remove(itemRemover);
            }

            // Atualiza itens existentes e adiciona novos itens
            foreach (var itemDto in itensRecebidos)
            {
                if (itensExistentesPorProduto.TryGetValue(itemDto.ProdutoCodigo, out var itemExistente))
                {
                    // Atualiza somente a quantidade (e outros campos permitidos)
                    itemExistente.Quantidade = itemDto.Quantidade;
                    // Se você tiver snapshot (NomeProduto, PrecoUnitario) e quiser atualizar, faça aqui.
                }
                else
                {
                    // Item novo — cria e adiciona à coleção (EF cuidará do NotaFiscalId)
                    var novoItem = new ItemNotaFiscal
                    {
                        ProdutoCodigo = itemDto.ProdutoCodigo,
                        Quantidade = itemDto.Quantidade
                        // NÃO atribua Id nem NotaFiscalId manualmente
                    };
                    notaFiscal.Itens.Add(novoItem);
                }
            }

            // Salva alterações (EF vai gerar INSERT/UPDATE/DELETE conforme necessário)
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotaFiscalExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        //POST
        [HttpPost]
        public async Task<ActionResult<NotaFiscal>> PostNotaFiscal(NotaFiscalInputDto notaFiscalInputDto)
        {
            var httpClient = _httpClientFactory.CreateClient("EstoqueApi");

            // Validar cada produto
            foreach (var item in notaFiscalInputDto.Itens)
            {
                var response = await httpClient.GetAsync($"{EstoqueApiBase}{item.ProdutoCodigo}");
                if (!response.IsSuccessStatusCode)
                {
                    // Produto não existe, retorna erro
                    return BadRequest($"Produto com código {item.ProdutoCodigo} não existe.");
                }
            }

            // Todos os produtos existem, mapeia para a entidade
            var notaFiscal = _mapper.Map<NotaFiscal>(notaFiscalInputDto);

            _context.NotasFiscais.Add(notaFiscal);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNotaFiscal", new { id = notaFiscal.Id }, notaFiscal);
        }


        // DELETE: api/NotasFiscais/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotaFiscal(int id)
        {
            var notaFiscal = await _context.NotasFiscais.FindAsync(id);
            if (notaFiscal == null)
            {
                return NotFound();
            }

            _context.NotasFiscais.Remove(notaFiscal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/NotasFiscais/Imprimir/5
        [HttpPatch("Imprimir/{id}")]
        public async Task<IActionResult> ImprimirNotaFiscal(int id)
        {
            // Garante um identificador único para rastreamento
            var requestId = Guid.NewGuid().ToString();

            var notaFiscal = await _context.NotasFiscais
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notaFiscal == null)
                return NotFound("Nota fiscal não encontrada.");

            if (notaFiscal.Status != Status.Aberta)
                return BadRequest("Somente notas fiscais com status 'Aberta' podem ser impressas.");

            var httpClient = _httpClientFactory.CreateClient("EstoqueApi");

            // Rastreia os produtos que foram atualizados (para possível reversão)
            var produtosAtualizados = new List<KeyValuePair<int, int>>();

            try
            {
                foreach (var item in notaFiscal.Itens)
                {
                    var dto = new { Quantidade = item.Quantidade };
                    var response = await httpClient.PostAsJsonAsync($"{EstoqueApiBase}{item.ProdutoCodigo}/decrementar", dto);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return BadRequest($"Produto {item.ProdutoCodigo} não encontrado.");

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var texto = await response.Content.ReadAsStringAsync();
                        return BadRequest($"Falha ao decrementar o produto {item.ProdutoCodigo}: {texto}");
                    }

                    if (!response.IsSuccessStatusCode)
                        return StatusCode(StatusCodes.Status503ServiceUnavailable, "Serviço de estoque indisponível.");

                    // Guarda o produto atualizado para eventual compensação
                    produtosAtualizados.Add(new KeyValuePair<int, int>(item.ProdutoCodigo, item.Quantidade));
                }

                // Tudo certo — fecha a nota fiscal
                notaFiscal.Status = Status.Fechada;
                _context.Entry(notaFiscal).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Nota fiscal impressa e fechada com sucesso.",
                    notaId = notaFiscal.Id,
                    requestId
                });
            }
            catch (Exception ex)
            {
                // Se algo falhar, reverte as alterações no estoque
                foreach (var p in produtosAtualizados)
                {
                    try
                    {
                        await httpClient.PostAsJsonAsync(
                            $"{EstoqueApiBase}{ p.Key}/incrementar",
                            new { Quantidade = p.Value }
                        );
                    }
                    catch
                    {

                    }
                }

                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    error = "Falha ao imprimir nota fiscal.",
                    detail = ex.Message,
                    requestId
                });
            }
        }

        private bool NotaFiscalExists(int id)
        {
            return _context.NotasFiscais.Any(e => e.Id == id);
        }
    }
}
