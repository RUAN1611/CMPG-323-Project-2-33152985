﻿using System;
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
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace EcoPowerLogistics_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OrdersController : ControllerBase
    {
        private readonly ecopowerlogisticsdevContext _context;
        private readonly IMapper _mapper;

        public OrdersController(ecopowerlogisticsdevContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Orders
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var orders = await _context.Orders.ToListAsync();
            return Ok(_mapper.Map<List<OrderDTO>>(orders));
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> GetOrder(short id)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpGet("customer/{id}", Name = "GetOrderByCustomer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<OrderDTO>>> GetOrderByCustomer(short id)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.Where(x => x.CustomerId == id).ToListAsync();

            if (orders == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<OrderDTO>>(orders));
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutOrder(short id, OrderDTO orderDTO)
        {
            var order = _mapper.Map<Order>(orderDTO);
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == orderDTO.CustomerId);

            order.Customer = customer;

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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
        public async Task<IActionResult> PatchOrder(int id, [FromBody] JsonPatchDocument<OrderDTO> patchOrderDTO)
        {
            if (id <= 0 || patchOrderDTO == null)
            {
                return BadRequest();
            }

            var orderFromDb = _context.Orders.FirstOrDefault(x => x.OrderId == id);

            if (orderFromDb == null)
            {
                return NotFound();
            }

            var orderToPatch = _mapper.Map<OrderDTO>(orderFromDb);

            patchOrderDTO.ApplyTo(orderToPatch, ModelState);

            if (!TryValidateModel(orderToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(orderToPatch, orderFromDb);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDTO>> PostOrder(OrderDTO orderDTO)
        {
            var order = _mapper.Map<Order>(orderDTO);
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ecopowerlogisticsdevContext.Orders'  is null.");
            }

            var customer = _context.Customers.FirstOrDefault(x => x.CustomerId == orderDTO.CustomerId);

            order.Customer = customer;

            _context.Orders.Add(order);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OrderExists(order.OrderId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(short id)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(short id)
        {
            return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
        }
    }
}
