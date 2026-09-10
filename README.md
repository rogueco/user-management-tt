# User Management

Tasks 1 to 4 were completed on the supplied MVC application; that version is in the git history (tag `tasks-1-4`).
Task 5 re-implements it as a Blazor WebAssembly client over a versioned REST API, on a layered solution under
`src/`, backed by PostgreSQL. Task 6 adds Redis caching, a RabbitMQ-driven worker for CSV imports, CI with a
coverage floor, and CD from a release branch.

## Running it

Requires Docker with Compose v2 and `make`.

```bash
make up        # builds the images and starts Postgres, Redis, RabbitMQ, the API and a worker
make down      # stops everything, keeps the data
make reset     # stops everything and deletes the data
```

Once `make up` returns the API is healthy, migrated and seeded.

| What | Where |
| --- | --- |
| Application | <http://localhost:8080> |
| API reference (Swagger UI) | <http://localhost:8080/usermanagementapi> |
| Health | <http://localhost:8080/health/ready> |
| RabbitMQ management | <http://localhost:15672> (guest / guest) |

Other targets: `make logs`, `make scale n=3` (more workers), `make psql`, `make redis`, `make test`, `make coverage`,
`make migration name=...`.

To run from source you need the .NET 10 SDK (`global.json` pins it). `dotnet run --project src/UserManagement.API`
runs against the compose Postgres; without Redis or RabbitMQ configured the cache is in-process and imports run
on an in-memory bus inside the API, which is also how the tests run.

## Layout

| Project | Purpose |
| --- | --- |
| `src/UserManagement.Domain` | `User`, `UserLog`, `ImportJob` and the change diff. No dependencies. |
| `src/UserManagement.Contracts` | DTOs shared by the API and the Blazor client, with the validation attributes the forms use. |
| `src/UserManagement.Services` | Application services returning `ServiceResult`, repository and unit-of-work interfaces, CSV parsing, the import processor, cache wiring. |
| `src/UserManagement.Repository.Sql` | EF Core on Npgsql, migrations, seeding. |
| `src/UserManagement.Messaging` | MassTransit with the transactional outbox, the import consumer. |
| `src/UserManagement.API` | Versioned controllers under `Controllers/V1`, Swagger, health checks; hosts the compiled Blazor app. |
| `src/UserManagement.Blazor` | Blazor WebAssembly with Material.Blazor. |
| `src/UserManagement.Worker` | Consumes import messages from RabbitMQ. |
| `tests/UserManagement.UnitTests` | Domain, services, controllers and extensions with Moq. |
| `tests/UserManagement.IntegrationTests` | Repository and API tests against real Postgres and Redis through Testcontainers. |

## CI and CD

- Pull requests to `main` (`test-pr.yml`): build, every test with coverage, then two gates. Total line coverage
  must stay above the floor in `.github/coverage-floor.txt` (currently 95%; nudge it up as coverage rises, or lower
  it in the PR and say why), and the executable lines a PR adds must be at least 80% covered. A sticky comment on
  the PR shows the total and the 20 least-covered classes. Both Docker images are built but not pushed.
- Pushes to `main` (`test-main.yml`): the tests again, and an issue labelled `ci-failure` if they fail.
- Pushes to `release` (`deploy.yml`): build and push the `api` and `worker` images to GHCR tagged with the short
  SHA and `release-latest`, then roll them out over SSH with `docker-compose.release.yml` to the host behind the
  `production` environment. The deploy job is skipped until the repository variable `DEPLOY_HOST` is set; it also
  needs the secrets `DEPLOY_USER` and `DEPLOY_SSH_KEY`, optionally `DEPLOY_PATH` and `SMOKE_URL`, and a host that
  is logged in to GHCR with a `.env` holding the real credentials.

Coverage leaves out the Blazor client, the worker host, EF migrations and DI wiring marked `ExcludeFromCodeCoverage`
(`tests/coverlet.runsettings`). `make coverage` produces the same numbers locally with an HTML report.

## Decisions worth knowing

- Services return a `ServiceResult` and one extension, `ServiceResultToActionResult`, maps it to a status code, so
  controllers stay thin and unhandled exceptions become ProblemDetails through the exception handler.
- Every create, update and delete is audited through the services; the log entry and the change commit in one
  transaction, and log entries have no foreign key so they outlive the user.
- A CSV import is accepted immediately, the job and its message commit together through the MassTransit outbox,
  and a worker processes rows through the same service the UI uses, so imported users are audited too. The
  consumer runs inside the inbox transaction, so a job is applied all or nothing and a crash mid-import is
  retried rather than leaving a half-imported file; the trade-off is that row progress is not visible until it finishes.
- Reads of users are cached with HybridCache, in-process plus Redis. HybridCache tag invalidation is per process,
  so writes made by the API are visible at once and writes made by the worker are visible within ten seconds.
  A message-based backplane would remove that window and was deliberately not built.

---

# User Management Technical Exercise

The exercise is an ASP.NET Core web application backed by Entity Framework Core, which faciliates management of some fictional users.
We recommend that you use [Visual Studio (Community Edition)](https://visualstudio.microsoft.com/downloads) or [Visual Studio Code](https://code.visualstudio.com/Download) to run and modify the application. 

**The application uses an in-memory database, so changes will not be persisted between executions.**

## The Exercise
Complete as many of the tasks below as you feel comfortable with. These are split into 4 levels of difficulty 
* **Standard** - Functionality that is common when working as a web developer
* **Advanced** - Slightly more technical tasks and problem solving
* **Expert** - Tasks with a higher level of problem solving and architecture needed
* **Platform** - Tasks with a focus on infrastructure and scaleability, rather than application development.

### 1. Filters Section (Standard)

The users page contains 3 buttons below the user listing - **Show All**, **Active Only** and **Non Active**. Show All has already been implemented. Implement the remaining buttons using the following logic:
* Active Only – This should show only users where their `IsActive` property is set to `true`
* Non Active – This should show only users where their `IsActive` property is set to `false`

### 2. User Model Properties (Standard)

Add a new property to the `User` class in the system called `DateOfBirth` which is to be used and displayed in relevant sections of the app.

### 3. Actions Section (Standard)

Create the code and UI flows for the following actions
* **Add** – A screen that allows you to create a new user and return to the list
* **View** - A screen that displays the information about a user
* **Edit** – A screen that allows you to edit a selected user from the list  
* **Delete** – A screen that allows you to delete a selected user from the list

Each of these screens should contain appropriate data validation, which is communicated to the end user.

### 4. Data Logging (Advanced)

Extend the system to capture log information regarding primary actions performed on each user in the app.
* In the **View** screen there should be a list of all actions that have been performed against that user. 
* There should be a new **Logs** page, containing a list of log entries across the application.
* In the Logs page, the user should be able to click into each entry to see more detail about it.
* In the Logs page, think about how you can provide a good user experience - even when there are many log entries.

### 5. Extend the Application (Expert)

Make a significant architectural change that improves the application.
Structurally, the user management application is very simple, and there are many ways it can be made more maintainable, scalable or testable.
Some ideas are:
* Re-implement the UI using a client side framework connecting to an API. Use of Blazor is preferred, but if you are more familiar with other frameworks, feel free to use them.
* Update the data access layer to support asynchronous operations.
* Implement authentication and login based on the users being stored.
* Implement bundling of static assets.
* Update the data access layer to use a real database, and implement database schema migrations.

### 6. Future-Proof the Application (Platform)

Add additional layers to the application that will ensure that it is scaleable with many users or developers. For example:
* Add CI pipelines to run tests and build the application.
* Add CD pipelines to deploy the application to cloud infrastructure.
* Add IaC to support easy deployment to new environments.
* Introduce a message bus and/or worker to handle long-running operations.

## Additional Notes

* Please feel free to change or refactor any code that has been supplied within the solution and think about clean maintainable code and architecture when extending the project.
* If any additional packages, tools or setup are required to run your completed version, please document these thoroughly.
