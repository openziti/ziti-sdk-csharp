using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZitiRestServerCSharp.Models;

namespace ZitiRestServerCSharp.Controllers
{
    [Route("api/MetricItemsController")]
    [ApiController]
    public class MetricItemsController : ControllerBase
    {
        private readonly MetricContext _context;

        public MetricItemsController(MetricContext context)
        {
            _context = context;
        }

        // GET: api/MetricItemsController
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetricItem>>> GetMetricItems()
        {
          if (_context.MetricItems == null)
          {
              return NotFound();
          }
            return await _context.MetricItems.ToListAsync();
        }

        // GET: api/MetricItemsController/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MetricItem>> GetMetricItem(long id)
        {
            Console.WriteLine("Starting Metrics -> Get -> id: " + id);
            if (_context.MetricItems == null)
            {
                return NotFound();
            }
            var metricItem = await _context.MetricItems.FindAsync(id);

            if (metricItem == null)
            {
                return NotFound();
            }
            Console.WriteLine("Ending Metrics -> Get -> id: " + id);
            return metricItem;
        }

        // PUT: api/MetricItemsController/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMetricItem(long id, MetricItem metricItem)
        {
            if (id != metricItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(metricItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MetricItemExists(id))
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

        // POST: api/MetricItemsController
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MetricItem>> PostMetricItem(MetricItem metricItem)
        {
            Console.WriteLine("Starting Add MetricItem with values" + metricItem);
            if (_context.MetricItems == null)
          {
              return Problem("Entity set 'MetricContext.MetricItems'  is null.");
          }
            _context.MetricItems.Add(metricItem);
            await _context.SaveChangesAsync();

            Console.WriteLine("Creating values" + metricItem);
            return CreatedAtAction(nameof(GetMetricItem), new { id = metricItem.Id }, metricItem);
        }

        // DELETE: api/MetricItemsController/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMetricItem(long id)
        {
            if (_context.MetricItems == null)
            {
                return NotFound();
            }
            var metricItem = await _context.MetricItems.FindAsync(id);
            if (metricItem == null)
            {
                return NotFound();
            }

            _context.MetricItems.Remove(metricItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MetricItemExists(long id)
        {
            return (_context.MetricItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
