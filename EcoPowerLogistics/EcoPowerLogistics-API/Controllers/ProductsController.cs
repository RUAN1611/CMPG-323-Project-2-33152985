using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoPowerLogistics_API.Models;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using EcoPowerLogistics_API.Models.DTO;

namespace EcoPowerLogistics_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ecopowerlogisticsdevContext _context;
        private readonly IMapper _mapper;

        public ProductsController(ecopowerlogisticsdevContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var products = await _context.Products.ToListAsync();
            return Ok(_mapper.Map<List<ProductDTO>>(products));
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(short id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductDTO>(product));
        }

        [HttpGet("product/{id}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByOrder(short id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }

            var products = await _context.Products
            .Include(p => p.OrderDetails)
            .Where(p => p.OrderDetails.Any(od => od.OrderId == id))
            .ToListAsync();

            if (products == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<ProductDTO>>(products));
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(short id, ProductDTO productDTO)
        {
            var product = _mapper.Map<Product>(productDTO);
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PatchProduct(int id, [FromBody] JsonPatchDocument<ProductDTO> patchProductDTO)
        {
            if (id <= 0 || patchProductDTO == null)
            {
                return BadRequest();
            }

            var productFromDb = _context.Products.FirstOrDefault(x => x.ProductId == id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            var productToPatch = _mapper.Map<ProductDTO>(productFromDb);

            patchProductDTO.ApplyTo(productToPatch, ModelState);

            if (!TryValidateModel(productToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(productToPatch, productFromDb);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> PostProduct(ProductDTO productDTO)
        {
            var product = _mapper.Map<Product>(productDTO);
            if (_context.Products == null)
            {
                return Problem("Entity set 'ecopowerlogisticsdevContext.Products'  is null.");
            }
            _context.Products.Add(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductExists(product.ProductId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(short id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(short id)
        {
            return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
