using Grpc.Core;
using MangaWorkflow.APIGrpcService.HuyNQ.Protos;
using MangaWorkflow.Services.HuyNQ;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MangaWorkflow.APIGrpcService.HuyNQ.Services;

public class ChapterHuyNqGRPCService(IChapterHuyNqService chapterService) : ChapterHuyNqGRPC.ChapterHuyNqGRPCBase
{
    private readonly IChapterHuyNqService _chapterService = chapterService;

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
            Console.WriteLine(ex.Message);
        }

        return results;
    }

    public override Task<ChapterHuyNq> GetByIdAsync(ChapterIdRequest request, ServerCallContext context)
    {
        return base.GetByIdAsync(request, context);
    }

    public override Task<MutationResult> CreateAsync(ChapterHuyNq request, ServerCallContext context)
    {
        return base.CreateAsync(request, context);
    }

    public override Task<MutationResult> UpdateAsync(ChapterHuyNq request, ServerCallContext context)
    {
        return base.UpdateAsync(request, context);
    }

    public override Task<MutationResult> DeleteAsync(ChapterIdRequest request, ServerCallContext context)
    {
        return base.DeleteAsync(request, context);
    }
}
