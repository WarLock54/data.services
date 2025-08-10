using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public partial class BaseValidator<E>:AbstractValidator<E> where E : class, new() 
    {
        public BaseValidator()
        {
            
        }
    }
}
