using System.Linq.Expressions;
using Application.Interfaces;
using Domain.Entities.Video;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Repositories;

public class GenericRepositoryTests
{
    private class TestSpecification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>>? Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new();
        public Expression<Func<T, object>>? OrderBy { get; set; }
        public Expression<Func<T, object>>? OrderByDescending { get; set; }
    }

    [Fact]
    public async Task GetByIdAsync_WorksForNormalAndSoftDeleted()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);

        var activeVideo = new VideoEntity { Id = 1, Title = "Active", IsDeleted = false };
        var deletedVideo = new VideoEntity { Id = 2, Title = "Deleted", IsDeleted = true };
        await context.Videos.AddRangeAsync(activeVideo, deletedVideo);
        await context.SaveChangesAsync();

        (await repo.GetByIdAsync(1, isSoft: false)).Should().NotBeNull();
        (await repo.GetByIdAsync(1, isSoft: true)).Should().BeNull();

        (await repo.GetByIdAsync(2, isSoft: false)).Should().BeNull();
        (await repo.GetByIdAsync(2, isSoft: true)).Should().NotBeNull();

        (await repo.GetByIdAsync(999, isSoft: false)).Should().BeNull();
    }

    [Fact]
    public async Task ListAllAsync_FiltersDeletedAppropriately()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);

        var v1 = new VideoEntity { Id = 1, Title = "V1", IsDeleted = false };
        var v2 = new VideoEntity { Id = 2, Title = "V2", IsDeleted = true };
        await context.Videos.AddRangeAsync(v1, v2);
        await context.SaveChangesAsync();

        var activeList = await repo.ListAllAsync(isSoft: false);
        activeList.Should().HaveCount(1);
        activeList.First().Id.Should().Be(1);

        var allList = await repo.ListAllAsync(isSoft: true);
        allList.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListAsync_WithSpecification_FiltersAndOrders()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);

        var v1 = new VideoEntity { Id = 1, Title = "Alpha", ViewCount = 10, IsDeleted = false };
        var v2 = new VideoEntity { Id = 2, Title = "Beta", ViewCount = 100, IsDeleted = false };
        var v3 = new VideoEntity { Id = 3, Title = "Gamma", ViewCount = 50, IsDeleted = false };
        await context.Videos.AddRangeAsync(v1, v2, v3);
        await context.SaveChangesAsync();

        await FluentActions.Invoking(() => repo.ListAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();

        var specDesc = new TestSpecification<VideoEntity>
        {
            OrderByDescending = v => v.ViewCount
        };
        var resDesc = await repo.ListAsync(specDesc);
        resDesc.Select(v => v.ViewCount).Should().ContainInOrder(100L, 50L, 10L);

        var specAsc = new TestSpecification<VideoEntity>
        {
            OrderBy = v => v.Title
        };
        var resAsc = await repo.ListAsync(specAsc);
        resAsc.Select(v => v.Title).Should().ContainInOrder("Alpha", "Beta", "Gamma");

        var specCrit = new TestSpecification<VideoEntity>
        {
            Criteria = v => v.ViewCount > 20
        };
        var resCrit = await repo.ListAsync(specCrit);
        resCrit.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddUpdateDeleteAndSave_WorksCorrectly()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);

        await FluentActions.Invoking(() => repo.AddAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();

        var video = new VideoEntity { Id = 10, Title = "Test Video", IsDeleted = false };
        await repo.AddAsync(video);
        await repo.SaveChangesAsync();

        (await context.Videos.FindAsync(10L)).Should().NotBeNull();

        await FluentActions.Invoking(() => repo.UpdateAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();

        video.Title = "Updated Title";
        await repo.UpdateAsync(video);
        (await context.Videos.FindAsync(10L))!.Title.Should().Be("Updated Title");

        await repo.DeleteAsync(999L);

        await repo.DeleteAsync(10L);
        var deletedEntity = await context.Videos.FindAsync(10L);
        deletedEntity!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void AsQurable_FiltersDeletedWhenSpecified()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);

        context.Videos.Add(new VideoEntity { Id = 1, Title = "Active", IsDeleted = false });
        context.Videos.Add(new VideoEntity { Id = 2, Title = "Deleted", IsDeleted = true });
        context.SaveChanges();

        repo.AsQurable(isSoft: false).Count().Should().Be(1);
        repo.AsQurable(isSoft: true).Count().Should().Be(2);
    }
}
