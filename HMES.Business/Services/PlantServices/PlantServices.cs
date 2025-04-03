using System.Net;
using AutoMapper;
using HMES.Business.Utilities.Converter;
using HMES.Data.DTO.Custom;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.PlantRepositories;
using HMES.Data.Repositories.TargetOfPlantRepositories;
using HMES.Data.Repositories.TargetValueRepositories;

namespace HMES.Business.Services.PlantServices;

public class PlantServices : IPlantServices
{
    
    private readonly IPlantRepositories _plantRepositories;
    private readonly ITargetOfPlantRepository _targetOfPlantRepository;
    private readonly ITargetValueRepositories _targetValueRepositories;

    private readonly IMapper _mapper;
    
    public PlantServices(IPlantRepositories plantRepositories, ITargetValueRepositories targetValueRepositories, IMapper mapper, ITargetOfPlantRepository targetOfPlantRepository)
    {
        _plantRepositories = plantRepositories;
        _mapper = mapper;
        _targetOfPlantRepository = targetOfPlantRepository;
        _targetValueRepositories = targetValueRepositories;
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
        var plant = await _plantRepositories.GetByIdAsync(id);
        
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
            var targetsOfPlant = plant.TargetOfPlants.ToList();

            if (targetsOfPlant.Count > 0)
            {
                await _targetOfPlantRepository.DeleteRange(targetsOfPlant);
            }

            await _plantRepositories.Delete(plant);
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

    public async Task<ResultModel<MessageResultModel>> SetValueForPlant(Guid plantId, Guid targetId)
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
        
        
        if(await _plantRepositories.PlantHasTargetValueType(plantId, target.Type))
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
        
        var targetOfPlant = new TargetOfPlant
        {
            Id = Guid.NewGuid(),
            PlantId = plantId,
            TargetValueId = targetId
        };
        
        try
        {
            await _targetOfPlantRepository.Insert(targetOfPlant);
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

    public async Task<ResultModel<MessageResultModel>> RemoveValueForPlant(Guid plantId, Guid targetId)
    {
        var targetOfPlant = await _targetOfPlantRepository.GetTargetOfPlantByPlantIdAndValueId(plantId, targetId);
        
        if (targetOfPlant == null)
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
            await _targetOfPlantRepository.Delete(targetOfPlant);
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

    public async Task<ResultModel<MessageResultModel>> UpdateValueForPlant(Guid plantId, Guid targetId, Guid newTargetId)
    {
        var targetOfPlantExist = await _targetOfPlantRepository.GetTargetOfPlantByPlantIdAndValueId(plantId, targetId);
        
        if (targetOfPlantExist == null)
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
        
        if(target.Type != targetOfPlantExist.TargetValue.Type)
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
        
        targetOfPlantExist.TargetValueId = newTargetId;
        
        try
        {
            await _targetOfPlantRepository.Update(targetOfPlantExist);
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

    public async Task<ResultModel<List<PlantResModel>>> GetPlantNotSetValueOfType(string type)
    {
        var plants = await _plantRepositories.GetPlantsWithoutTargetValueOfType(type);
        return new ResultModel<List<PlantResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = _mapper.Map<List<PlantResModel>>(plants)
        };
    }
}