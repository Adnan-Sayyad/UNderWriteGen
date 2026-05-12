using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistributionAndPartyManagement.Models.Entities
{
	public class CustomerParty
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[MaxLength(20)]
		public string PartyID { get; set; } = string.Empty;

		[Required, MaxLength(20)]
		public string PartyType { get; set; } = string.Empty;

		[Required, MaxLength(200)]
		public string Name { get; set; } = string.Empty;

		public DateTime? DOBIncorporation { get; set; }

		[MaxLength(500)]
		public string? ContactInfo { get; set; }

		[Required, MaxLength(20)]
		public string Segment { get; set; } = string.Empty;

		[Required, MaxLength(20)]
		public string Status { get; set; } = "Active";

		[MaxLength(50)]
		public string? CreatedByUserId { get; set; }
	}
}
