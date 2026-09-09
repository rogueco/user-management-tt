using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List(new());

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }


    [Fact]
    public void List_WhenFilterIsInvalid_MustRedirectToUnfilteredList()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError(nameof(UserFilter.IsActive), "Invalid");

        // Act
        var result = controller.List(new());

        // Assert
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));

        _userService.Verify(s => s.Filter(It.IsAny<UserFilter>()), Times.Never());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void List_WhenFilterProvided_MustPassFilterToService(bool isActive)
    {
        // Arrange
        var controller = CreateController();
        var users = SetupUsers(isActive: isActive);

        // Act
        var result = controller.List(new UserFilter { IsActive = isActive });

        // Assert
        result
            .Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);

        _userService.Verify(s => s.Filter(It.Is<UserFilter>(f => f.IsActive == isActive)), Times.Once());
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        };

        _userService
            .Setup(s => s.Filter(It.IsAny<UserFilter>()))
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}
