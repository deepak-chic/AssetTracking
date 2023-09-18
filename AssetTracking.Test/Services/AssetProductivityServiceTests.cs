using System;
using System.Threading.Tasks;
using AssetTrackingService.AutoMapper;
using AssetTrackingService.DataTransferObject;
using AssetTrackingService.Entities;
using AssetTrackingService.Services;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using FluentAssertions;
using IoTPlatformLibrary;
using IoTPlatformLibrary.DataBase.Abstract;
using Moq;
using Xunit;

namespace AssetTracking.Test.Services
{
    public class AssetProductivityServiceTests
    {
        private readonly Mock<IRepository<AssetProductivity, Guid>> _repo;
        private readonly IMapper _mapper;
        private readonly AssetProductivityService _assetProductivityService;

        public AssetProductivityServiceTests()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.ConfigureDataProfiles();
                cfg.AddExpressionMapping();
            });
            mapperConfiguration.CompileMappings();
            mapperConfiguration.AssertConfigurationIsValid();
            _mapper = new Mapper(mapperConfiguration);
            _repo = new Mock<IRepository<AssetProductivity, Guid>>();
            _assetProductivityService = new AssetProductivityService(_repo.Object, _mapper);
        }

        [Fact]
        public async Task AddProductivityAsync_ValidValues_Success()
        {
            // Arrange
            AssetProdInsertDto insertDto = new AssetProdInsertDto() { DeviceId = "Device01" };
            _repo.Setup(x => x.AddAsync(It.IsAny<AssetProductivity>(), It.IsAny<ServiceContext>())).ReturnsAsync(new AssetProductivity() { id = Guid.NewGuid() });

            // Act
            var guid = await _assetProductivityService.AddProductivityAsync(insertDto);

            // Assert
            guid.Should().NotBeEmpty();
        }
    }
}