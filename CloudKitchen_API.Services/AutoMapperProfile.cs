using AutoMapper;
using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MenuItem, MenuItemListDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
        }
    }
}
