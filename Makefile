COMPOSE := docker compose
REPO    := src/UserManagement.Repository.Sql

.PHONY: up down reset logs worker-logs scale build test coverage migrate migration psql redis rabbit

up:            ## Build and start everything
	$(COMPOSE) up --build --detach --wait

down:          ## Stop everything, keep data
	$(COMPOSE) down

reset:         ## Stop everything and delete data
	$(COMPOSE) down --volumes

logs:          ## Follow application logs
	$(COMPOSE) logs --follow api worker

worker-logs:   ## Follow worker logs only
	$(COMPOSE) logs --follow worker

scale:         ## Run several workers: make scale n=3
	$(COMPOSE) up --detach --scale worker=$(n) worker

build:         ## Build the solution
	dotnet build UserManagement.sln

test:          ## Run every test project (integration tests need Docker)
	dotnet test UserManagement.sln

coverage:      ## Run the tests with coverage and write an HTML report to TestResults/coverage
	dotnet tool restore
	rm -rf TestResults
	dotnet test UserManagement.sln --collect:"XPlat Code Coverage" --settings tests/coverlet.runsettings --results-directory TestResults
	dotnet reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/coverage -reporttypes:"Html;TextSummary" -verbosity:Warning
	@grep "Line coverage" TestResults/coverage/Summary.txt
	@echo "Report: TestResults/coverage/index.html"

migrate:       ## Apply pending migrations to the database in compose
	dotnet tool restore
	dotnet ef database update --project $(REPO) --startup-project $(REPO)

migration:     ## Add a migration: make migration name=AddSomething
	dotnet tool restore
	dotnet ef migrations add $(name) --project $(REPO) --startup-project $(REPO) --output-dir Migrations

psql:          ## Open a psql shell on the compose database
	$(COMPOSE) exec postgres psql -U $${POSTGRES_USER:-usermanagement} -d $${POSTGRES_DB:-usermanagement}

redis:         ## Open a redis-cli shell on the compose cache
	$(COMPOSE) exec redis redis-cli

rabbit:        ## Print the RabbitMQ management URL
	@echo "http://localhost:15672 (user: $${RABBITMQ_USER:-guest}, password: $${RABBITMQ_PASSWORD:-guest})"
