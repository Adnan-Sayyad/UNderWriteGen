using System.ComponentModel.DataAnnotations;
using DistributionAndPartyManagement.Validation;

namespace DistributionAndPartyManagement.Models.DTOs
{
	public class CreateCustomerPartyDto
	{
		[Required(ErrorMessage = "PartyType is required.")]
		[AllowedValues("Individual", "Corporation",
			ErrorMessage = "PartyType must be 'Individual' or 'Corporation'.")]
		public string PartyType { get; set; } = string.Empty;

		[Required(ErrorMessage = "Name is required.")]
		[MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
		[MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
		[NoDigits(ErrorMessage = "Name must not contain numbers.")]
		[RegularExpression(@"^[a-zA-Z\s\-\'\.&,]+$",
			ErrorMessage = "Name may only contain letters, spaces, hyphens, apostrophes, periods, ampersands, and commas.")]
		public string Name { get; set; } = string.Empty;

		[ValidDOB]
		public DateTime? DOBIncorporation { get; set; }

		[SmartContactInfo]
		[MaxLength(500, ErrorMessage = "ContactInfo must not exceed 500 characters.")]
		public string? ContactInfo { get; set; }

		[Required(ErrorMessage = "Segment is required.")]
		[AllowedValues("Retail", "Corporate", "SME",
			ErrorMessage = "Segment must be 'Retail', 'Corporate', or 'SME'.")]
		public string Segment { get; set; } = string.Empty;
	}

	public class UpdateCustomerPartyDto
	{
		[Required(ErrorMessage = "Name is required.")]
		[MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
		[MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
		[NoDigits(ErrorMessage = "Name must not contain numbers.")]
		[RegularExpression(@"^[a-zA-Z\s\-\'\.&,]+$",
			ErrorMessage = "Name may only contain letters, spaces, hyphens, apostrophes, periods, ampersands, and commas.")]
		public string Name { get; set; } = string.Empty;

		[ValidDOB]
		public DateTime? DOBIncorporation { get; set; }

		[SmartContactInfo]
		[MaxLength(500, ErrorMessage = "ContactInfo must not exceed 500 characters.")]
		public string? ContactInfo { get; set; }

		[Required(ErrorMessage = "Segment is required.")]
		[AllowedValues("Retail", "Corporate", "SME",
			ErrorMessage = "Segment must be 'Retail', 'Corporate', or 'SME'.")]
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
		public string? CreatedByUserId { get; set; }
	}
}
