using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Services
{
    public class ReferralMatrixService : IReferralMatrixService
    {
        private readonly IReferralMatrixRepository _repository;

        public ReferralMatrixService(IReferralMatrixRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ReferralMatrixResponseDto>> GetAllMatricesAsync()
        {
            var matrices = await _repository.GetAllAsync();
            return matrices.Select(MapToResponseDto);
        }

        public async Task<ReferralMatrixResponseDto?> GetMatrixByIdAsync(Guid id)
        {
            var matrix = await _repository.GetByIdAsync(id);
            return matrix is null ? null : MapToResponseDto(matrix);
        }

        public async Task<IEnumerable<ReferralMatrixResponseDto>> GetMatricesByProductLineAsync(string productLine)
        {
            var matrices = await _repository.GetByProductLineAsync(productLine);
            return matrices.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ReferralMatrixResponseDto>> GetActiveMatricesAsync()
        {
            var matrices = await _repository.GetActiveMatricesAsync();
            return matrices.Select(MapToResponseDto);
        }

        public async Task<ReferralMatrixResponseDto> CreateMatrixAsync(CreateReferralMatrixDto dto)
        {
            var matrix = new ReferralMatrix
            {
                ProductLine = dto.ProductLine,
                CriteriaJSON = dto.CriteriaJSON,
                Operator = dto.Operator,
                Threshold = dto.Threshold,
                RequiredAuthority = dto.RequiredAuthority,
                Status = dto.Status
            };

            var created = await _repository.CreateAsync(matrix);
            return MapToResponseDto(created);
        }

        public async Task<ReferralMatrixResponseDto?> UpdateMatrixAsync(Guid id, UpdateReferralMatrixDto dto)
        {
            var matrix = new ReferralMatrix
            {
                ReferralMatrixID = id,
                ProductLine = dto.ProductLine,
                CriteriaJSON = dto.CriteriaJSON,
                Operator = dto.Operator,
                Threshold = dto.Threshold,
                RequiredAuthority = dto.RequiredAuthority,
                Status = dto.Status
            };

            var updated = await _repository.UpdateAsync(matrix);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<ReferralMatrixResponseDto?> UpdateMatrixStatusAsync(Guid id, UpdateMatrixStatusDto dto)
        {
            var updated = await _repository.UpdateStatusAsync(id, dto.Status);
            return updated is null ? null : MapToResponseDto(updated);
        }

        public async Task<bool> DeleteMatrixAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static ReferralMatrixResponseDto MapToResponseDto(ReferralMatrix matrix) => new()
        {
            ReferralMatrixID = matrix.ReferralMatrixID,
            ProductLine = matrix.ProductLine,
            CriteriaJSON = matrix.CriteriaJSON,
            Operator = matrix.Operator,
            Threshold = matrix.Threshold,
            RequiredAuthority = matrix.RequiredAuthority,
            Status = matrix.Status
        };
    }
}
