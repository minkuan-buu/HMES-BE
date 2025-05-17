using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.PhaseRepositories;

namespace HMES.Business.Services.PhaseServices;

public class PhaseServices: IPhaseServices
{
    private readonly IPhaseRepositories _phaseRepository;
    private readonly IMapper _mapper;
    
    public PhaseServices(IPhaseRepositories phaseRepository, IMapper mapper)
    {
        _phaseRepository = phaseRepository;
        _mapper = mapper;
    }


    public async Task<ResultModel<ListDataResultModel<PhaseResModel>>> GetAllPhasesAsync()
    {
        var (phases, totalItems) = await _phaseRepository.GetAllPhasesAsync();
        
        var totalPages = (int)Math.Ceiling((double)totalItems / 10);
        
        var result = new ListDataResultModel<PhaseResModel>
        {
            Data = _mapper.Map<List<PhaseResModel>>(phases),
            CurrentPage = 1,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = 10
        };
        return new ResultModel<ListDataResultModel<PhaseResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<ListDataResultModel<PhaseResModel>>> GetAllPhasesOfPlantAsync(Guid plantId)
    {
        var (phases, totalItems) = await _phaseRepository.GetAllPhasesOfPlantAsync(plantId);
        
        var totalPages = (int)Math.Ceiling((double)totalItems / 10);
        
        var result = new ListDataResultModel<PhaseResModel>
        {
            Data = _mapper.Map<List<PhaseResModel>>(phases),
            CurrentPage = 1,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = 10
        };
        return new ResultModel<ListDataResultModel<PhaseResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<DataResultModel<PhaseResModel>>> CreateNewPhaseAsync(AddNewPhaseDto newPhase, string? token)
    {
        
        // Create a new Phase entity
        var phase = new GrowthPhase()
        {
            Id = Guid.NewGuid(),
            Name = TextConvert.ConvertToUnicodeEscape(newPhase.Name.Trim()),
            Status = PhaseStatusEnums.Active.ToString(),
        };
        
        if(token != null)
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));
            
            var existedUserPhase = await _phaseRepository.GetGrowthPhaseByUserId(userId);
            if (existedUserPhase != null)
            {
                existedUserPhase.Name = TextConvert.ConvertToUnicodeEscape(newPhase.Name.Trim());
                await _phaseRepository.Update(existedUserPhase);
                return new ResultModel<DataResultModel<PhaseResModel>>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new DataResultModel<PhaseResModel>
                    {
                        Data = _mapper.Map<PhaseResModel>(existedUserPhase)
                    }
                };
            }
            else
            {
                phase.UserId = userId;
                await _phaseRepository.Insert(phase);
            }
        }
        
        // Check if the Phase with the same name already exists
        var existingPhase = await _phaseRepository.GetGrowthPhaseByName(TextConvert.ConvertToUnicodeEscape(newPhase.Name.Trim()));
        if (existingPhase != null)
        {
            throw new CustomException("Phase with the same name already exists");
        }

        await _phaseRepository.Insert(phase);
        var phaseDto = _mapper.Map<PhaseResModel>(phase);

        return new ResultModel<DataResultModel<PhaseResModel>>
        {
            StatusCodes = (int)HttpStatusCode.Created,
            Response = new DataResultModel<PhaseResModel>
            {
                Data = phaseDto
            }
        };
        
        
    }

    public async Task<ResultModel<DataResultModel<PhaseResModel>>> GetPhaseByIdAsync(Guid id)
    {
        var phase = await _phaseRepository.GetGrowthPhaseById(id);
        if (phase == null)
        {
            throw new CustomException("Phase not found");
        }
        
        var phaseDto = _mapper.Map<PhaseResModel>(phase);
        
        return new ResultModel<DataResultModel<PhaseResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new DataResultModel<PhaseResModel>
            {
                Data = phaseDto
            }
        };
    }

    public async Task<ResultModel<DataResultModel<PhaseResModel>>> UpdatePhaseAsync(Guid id, AddNewPhaseDto updatePhase)
    {
        var phase = await _phaseRepository.GetGrowthPhaseById(id);
        if (phase == null)
        {
            throw new CustomException("Phase not found");
        }
        
        // Check if the Phase with the same name already exists
        var existingPhase = await _phaseRepository.GetGrowthPhaseByName(TextConvert.ConvertToUnicodeEscape(updatePhase.Name.Trim()));
        if (existingPhase != null && existingPhase.Id != id)
        {
            throw new CustomException("Phase with the same name already exists");
        }

        phase.Name = TextConvert.ConvertToUnicodeEscape(updatePhase.Name.Trim());
        
        await _phaseRepository.Update(phase);
        
        var phaseDto = _mapper.Map<PhaseResModel>(phase);
        
        return new ResultModel<DataResultModel<PhaseResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new DataResultModel<PhaseResModel>
            {
                Data = phaseDto
            }
        };
    }
}