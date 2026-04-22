using System.ComponentModel.DataAnnotations;

namespace DistributionAndPartyManagement.Models.DTOs
{
	public class CreateCustomerPartyDto
	{
		[Required, MaxLength(20)]
		public string PartyType { get; set; } = string.Empty;

		[Required, MaxLength(200)]
		public string Name { get; set; } = string.Empty;

		public DateTime? DOBIncorporation { get; set; }

		[MaxLength(500)]
		public string? ContactInfo { get; set; }

		[Required, MaxLength(20)]
		public string Segment { get; set; } = string.Empty;
	}

	public class UpdateCustomerPartyDto
	{
		[Required, MaxLength(200)]
		public string Name { get; set; } = string.Empty;

		public DateTime? DOBIncorporation { get; set; }

		[MaxLength(500)]
		public string? ContactInfo { get; set; }

		[Required, MaxLength(20)]
		public string Segment { get; set; } = string.Empty;
	}

	public class CustomerPartyResponseDto
	{
		public string PartyID { get; set; } = string.Empty;
		public string PartyType { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public DateTime? DOBIncorporation { get; set; }
		public string? ContactInfo { get; set; }
		public string Segment { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
	}
}
