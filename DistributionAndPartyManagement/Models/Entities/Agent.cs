using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistributionAndPartyManagement.Models.Entities
{
	public class Agent
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[MaxLength(20)]
		public string AgentID { get; set; } = string.Empty;

		[Required, MaxLength(150)]
		public string Name { get; set; } = string.Empty;

		[Required, MaxLength(50)]
		public string ProducerCode { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? ContactInfo { get; set; }

		[MaxLength(100)]
		public string? Region { get; set; }

		[Required, MaxLength(20)]
		public string Status { get; set; } = "Active";
	}
}
