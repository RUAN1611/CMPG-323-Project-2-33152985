using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoPowerLogistics_API.Models;
using Microsoft.AspNetCore.JsonPatch;
using EcoPowerLogistics_API.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace EcoPowerLogistics_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CustomersController : ControllerBase
    {
        private readonly ecopowerlogisticsdevContext _context;
        private readonly IMapper _mapper;

        public CustomersController(ecopowerlogisticsdevContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Customers
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers()
        {
            if (_context.Customers == null)
            {
                return NotFound();
            }
            var customers = await _context.Customers.ToListAsync();
            return Ok(_mapper.Map<List<CustomerDTO>>(customers));
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDTO>> GetCustomer(short id)
        {
            if (_context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CustomerDTO>(customer));
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCustomer(short id, CustomerDTO customerDTO)
        {

            var customer = _mapper.Map<Customer>(customerDTO);

            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(customerDTO);
        }

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDTO>> PostCustomer(CustomerDTO customerDTO)
        {
            var customer = _mapper.Map<Customer>(customerDTO);

            if (_context.Customers == null)
            {
                return Problem("Entity set 'ecopowerlogisticsdevContext.Customers'  is null.");
            }
            _context.Customers.Add(customer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CustomerExists(customer.CustomerId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PatchCustomer(int id, [FromBody] JsonPatchDocument<CustomerDTO> patchCustomerDTO)
        {
            if (id <= 0 || patchCustomerDTO == null)
            {
                return BadRequest();
            }

            var customerFromDb = _context.Customers.FirstOrDefault(x => x.CustomerId == id);

            if (customerFromDb == null)
            {
                return NotFound();
            }

            var customerToPatch = _mapper.Map<CustomerDTO>(customerFromDb);

            patchCustomerDTO.ApplyTo(customerToPatch, ModelState);

            if (!TryValidateModel(customerToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(customerToPatch, customerFromDb);

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomer(short id)
        {
            if (_context.Customers == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(short id)
        {
            return (_context.Customers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }
    }
}
