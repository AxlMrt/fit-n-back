using FitnessApp.Modules.Exercises.Application.DTOs.Requests;
using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace FitnessApp.Modules.Exercises.Tests.Application.Services
{
    public class TagServiceTests
    {
        private readonly Mock<ITagRepository> _mockTagRepository;
        private readonly Mock<IExerciseMapper> _mockExerciseMapper;
        private readonly TagService _tagService;

        public TagServiceTests()
        {
            _mockTagRepository = new Mock<ITagRepository>();
            _mockExerciseMapper = new Mock<IExerciseMapper>();
            _tagService = new TagService(_mockTagRepository.Object, _mockExerciseMapper.Object);
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenTagExists_ShouldReturnMappedTag()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var tag = new Tag("Test Tag");
            var expectedResponse = new TagResponse { Id = tagId, Name = "Test Tag" };

            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync(tag);
            _mockExerciseMapper.Setup(mapper => mapper.MapToTag(tag)).Returns(expectedResponse);

            // Act
            var result = await _tagService.GetByIdAsync(tagId);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedResponse);
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockExerciseMapper.Verify(mapper => mapper.MapToTag(tag), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTagDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync((Tag)null);

            // Act
            var result = await _tagService.GetByIdAsync(tagId);

            // Assert
            result.Should().BeNull();
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockExerciseMapper.Verify(mapper => mapper.MapToTag(It.IsAny<Tag>()), Times.Never);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenTagsExist_ShouldReturnAllMappedTags()
        {
            // Arrange
            var tags = new List<Tag>
            {
                new Tag("Tag 1"),
                new Tag("Tag 2"),
                new Tag("Tag 3")
            };

            var expectedResponses = new List<TagResponse>
            {
                new TagResponse { Id = Guid.NewGuid(), Name = "Tag 1" },
                new TagResponse { Id = Guid.NewGuid(), Name = "Tag 2" },
                new TagResponse { Id = Guid.NewGuid(), Name = "Tag 3" }
            };

            _mockTagRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(tags);

            for (int i = 0; i < tags.Count; i++)
            {
                _mockExerciseMapper.Setup(mapper => mapper.MapToTag(tags[i])).Returns(expectedResponses[i]);
            }

            // Act
            var result = await _tagService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedResponses);
            _mockTagRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTagsExist_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockTagRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Tag>());

            // Act
            var result = await _tagService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockTagRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        #endregion

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WhenMatchingTagsExist_ShouldReturnMatchingTags()
        {
            // Arrange
            var searchTerm = "workout";
            var matchingTags = new List<Tag>
            {
                new Tag("Workout Upper"),
                new Tag("Workout Lower")
            };

            var expectedResponses = new List<TagResponse>
            {
                new TagResponse { Id = Guid.NewGuid(), Name = "Workout Upper" },
                new TagResponse { Id = Guid.NewGuid(), Name = "Workout Lower" }
            };

            _mockTagRepository.Setup(repo => repo.SearchAsync(searchTerm)).ReturnsAsync(matchingTags);

            for (int i = 0; i < matchingTags.Count; i++)
            {
                _mockExerciseMapper.Setup(mapper => mapper.MapToTag(matchingTags[i])).Returns(expectedResponses[i]);
            }

            // Act
            var result = await _tagService.SearchAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);
            _mockTagRepository.Verify(repo => repo.SearchAsync(searchTerm), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_WhenNoMatchingTagsExist_ShouldReturnEmptyCollection()
        {
            // Arrange
            var searchTerm = "nonexistent";
            _mockTagRepository.Setup(repo => repo.SearchAsync(searchTerm)).ReturnsAsync(new List<Tag>());

            // Act
            var result = await _tagService.SearchAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockTagRepository.Verify(repo => repo.SearchAsync(searchTerm), Times.Once);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WhenValidRequest_ShouldCreateTagAndReturnId()
        {
            // Arrange
            var request = new CreateTagRequest { Name = "New Tag", Description = "Description" };

            _mockTagRepository.Setup(repo => repo.GetByNameAsync(request.Name)).ReturnsAsync((Tag)null);
            _mockTagRepository.Setup(repo => repo.AddAsync(It.IsAny<Tag>())).Returns(Task.CompletedTask);

            // Act
            var result = await _tagService.CreateAsync(request);

            // Assert
            result.Should().NotBeEmpty();
            _mockTagRepository.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
            _mockTagRepository.Verify(repo => repo.AddAsync(It.Is<Tag>(t =>
                t.Name == request.Name &&
                t.Description == request.Description)),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            CreateTagRequest request = null;

            // Act
            Func<Task> act = async () => await _tagService.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("request");
            _mockTagRepository.Verify(repo => repo.GetByNameAsync(It.IsAny<string>()), Times.Never);
            _mockTagRepository.Verify(repo => repo.AddAsync(It.IsAny<Tag>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenTagWithSameNameExists_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new CreateTagRequest { Name = "Existing Tag", Description = "Description" };
            var existingTag = new Tag("Existing Tag");

            _mockTagRepository.Setup(repo => repo.GetByNameAsync(request.Name)).ReturnsAsync(existingTag);

            // Act
            Func<Task> act = async () => await _tagService.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"A tag with the name '{request.Name}' already exists.");
            _mockTagRepository.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
            _mockTagRepository.Verify(repo => repo.AddAsync(It.IsAny<Tag>()), Times.Never);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenValidRequestAndId_ShouldUpdateTag()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var request = new UpdateTagRequest { Name = "Updated Tag", Description = "Updated Description" };
            var existingTag = new Tag("Original Tag", "Original Description");

            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync(existingTag);
            _mockTagRepository.Setup(repo => repo.GetByNameAsync(request.Name)).ReturnsAsync((Tag)null);
            _mockTagRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Tag>())).Returns(Task.CompletedTask);

            // Act
            await _tagService.UpdateAsync(tagId, request);

            // Assert
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockTagRepository.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
            _mockTagRepository.Verify(repo => repo.UpdateAsync(It.Is<Tag>(t =>
                t.Name == request.Name &&
                t.Description == request.Description)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTagDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var request = new UpdateTagRequest { Name = "Updated Tag", Description = "Updated Description" };

            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync((Tag)null);

            // Act
            Func<Task> act = async () => await _tagService.UpdateAsync(tagId, request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Tag with ID {tagId} not found.");
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockTagRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Tag>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            UpdateTagRequest request = null;

            // Act
            Func<Task> act = async () => await _tagService.UpdateAsync(tagId, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("request");
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Never);
            _mockTagRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Tag>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenNameAlreadyExistsOnDifferentTag_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var differentTagId = Guid.NewGuid();
            var request = new UpdateTagRequest { Name = "Existing Tag", Description = "Updated Description" };

            var existingTag = new Tag("Original Tag", "Original Description");
            
            var conflictingTag = new Tag("Existing Tag");
            
            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync(existingTag);
            
            _mockTagRepository.Setup(repo => repo.GetByNameAsync(request.Name))
                .ReturnsAsync(conflictingTag);

            // Act
            Func<Task> act = async () => await _tagService.UpdateAsync(tagId, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"A tag with the name '{request.Name}' already exists.");
            
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockTagRepository.Verify(repo => repo.GetByNameAsync(request.Name), Times.Once);
            _mockTagRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Tag>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenUpdatingToSameName_ShouldNotCheckForNameConflict()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var originalName = "Original Tag";
            var request = new UpdateTagRequest { Name = originalName, Description = "Updated Description" };

            var existingTag = new Tag(originalName, "Original Description");

            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync(existingTag);
            _mockTagRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Tag>())).Returns(Task.CompletedTask);

            // Act
            await _tagService.UpdateAsync(tagId, request);

            // Assert
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockTagRepository.Verify(repo => repo.GetByNameAsync(It.IsAny<string>()), Times.Never);
            _mockTagRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Tag>()), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenTagExists_ShouldDeleteTag()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var existingTag = new Tag("Tag to delete");

            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync(existingTag);
            _mockTagRepository.Setup(repo => repo.DeleteAsync(tagId)).Returns(Task.CompletedTask);

            // Act
            await _tagService.DeleteAsync(tagId);

            // Assert
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockTagRepository.Verify(repo => repo.DeleteAsync(tagId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenTagDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var tagId = Guid.NewGuid();

            _mockTagRepository.Setup(repo => repo.GetByIdAsync(tagId)).ReturnsAsync((Tag)null);

            // Act
            Func<Task> act = async () => await _tagService.DeleteAsync(tagId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Tag with ID {tagId} not found.");
            _mockTagRepository.Verify(repo => repo.GetByIdAsync(tagId), Times.Once);
            _mockTagRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion
    }
}
       