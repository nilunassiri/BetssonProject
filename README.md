# qa-backend-code-challenge

Code challenge for QA Backend Engineer candidates.

### Build Docker image

Run this command from the directory where there is the solution file.

```
docker build -f src/Betsson.OnlineWallets.Web/Dockerfile .
```

### Run Docker container

```
docker run -p <port>:8080 <image id>
```

### Open Swagger

```
http://localhost:<port>/swagger/index.html
```

### Run tests

First you need to provide the port you selected for docker on this path and save it:

`tests\Betsson.OnlineWallets.E2ETests\appsettings.json`

Then you can run the unit tests using this command

```
dotnet test tests/Betsson.OnlineWallets.UnitTests
```
And run the E2E api tests using this command

```
dotnet test tests/Betsson.OnlineWallets.E2ETests
```
