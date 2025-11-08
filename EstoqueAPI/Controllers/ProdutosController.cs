using AutoMapper;
using EstoqueAPI.Data;
using EstoqueAPI.DTOs;
using EstoqueAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly EstoqueContext _context;
        private readonly IMapper _mapper;

        public ProdutosController(EstoqueContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // GET: api/Produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                return NotFound();
            }

            return produto;
        }

        // PUT: api/Produtos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.Codigo)
            {
                return BadRequest();
            }

            _context.Entry(produto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Produtos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(ProdutoInputDto produtoInputDto)
        {
            // Converte DTO em entidade
            var produto = _mapper.Map<Produto>(produtoInputDto);

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduto", new { id = produto.Codigo }, produto);
        }

        // DELETE: api/Produtos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Codigo == id);
        }

        [HttpPost("{codigo}/decrementar")]
        public async Task<IActionResult> DecrementarSaldo(int codigo, [FromBody] DecrementoDto dto)
        {
            if (dto == null || dto.Quantidade <= 0)
                return BadRequest("Quantidade inválida.");

            var rows = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Produtos SET Saldo = Saldo - {0} WHERE Codigo = {1} AND Saldo >= {0}",
                dto.Quantidade, codigo);

            if (rows == 0)
            {
                var exists = await _context.Produtos.AnyAsync(p => p.Codigo == codigo);
                if (!exists) return NotFound();
                return BadRequest("Saldo insuficiente.");
            }

            return Ok();
        }

        [HttpPost("{codigo}/incrementar")]
        public async Task<IActionResult> IncrementarSaldo(int codigo, [FromBody] DecrementoDto dto)
        {
            if (dto == null || dto.Quantidade <= 0)
                return BadRequest("Quantidade inválida.");

            var rows = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Produtos SET Saldo = Saldo + {0} WHERE Codigo = {1}",
                dto.Quantidade, codigo);

            if (rows == 0)
            {
                var exists = await _context.Produtos.AnyAsync(p => p.Codigo == codigo);
                if (!exists) return NotFound();
            }

            return Ok();
        }
    }
}
