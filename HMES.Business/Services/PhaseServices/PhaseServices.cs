using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Authentication;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Enums;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Repositories.PhaseRepositories;
using HMES.Data.Repositories.PlantOfPhaseRepositories;
using HMES.Data.Repositories.PlantRepositories;
using HMES.Data.Repositories.TargetOfPhaseRepositories;

namespace HMES.Business.Services.PhaseServices;

public class PhaseServices : IPhaseServices
{
    private readonly IPhaseRepositories _phaseRepository;
    private readonly IPlantRepositories _plantRepositories;
    private readonly IPlantOfPhaseRepositories _plantOfPhaseRepository;
    private readonly IDeviceItemsRepositories _deviceItemsRepositories;
    private readonly ITargetOfPhaseRepository _targetOfPhaseRepository;
    private readonly IMapper _mapper;

    public PhaseServices(ITargetOfPhaseRepository targetOfPhaseRepository, IDeviceItemsRepositories deviceItemsRepositories, IPhaseRepositories phaseRepository, IMapper mapper, IPlantOfPhaseRepositories plantOfPhaseRepository, IPlantRepositories plantRepositories)
    {
        _phaseRepository = phaseRepository;
        _mapper = mapper;
        _plantOfPhaseRepository = plantOfPhaseRepository;
        _plantRepositories = plantRepositories;
        _deviceItemsRepositories = deviceItemsRepositories;
        _targetOfPhaseRepository = targetOfPhaseRepository;
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

    public async Task<ResultModel<ListDataResultModel<PhaseResModel>>> GetAllPhasesIncludeUserAsync(Guid plantId, string token)
    {
        var userId = new Guid(Authentication.DecodeToken(token, "userid"));

        var (phases, totalItems) = await _plantOfPhaseRepository.GetPhasesByPlantId(plantId, userId);

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

        var totalPages = (int)Math.Ceiling((double)totalItems / 1000);

        var result = new ListDataResultModel<PhaseResModel>
        {
            Data = _mapper.Map<List<PhaseResModel>>(phases),
            CurrentPage = 1,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = 1000
        };
        return new ResultModel<ListDataResultModel<PhaseResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<DataResultModel<PhaseResModel>>> CreateNewPhaseAsync(AddNewPhaseDto? newPhase, string? token)
    {

        // Create a new Phase entity
        var phase = new GrowthPhase()
        {
            Id = Guid.NewGuid(),
            Name = (newPhase != null && !string.IsNullOrWhiteSpace(newPhase.Name))
                ? TextConvert.ConvertToUnicodeEscape(newPhase.Name.Trim())
                : null,
            Status = PhaseStatusEnums.Active.ToString(),
        };

        if (token != null)
        {
            Guid userId = new Guid(Authentication.DecodeToken(token, "userid"));

            var existedUserPhase = await _phaseRepository.GetGrowthPhaseByUserId(userId);
            if (existedUserPhase == null)
            {
                phase.UserId = userId;
                phase.Name = null;
                await _phaseRepository.Insert(phase);
            }

            return new ResultModel<DataResultModel<PhaseResModel>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<PhaseResModel>
                {
                    Data = _mapper.Map<PhaseResModel>(existedUserPhase ?? phase)
                }
            };
        }

        // Check if the Phase with the same name already exists
        var existingPhase = await _phaseRepository.GetGrowthPhaseByName(TextConvert.ConvertToUnicodeEscape(newPhase.Name.Trim()));
        if (existingPhase != null)
        {
            throw new CustomException("Phase with the same name already exists");
        }

        var count = await _phaseRepository.CountGrowthPhase();
        switch (count)
        {
            case 0:
                phase.IsDefault = true;
                phase.PhaseNumber = 1;
                break;
            case 1:
                phase.IsDefault = true;
                phase.PhaseNumber = 2;
                break;
            case 2:
                phase.IsDefault = true;
                phase.PhaseNumber = 3;
                break;
            default:
                phase.IsDefault = false;
                phase.PhaseNumber = count + 1;
                break;
        }
        
        var defaultCount = await _phaseRepository.CountDefaultGrowthPhase();
        if (defaultCount < 3)
        {
            phase.IsDefault = true;
            var plants = await _plantRepositories.GetAllPlants();
            List<PlantOfPhase> plantsOfPhase = new List<PlantOfPhase>();
            if (plants.Count > 0)
            {
                foreach (var plant in plants)
                {
                    var plantOfPhase = new PlantOfPhase
                    {
                        Id = Guid.NewGuid(),
                        PlantId = plant.Id,
                        PhaseId = phase.Id
                    };
                    plantsOfPhase.Add(plantOfPhase);
                }
                if (plantsOfPhase.Count > 0)
                {
                    await _plantOfPhaseRepository.InsertRange(plantsOfPhase);
                }
            }
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
        var existingPhase =
            await _phaseRepository.GetGrowthPhaseByName(TextConvert.ConvertToUnicodeEscape(updatePhase.Name.Trim()));
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

    public async Task<ResultModel<DataResultModel<PlantAndPhaseForTargetListDto>>> SetPhaseForPlant(Guid plantId, Guid phaseId)
    {

        var plant = await _plantRepositories.GetByIdAsync(plantId);
        if (plant == null)
        {
            throw new CustomException("Plant not found");
        }

        var phase = await _phaseRepository.GetGrowthPhaseById(phaseId);
        if (phase == null)
        {
            throw new CustomException("Phase not found");
        }

        var existingPlantOfPhase = await _plantOfPhaseRepository.GetPlantOfPhasesByPlantIdAndPhaseId(plantId, phaseId);
        if (existingPlantOfPhase != null)
        {
            return new ResultModel<DataResultModel<PlantAndPhaseForTargetListDto>>
            {
                StatusCodes = (int)HttpStatusCode.OK,
                Response = new DataResultModel<PlantAndPhaseForTargetListDto>
                {
                    Data = null
                }
            };
        }

        var plantOfPhase = new PlantOfPhase
        {
            Id = Guid.NewGuid(),
            PlantId = plantId,
            PhaseId = phaseId
        };
        await _plantOfPhaseRepository.Insert(plantOfPhase);

        var plantAndPhase = new PlantAndPhaseForTargetListDto
        {
            PlantOfPhaseId = plantOfPhase.Id,
            PlantId = plant.Id,
            PlantName = plant.Name,
            PhaseId = phase.Id,
            PhaseName = phase.Name,
        };

        return new ResultModel<DataResultModel<PlantAndPhaseForTargetListDto>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new DataResultModel<PlantAndPhaseForTargetListDto>
            {
                Data = plantAndPhase
            }
        };

    }

    public async Task<ResultModel<MessageResultModel>> DeletePhaseAsync(Guid id)
    {
        var phase = await _phaseRepository.GetGrowthPhaseByIdWithTargetValue(id);
        if (phase == null)
        {
            throw new CustomException("Phase not found");
        }

        // Check if the phase is default
        if (phase.IsDefault != null)
        {
            if ((bool)phase.IsDefault)
            {
                throw new CustomException("Cannot delete default phase");
            }
        }
        
        // Check if the phase is used by any plant
        if (await _deviceItemsRepositories.CheckDeviceItemByPhaseId(id))
        {
            throw new CustomException("Cannot delete because there are device items using this phase");
        }
        try
        {
            if (phase.PlantOfPhases.Count > 0)
            {
                var plantOfPhasesToDelete = phase.PlantOfPhases.ToList();
                foreach (var plantOfPhase in plantOfPhasesToDelete)
                {
                    if (plantOfPhase.TargetOfPhases.Count > 0)
                    {
                        await _targetOfPhaseRepository.DeleteRange(plantOfPhase.TargetOfPhases);
                    }
                    await _plantOfPhaseRepository.Delete(plantOfPhase);
                }
            }
            await _phaseRepository.Delete(phase);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel
            {
                Message = "Phase deleted successfully"
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> UpdateStatusPhaseAsync(Guid id)
    {
        var phase = await _phaseRepository.GetGrowthPhaseById(id);
        if (phase == null)
        {
            throw new CustomException("Phase not found");
        }
        if (phase.IsDefault != null)
        {
            if ((bool)phase.IsDefault)
            {
                throw new CustomException("Cannot update status of default phase");
            }
        }
        phase.Status = phase.Status == PhaseStatusEnums.Active.ToString() ? PhaseStatusEnums.Inactive.ToString() : PhaseStatusEnums.Active.ToString();
        
        await _phaseRepository.Update(phase);

        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel
            {
                Message = "Phase status updated successfully"
            }
        };
    }
}