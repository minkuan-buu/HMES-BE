using System.Net;
using AutoMapper;
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
using HMES.Data.Repositories.TargetValueRepositories;

namespace HMES.Business.Services.PlantServices;

public class PlantServices : IPlantServices
{
    
    private readonly IPlantRepositories _plantRepositories;
    private readonly ITargetOfPhaseRepository _targetOfPhaseRepository;
    private readonly ITargetValueRepositories _targetValueRepositories;
    private readonly IPhaseRepositories _phaseRepositories;
    private readonly IPlantOfPhaseRepositories _plantOfPhaseRepositories;
    private readonly IDeviceItemsRepositories _deviceItemsRepositories;

    private readonly IMapper _mapper;
    
    public PlantServices(IDeviceItemsRepositories deviceItemsRepositories,IPlantOfPhaseRepositories plantOfPhaseRepositories  ,IPhaseRepositories phaseRepositories,IPlantRepositories plantRepositories, ITargetValueRepositories targetValueRepositories, IMapper mapper, ITargetOfPhaseRepository targetOfPhaseRepository)
    {
        _plantRepositories = plantRepositories;
        _mapper = mapper;
        _targetOfPhaseRepository = targetOfPhaseRepository;
        _targetValueRepositories = targetValueRepositories;
        _phaseRepositories = phaseRepositories;
        _plantOfPhaseRepositories = plantOfPhaseRepositories;
        _deviceItemsRepositories = deviceItemsRepositories;
    }
    
    
    public async Task<ResultModel<ListDataResultModel<PlantResModel>>> GetAllPlantsAsync(string? keyword, string? status, int pageIndex, int pageSize)
    {
        
        var encodeKeyword = TextConvert.ConvertToUnicodeEscape(keyword??string.Empty);
        
        var (plants, totalItems) = await _plantRepositories.GetAllPlantsAsync(encodeKeyword,  status, pageIndex, pageSize);
        
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        
        var result = new ListDataResultModel<PlantResModel>
        {
            Data = _mapper.Map<List<PlantResModel>>(plants),
            CurrentPage = pageIndex,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };
        return new ResultModel<ListDataResultModel<PlantResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<DataResultModel<PlantResModelWithTarget>>> GetByIdAsync(Guid id)
    {
        var plant = await _plantRepositories.GetByIdNotIncludeUserAsync(id);
        
        if (plant == null)
        {
            return new ResultModel<DataResultModel<PlantResModelWithTarget>>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = null
            };
        }
        var plantDto = _mapper.Map<PlantResModelWithTarget>(plant);
        
        var result = new DataResultModel<PlantResModelWithTarget>
        {
            Data = plantDto
        };
        return new ResultModel<DataResultModel<PlantResModelWithTarget>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
        
    }

    public async Task<ResultModel<DataResultModel<PlantResModel>>> CreatePlantAsync(PlantReqModel plantReqModel)
    {
        if(plantReqModel.Status.ToString() == null || plantReqModel.Name == null)
        {
            return new ResultModel<DataResultModel<PlantResModel>>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = null
            };
        }
        
        var plant = _mapper.Map<Plant>(plantReqModel);

        var phases = await _phaseRepositories.GetGrowthPhasesNoUser();

        List<PlantOfPhase> plantOfPhases = new List<PlantOfPhase>();
        foreach (var phase in phases)
        {
            var plantOfPhase = new PlantOfPhase
            {
                Id = Guid.NewGuid(),
                PlantId = plant.Id,
                PhaseId = phase.Id
            };
            plantOfPhases.Add(plantOfPhase);
        }
        
        plant.PlantOfPhases = plantOfPhases;
        
        try
        {
            await _plantRepositories.Insert(plant);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        var plantDto = _mapper.Map<PlantResModel>(plant);
        
        var result = new DataResultModel<PlantResModel>
        {
            Data = plantDto
        };
        return new ResultModel<DataResultModel<PlantResModel>>
        {
            StatusCodes = (int)HttpStatusCode.Created,
            Response = result
        };
    }

    public async Task<ResultModel<DataResultModel<PlantResModel>>> UpdatePlantAsync(Guid id, PlantReqModel plantReqModel)
    {
        
        if(plantReqModel.Status.ToString() == null && plantReqModel.Name == null)
        {
            return new ResultModel<DataResultModel<PlantResModel>>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = null
            };
        }
        
        var plant = await _plantRepositories.GetByIdAsync(id);
        
        if (plant == null)
        {
            return new ResultModel<DataResultModel<PlantResModel>>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = null
            };
        }
        
        if(plantReqModel.Status.ToString() != null)
        {
            plant.Status = plantReqModel.Status.ToString();
        }
        if (plantReqModel.Name != null)
        {
            plant.Name = TextConvert.ConvertToUnicodeEscape(plantReqModel.Name);
        }
        
        
        try
        {
            await _plantRepositories.Update(plant);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        var plantDto = _mapper.Map<PlantResModel>(plant);
        
        var result = new DataResultModel<PlantResModel>
        {
            Data = plantDto
        };
        return new ResultModel<DataResultModel<PlantResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<MessageResultModel>> DeletePlantAsync(Guid id)
    {
        var plant = await _plantRepositories.GetByIdAsync(id);
        
        if (plant == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant not found"
                }
            };
        }
        try
        {
            // var targetsOfPlant = plant.TargetOfPlants.ToList();
            //
            // if (targetsOfPlant.Count > 0)
            // {
            //     await _targetOfPlantRepository.DeleteRange(targetsOfPlant);
            // }
            plant.Status = PlantStatusEnums.Inactive.ToString();
            await _plantRepositories.Update(plant);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        var result = new MessageResultModel
        {
            Message = "Plant deleted successfully"
        };
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<MessageResultModel>> SetValueForPlant(Guid plantId, Guid targetId, Guid phaseId)
    {
        var plant = await _plantRepositories.GetByIdAsync(plantId);
        
        if (plant == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant not found"
                }
            };
        }
        
        var target = await _targetValueRepositories.GetTargetValueById(targetId);
        
        if (target == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Target value not found"
                }
            };
        }
        
        
        if(await _plantRepositories.PlantHasTargetValueType(plantId, target.Type, phaseId))
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = new MessageResultModel()
                {
                    Message = "Plant already has this target value type"
                }
            };
        }
        
        var plantPhase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(plantId, phaseId);
        
        if (plantPhase == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant does not have this phase"
                }
            };
        }
        
        var targetOfPhase = new TargetOfPhase
        {
            Id = Guid.NewGuid(),
            PlantOfPhaseId = plantPhase.Id,
            TargetValueId = targetId
        };
        
        try
        {
            await _targetOfPhaseRepository.Insert(targetOfPhase);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Set target value for plant successfully"
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> SetValueForCustomPhase(Guid plantId, Guid targetId, Guid phaseId)
    {
        var plant = await _plantRepositories.GetByIdAsync(plantId);
        
        if (plant == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant not found"
                }
            };
        }
        
        var target = await _targetValueRepositories.GetTargetValueById(targetId);
        
        if (target == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Target value not found"
                }
            };
        }
        
        
        if(await _plantRepositories.PlantHasTargetValueType(plantId, target.Type, phaseId))
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = new MessageResultModel()
                {
                    Message = "Plant already has this target value type"
                }
            };
        }
        
        var plantPhase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(plantId, phaseId);
        
        if (plantPhase == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant does not have this phase"
                }
            };
        }
        
        var targetOfPhase = await _targetOfPhaseRepository.GetTargetOfPlantByPlantIdAndValueId(plantPhase.Id, targetId);

        if (targetOfPhase != null)
        {
            targetOfPhase.TargetValueId = targetId;
            try
            {
                await _targetOfPhaseRepository.Update(targetOfPhase);
                return new ResultModel<MessageResultModel>
                {
                    StatusCodes = (int)HttpStatusCode.OK,
                    Response = new MessageResultModel()
                    {
                        Message = "Set target value for plant successfully"
                    }
                };
            }
            catch (Exception e)
            {
                throw new CustomException(e.Message);
            }
        }
        
        var newTargetOfPhase = new TargetOfPhase
        {
            Id = Guid.NewGuid(),
            PlantOfPhaseId = plantPhase.Id,
            TargetValueId = targetId
        };
        
        try
        {
            await _targetOfPhaseRepository.Insert(newTargetOfPhase);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Set target value for plant successfully"
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> RemoveValueForPlant(Guid plantId, Guid targetId, Guid phaseId)
    {
        var plantPhase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(plantId, phaseId);
        if (plantPhase == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant does not have this phase"
                }
            };
        }
        
        var targetOfPhase = await _targetOfPhaseRepository.GetTargetOfPlantByPlantIdAndValueId(plantPhase.Id, targetId);
        
        if (targetOfPhase == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant does not have this target value"
                }
            };
        }
        
        try
        {
            await _targetOfPhaseRepository.Delete(targetOfPhase);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Remove target value for plant successfully"
            }
        };
        
    }

    public async Task<ResultModel<MessageResultModel>> UpdateValueForPlant(Guid plantId, Guid targetId, Guid newTargetId, Guid phaseId )
    {
        
        var plantPhase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(plantId, phaseId);
        if (plantPhase == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant does not have this phase"
                }
            };
        }
        
        var targetOfPhaseExist = await _targetOfPhaseRepository.GetTargetOfPlantByPlantIdAndValueId(plantPhase.Id ,targetId);
        
        if (targetOfPhaseExist == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Plant does not have this target value"
                }
            };
        }
        
        var target = await _targetValueRepositories.GetTargetValueById(targetId);
        if(target == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = new MessageResultModel()
                {
                    Message = "Target value not found"
                }
            };
        }
        
        if(target.Type != targetOfPhaseExist.TargetValue.Type)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = new MessageResultModel()
                {
                    Message = "Please choose target value type same as the old one"
                }
            };
        }
        
        targetOfPhaseExist.TargetValueId = newTargetId;
        
        try
        {
            await _targetOfPhaseRepository.Update(targetOfPhaseExist);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Set target value for plant successfully"
            }
        };
    }

    public async Task<ResultModel<List<PlantResModelWithTarget>>> GetPlantNotSetValueOfType(string type)
    {
        var plants = await _plantRepositories.GetPlantsWithoutTargetValueOfType(type);
        return new ResultModel<List<PlantResModelWithTarget>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = _mapper.Map<List<PlantResModelWithTarget>>(plants)
        };
    }

    public async Task<ResultModel<MessageResultModel>> SetPhaseForPlant(Guid plantId, Guid phaseId)
    {
        var plant = await _plantRepositories.GetByIdAsync(plantId);
        
        if (plant == null)
        {
            throw new CustomException("Plant not found");
        }
        var phase = await _phaseRepositories.GetGrowthPhaseById(phaseId);
        if (phase == null)
        {
            throw new CustomException("Phase not found");
        }
        if (await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(plantId, phaseId) != null)
        {
            throw new CustomException("Plant already has this phase");
        }
        var plantOfPhase = new PlantOfPhase
        {
            Id = Guid.NewGuid(),
            PlantId = plantId,
            PhaseId = phaseId
        };
        try
        {
            await _plantOfPhaseRepositories.Insert(plantOfPhase);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Set phase for plant successfully"
            }
        };
    }

    public async Task<ResultModel<MessageResultModel>> RemovePhaseForPlant(Guid plantId, Guid phaseId)
    {
        if (await _plantRepositories.GetByIdAsync(plantId) == null)
        {
            throw new CustomException("Plant not found");
        }
        
        var plantOfPhase = await _plantOfPhaseRepositories.GetPlantOfPhasesByPlantIdAndPhaseId(plantId, phaseId);
        
        if (plantOfPhase == null)
        {
            throw new CustomException("Plant does not have this phase");
        }
        
        if (await _deviceItemsRepositories.CheckDeviceItemByPlantIdAndPhaseId(plantId, phaseId))
        {
            throw new CustomException("Cannot remove phase for plant because there are device items using this phase");
        }
        try
        {
            await _targetOfPhaseRepository.DeleteRange(plantOfPhase.TargetOfPhases);
            await _plantOfPhaseRepositories.Delete(plantOfPhase);
        }
        catch (Exception e)
        {
            throw new CustomException(e.Message);
        }
        
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel()
            {
                Message = "Remove phase for plant successfully"
            }
        };
    }
}