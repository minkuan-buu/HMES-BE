using System.Net;
using AutoMapper;
using HMES.Data.DTO.RequestModel;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using HMES.Data.Repositories.TargetValueRepositories;

namespace HMES.Business.Services.TargetValueServices;

public class TargetValueServices : ITargetValueServices
{
    
    private readonly ITargetValueRepositories _targetValueRepositories;
    private readonly IMapper _mapper;
    
    public TargetValueServices(ITargetValueRepositories targetValueRepositories, IMapper mapper)
    {
        _targetValueRepositories = targetValueRepositories;
        _mapper = mapper;
    }
    
    
    public async Task<ResultModel<ListDataResultModel<TargetResModel>>> GetAllTargetValuesAsync(string? type, decimal? minValue, decimal? maxValue, int pageIndex, int pageSize)
    {
        var (targetValues, totalItems) = await _targetValueRepositories.GetAllValuesAsync(type, minValue, maxValue, pageIndex, pageSize);
        
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        
        var result = new ListDataResultModel<TargetResModel>
        {
            Data = _mapper.Map<List<TargetResModel>>(targetValues),
            CurrentPage = pageIndex,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };
        return new ResultModel<ListDataResultModel<TargetResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
        
    }

    public async Task<ResultModel<DataResultModel<TargetResModelWithPlants>>> GetByIdAsync(Guid id)
    {
        var targetValue = await _targetValueRepositories.GetTargetValueById(id);
        
        if (targetValue == null)
        {
            return new ResultModel<DataResultModel<TargetResModelWithPlants>>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = null
            };
        }
        var targetValueDto = _mapper.Map<TargetResModelWithPlants>(targetValue);
        
        var result = new DataResultModel<TargetResModelWithPlants>
        {
            Data = targetValueDto
        };
        
        return new ResultModel<DataResultModel<TargetResModelWithPlants>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
        
    }

    public async Task<ResultModel<DataResultModel<TargetResModel>>> CreateTargetValueAsync(TargetReqModel targetReqModel)
    {
        
        if(targetReqModel.MinValue > targetReqModel.MaxValue)
        {
            return new ResultModel<DataResultModel<TargetResModel>>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = null
            };
        }
        
        var targetValueExist = await _targetValueRepositories.GetTargetValueByTypeAndMinAndMax(targetReqModel.Type.ToString(), targetReqModel.MinValue, targetReqModel.MaxValue);
        if(targetValueExist != null)
        {
            return new ResultModel<DataResultModel<TargetResModel>>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = null
            };
        }
        
        
        var targetValue = _mapper.Map<TargetValue>(targetReqModel);
        
        await _targetValueRepositories.Insert(targetValue);
        
        var result = new DataResultModel<TargetResModel>
        {
            Data = _mapper.Map<TargetResModel>(targetValue)
        };
        
        return new ResultModel<DataResultModel<TargetResModel>>
        {
            StatusCodes = (int)HttpStatusCode.Created,
            Response = result
        };
        
    }

    public async Task<ResultModel<DataResultModel<TargetResModel>>> UpdateTargetValueAsync(Guid id, TargetReqModel targetReqModel)
    {
        var targetValue = await _targetValueRepositories.GetTargetValueById(id);
        
        if (targetValue == null || IsTargetValueExist(targetValue, targetReqModel))
        {
            return new ResultModel<DataResultModel<TargetResModel>>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = null
            };
        }

        var tempId = targetValue.Id;
        _mapper.Map(targetReqModel, targetValue);
        targetValue.Id = tempId;

        try
        {
            await _targetValueRepositories.Update(targetValue);
        }catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new ResultModel<DataResultModel<TargetResModel>>
            {
                StatusCodes = (int)HttpStatusCode.BadRequest,
                Response = null
            };
        }
        
        
        var result = new DataResultModel<TargetResModel>
        {
            Data = _mapper.Map<TargetResModel>(targetValue)
        };
        
        return new ResultModel<DataResultModel<TargetResModel>>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = result
        };
    }

    public async Task<ResultModel<MessageResultModel>> DeleteTargetValueAsync(Guid id)
    {
        var targetValue = await _targetValueRepositories.GetTargetValueById(id);
        
        if (targetValue == null)
        {
            return new ResultModel<MessageResultModel>
            {
                StatusCodes = (int)HttpStatusCode.NotFound,
                Response = null
            };
        }
        
        await _targetValueRepositories.Delete(targetValue);
        
        return new ResultModel<MessageResultModel>
        {
            StatusCodes = (int)HttpStatusCode.OK,
            Response = new MessageResultModel
            {
                Message = "Target value deleted successfully"
            }
        };
    }
    
    private bool IsTargetValueExist(TargetValue targetValue, TargetReqModel targetReqModel)
    {
        return targetReqModel.MinValue != targetReqModel.MaxValue
               || targetValue.MinValue != targetValue.MaxValue
               || !targetValue.Type.Equals(targetReqModel.Type.ToString());
    }
    
}