using System.ComponentModel.DataAnnotations;
using DistributionAndPartyManagement.Validation;

namespace DistributionAndPartyManagement.Models.DTOs
{
	public class CreateAgentDto
	{
		[Required(ErrorMessage = "Name is required.")]
		[MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
		[MaxLength(150, ErrorMessage = "Name must not exceed 150 characters.")]
		[NoDigits(ErrorMessage = "Name must not contain numbers.")]
		[RegularExpression(@"^[a-zA-Z\s\-\'\.]+$",
			ErrorMessage = "Name may only contain letters, spaces, hyphens, apostrophes, and periods.")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "ProducerCode is required.")]
		[MinLength(3, ErrorMessage = "ProducerCode must be at least 3 characters.")]
		[MaxLength(50, ErrorMessage = "ProducerCode must not exceed 50 characters.")]
		[RegularExpression(@"^[a-zA-Z0-9\-_]+$",
			ErrorMessage = "ProducerCode may only contain letters, digits, hyphens, and underscores.")]
		public string ProducerCode { get; set; } = string.Empty;

		[SmartContactInfo]
		[MaxLength(500, ErrorMessage = "ContactInfo must not exceed 500 characters.")]
		public string? ContactInfo { get; set; }

		[MaxLength(100, ErrorMessage = "Region must not exceed 100 characters.")]
		[RegularExpression(@"^[a-zA-Z\s\-]+$",
			ErrorMessage = "Region may only contain letters, spaces, and hyphens.")]
		public string? Region { get; set; }
	}

	public class UpdateAgentDto
	{
		[Required(ErrorMessage = "Name is required.")]
		[MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
		[MaxLength(150, ErrorMessage = "Name must not exceed 150 characters.")]
		[NoDigits(ErrorMessage = "Name must not contain numbers.")]
		[RegularExpression(@"^[a-zA-Z\s\-\'\.]+$",
			ErrorMessage = "Name may only contain letters, spaces, hyphens, apostrophes, and periods.")]
		public string Name { get; set; } = string.Empty;

		[SmartContactInfo]
		[MaxLength(500, ErrorMessage = "ContactInfo must not exceed 500 characters.")]
		public string? ContactInfo { get; set; }

		[MaxLength(100, ErrorMessage = "Region must not exceed 100 characters.")]
		[RegularExpression(@"^[a-zA-Z\s\-]+$",
			ErrorMessage = "Region may only contain letters, spaces, and hyphens.")]
		public string? Region { get; set; }
	}

	public class AgentResponseDto
	{
		public string AgentID { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string ProducerCode { get; set; } = string.Empty;
		public string? ContactInfo { get; set; }
		public string? Region { get; set; }
		public string Status { get; set; } = string.Empty;
	}
}
