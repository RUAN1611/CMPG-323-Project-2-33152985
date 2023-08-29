using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoPowerLogistics_API.Models;
using AutoMapper;
using EcoPowerLogistics_API.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace EcoPowerLogistics_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ecopowerlogisticsdevContext _context;
        private readonly IMapper _mapper;

        public OrderDetailsController(ecopowerlogisticsdevContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/OrderDetails
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDetailDTO>>> GetOrderDetails()
        {
            if (_context.OrderDetails == null)
            {
                return NotFound();
            }
            var orderDetails = await _context.OrderDetails.ToListAsync();
            return Ok(_mapper.Map<List<OrderDetailDTO>>(orderDetails));
        }

        // GET: api/OrderDetails/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(short id)
        {
            if (_context.OrderDetails == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
            .Include(od => od.Order)
                .ThenInclude(o => o.Customer)
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.OrderDetailsId == id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            return Ok(orderDetail);
        }

        // PUT: api/OrderDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutOrderDetail(short id, OrderDetailDTO orderDetailDTO)
        {
            var orderDetail = _mapper.Map<OrderDetail>(orderDetailDTO);
            if (id != orderDetail.OrderDetailsId)
            {
                return BadRequest();
            }

            _context.Entry(orderDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderDetailExists(id))
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

        // POST: api/OrderDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDetailDTO>> PostOrderDetail(OrderDetailDTO orderDetailDTO)
        {
            var orderDetail = _mapper.Map<OrderDetail>(orderDetailDTO);
            if (_context.OrderDetails == null)
            {
                return Problem("Entity set 'ecopowerlogisticsdevContext.OrderDetails'  is null.");
            }

            // Fetch the order and product from the database
            var order = await _context.Orders.FindAsync(orderDetail.OrderId);
            var product = await _context.Products.FindAsync(orderDetail.ProductId);

            // Check if order and product exist
            if (order == null || product == null)
            {
                return NotFound();
            }

            // Assign the order and product to the orderDetail
            orderDetail.Order = order;
            orderDetail.Product = product;

            _context.OrderDetails.Add(orderDetail);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OrderDetailExists(orderDetail.OrderDetailsId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOrderDetail", new { id = orderDetail.OrderDetailsId }, orderDetail);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PatchOrderDetail(int id, [FromBody] JsonPatchDocument<OrderDetailDTO> patchOrderDetailDTO)
        {
            if (id <= 0 || patchOrderDetailDTO == null)
            {
                return BadRequest();
            }

            var orderDetailFromDb = _context.OrderDetails.FirstOrDefault(x => x.OrderDetailsId == id);

            if (orderDetailFromDb == null)
            {
                return NotFound();
            }

            var orderDetailToPatch = _mapper.Map<OrderDetailDTO>(orderDetailFromDb);

            patchOrderDetailDTO.ApplyTo(orderDetailToPatch, ModelState);

            if (!TryValidateModel(orderDetailToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(orderDetailToPatch, orderDetailFromDb);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/OrderDetails/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrderDetail(short id)
        {
            if (_context.OrderDetails == null)
            {
                return NotFound();
            }
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderDetailExists(short id)
        {
            return (_context.OrderDetails?.Any(e => e.OrderDetailsId == id)).GetValueOrDefault();
        }
    }
}
