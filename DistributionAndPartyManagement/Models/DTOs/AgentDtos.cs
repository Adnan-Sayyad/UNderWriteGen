using System.ComponentModel.DataAnnotations;

namespace DistributionAndPartyManagement.Models.DTOs
{
	public class CreateAgentDto
	{
		[Required, MaxLength(150)]
		public string Name { get; set; } = string.Empty;

		[Required, MaxLength(50)]
		public string ProducerCode { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? ContactInfo { get; set; }

		[MaxLength(100)]
		public string? Region { get; set; }
	}

	public class UpdateAgentDto
	{
		[Required, MaxLength(150)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? ContactInfo { get; set; }

		[MaxLength(100)]
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
