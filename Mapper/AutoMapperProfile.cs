using AutoMapper;
using TelemarketingControlSystem.Models.Auth;
using static TelemarketingControlSystem.Services.Auth.AuthModels;



namespace TelemarketingControlSystem.Mapper
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			//-----------------------------AUTH-------------------------------------------
			CreateMap<UserTenantRole, UserTenantDto>()
				.ForMember(dest => dest.userName, opt => opt.MapFrom(src => src.UserName))
				.ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name))
				.ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))

				.ReverseMap();

			CreateMap<GroupTenantRole, GroupTenantDto>()
					.ForMember(dest => dest.groupName, opt => opt.MapFrom(src => src.GroupName))
					.ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name))
					.ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
					.ReverseMap();

		}
	}
}
