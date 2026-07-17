using Grpc.Core;
using MangaWorkflow.APIGrpcService.HuyNQ.Protos;
using MangaWorkflow.Services.HuyNQ;
using MangaWorkflow.Services.HuyNQ.DTOs.Chapter;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MangaWorkflow.APIGrpcService.HuyNQ.Services;

public class ChapterHuyNqGRPCService(IChapterHuyNqService chapterService) : ChapterHuyNqGRPC.ChapterHuyNqGRPCBase
{
    private readonly IChapterHuyNqService _chapterService = chapterService;

    [Authorize]
    public override async Task<ChapterHuyNqList> GetAllAsync(EmptyRequest request, ServerCallContext context)
    {
        var results = new ChapterHuyNqList();

        try
        {
            var items = await _chapterService.GetAllAsync();

            var opt = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var itemsJsonString = JsonSerializer.Serialize(items, opt);

            var result = JsonSerializer.Deserialize<List<ChapterHuyNq>>(itemsJsonString);

            results.Items.AddRange(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return results;
    }

    [Authorize(Roles = "1, 2")]
    public override async Task<ChapterHuyNq> GetByIdAsync(ChapterIdRequest request, ServerCallContext context)
    {
        var result = new ChapterHuyNq();
        try
        {
            var item = await _chapterService.GetByIdAsync(request.HuynqId);

            var opt = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var itemJsonString = JsonSerializer.Serialize(item, opt);

            result = JsonSerializer.Deserialize<ChapterHuyNq>(itemJsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return result ?? new();
    }

    [Authorize]
    public override async Task<ChapterHuyNqList> SearchAsync(Protos.ChapterSearchRequest request, ServerCallContext context)
    {
        try
        {
            var items = await _chapterService.SearchAsync(new MangaWorkflow.Services.HuyNQ.DTOs.Chapter.ChapterSearchRequest(
                request.HasTitle ? request.Title : null,
                request.HasChapterNumber ? request.ChapterNumber : null,
                request.HasApproved ? request.Approved : null));

            var opt = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var itemsJsonString = JsonSerializer.Serialize(items, opt);

            var itemsDeserialize = JsonSerializer.Deserialize<List<ChapterHuyNq>>(itemsJsonString);

            var result = new ChapterHuyNqList();

            if (itemsDeserialize != null)
                result.Items.AddRange(itemsDeserialize);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return new ();
    }

    [Authorize(Roles = "1")]
    public override async Task<MutationResult> CreateAsync(ChapterHuyNq request, ServerCallContext context)
    {
        MutationResult mutationResult = new();

        try
        {
            var opt = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var itesmJsonString = JsonSerializer.Serialize(request, opt);

            var item = JsonSerializer.Deserialize<ChapterCreateRequest>(itesmJsonString, opt) ?? throw new InvalidDataException("Body invalid");

            ValidateOrThrow(item);

            var result = await _chapterService.CreateAsync(item);

            mutationResult.Result = result;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return mutationResult;
    }

    [Authorize(Roles = "1")]
    public override async Task<MutationResult> UpdateAsync(ChapterHuyNq request, ServerCallContext context)
    {
        MutationResult mutationResult = new();

        try
        {
            var opt = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var itesmJsonString = JsonSerializer.Serialize(request, opt);

            var item = JsonSerializer.Deserialize<ChapterUpdateRequest>(itesmJsonString, opt) ?? throw new InvalidDataException("Body invalid");

            ValidateOrThrow(item);

            var result = await _chapterService.UpdateAsync(request.HuynqId, item);

            mutationResult.Result = result;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return mutationResult;
    }

    [Authorize(Roles = "1")]
    public override async Task<MutationResult> DeleteAsync(ChapterIdRequest request, ServerCallContext context)
    {
        MutationResult mutationResult = new();

        try
        {
            var result = await _chapterService.DeleteAsync(request.HuynqId);

            mutationResult.Result = result == true ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return mutationResult;
    }

    // Mirrors the DataAnnotations validation ASP.NET performs automatically for
    // [ApiController] endpoints. gRPC has no built-in model validation, so we run
    // it by hand and surface failures as an InvalidArgument status to the client.
    private static void ValidateOrThrow(object dto)
    {
        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(dto, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
        }
    }
}
