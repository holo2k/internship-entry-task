using Application.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class DefaultRandomProvider : IRandomProvider
    {
        private readonly Random random = new Random();
        public double NextDouble() => random.NextDouble();
    }
}
