using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Models;




public class MyClass
{
    public DateTime date { get; set; }
    public decimal PaidPrice { get; set; }
}
