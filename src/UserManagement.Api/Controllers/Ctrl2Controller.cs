using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Models;

namespace UserManagement.Api.Controllers;

// NOTA: controller creato apposta per un esercizio di code review.
// Contiene code smell introdotti volutamente: NON usare come riferimento.
[ApiController]
[Route("api/v1/[controller]")]
public class Ctrl2Controller : ControllerBase
{
    public static List<User> data = new();

    private readonly AppDbContext db;

    public Ctrl2Controller(AppDbContext context)
    {
        db = context;
    }

    [HttpGet]
    public IActionResult DoStuff()
    {
        try
        {
            var x = db.Users.ToList();
            var res = new List<object>();

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] != null)
                {
                    if (x[i].FirstName != null)
                    {
                        if (x[i].LastName != null)
                        {
                            res.Add(new
                            {
                                x[i].Id,
                                x[i].FirstName,
                                x[i].LastName,
                                x[i].Email,
                                x[i].CreatedAt,
                                x[i].UpdatedAt
                            });
                        }
                    }
                }
            }

            data = x;

            return Ok(res);
        }
        catch (Exception)
        {
        }

        return Ok();
    }

    [HttpGet("2")]
    public object GetData2(int flag)
    {
        var all = db.Users.ToList();

        if (flag == 1)
        {
            var res = new List<object>();
            foreach (var u in all)
            {
                res.Add(new { u.Id, u.FirstName, u.LastName, u.Email, u.CreatedAt, u.UpdatedAt });
            }
            return res;
        }
        else if (flag == 2)
        {
            var res = new List<object>();
            foreach (var u in all.Where(u => u.UpdatedAt != null))
            {
                res.Add(new { u.Id, u.FirstName, u.LastName, u.Email, u.CreatedAt, u.UpdatedAt });
            }
            return res;
        }

        return all;
    }

    [HttpGet("byname")]
    public async Task<IActionResult> Handle(string q)
    {
        var pattern = $"%{q}%";
        var result = await db.Users
            .Where(u => EF.Functions.Like(u.FirstName, pattern))
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("count")]
    public int GetCount()
    {
        return db.Users.ToList().Count;
    }
}
