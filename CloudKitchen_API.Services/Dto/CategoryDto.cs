using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Dto
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int? DisplayOrder { get; set; }

    }
}
