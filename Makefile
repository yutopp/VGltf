PROJECT_NAME:=VGltf

PACKAGE_NAME:=net.yutopp.vgltf
PACKAGE_DIR:=Packages/${PACKAGE_NAME}
PACKAGE_JSON_PATH:=${PACKAGE_DIR}/package.json

PROJECT_DIR:=standalone-project
PROJECT_TEST_DIR:=${PROJECT_DIR}/Tests

DOTNET_FRAMEWORK=net5.0

.PHONY: test
test: test-dotnet

.PHONY: setup
setup:
	git submodule update --init --recursive

# .NET Framework 3.5
.PHONY: restore-net35
restore-net35:
	msbuild ${PROJECT_TEST_DIR} /t:restore /p:TargetFramework=net35

.PHONY: build-debug-net35
build-debug-net35: restore-net35
	msbuild ${PROJECT_TEST_DIR} /p:TargetFramework=net35

.PHONY: test-net35
test-net35: build-debug-net35 test-results
	mono ${NUNIT_CONSOLE} ${PROJECT_TEST_DIR}/bin/Debug/net35/Tests.dll --result=test-results/results.xml;transform=nunit-transforms/nunit3-junit.xslt

# .NET Framework 4.5
.PHONY: restore-net45
restore-net45:
	msbuild ${PROJECT_TEST_DIR} /t:restore /p:TargetFramework=net45

.PHONY: build-debug-net45
build-debug-net45: restore-net45
	msbuild ${PROJECT_TEST_DIR} /p:TargetFramework=net45

.PHONY: test-net45
test-net45: build-debug-net45 test-results
	mono ${NUNIT_CONSOLE} ${PROJECT_TEST_DIR}/bin/Debug/net45/Tests.dll --result=test-results/results.xml;transform=nunit-transforms/nunit3-junit.xslt

# .NET Standard 2.x, .NET5.0
.PHONY: restore-dotnet
restore-dotnet:
	dotnet restore ${PROJECT_TEST_DIR}

.PHONY: test-dotnet
test-dotnet: restore-dotnet
	mkdir -p test-results/dotnet
	dotnet test ${PROJECT_TEST_DIR} -f ${DOTNET_FRAMEWORK} -r test-results/dotnet/results.xml

.PHONY: coverage-dotnet
coverage-dotnet: restore-dotnet
	mkdir -p coverage
	dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov -f ${DOTNET_FRAMEWORK} ${PROJECT_TEST_DIR}
	cp ${PROJECT_TEST_DIR}/coverage.${DOTNET_FRAMEWORK}.info coverage/lcov.info

.PHONY: benchmark-dotnet
benchmark-dotnet:
	dotnet run -p ${PROJECT_DIR}/Benchmarks -c Release -f ${DOTNET_FRAMEWORK} -- --job short --runtimes core

#
.PHONY: publish-to-nuget
publish-to-nuget:
	dotnet pack ${PROJECT_DIR}/${PROJECT_NAME} -c Release -p:PackageVersion=$(PROJECT_VERSION)
	dotnet nuget push $(PROJECT_DIR)/$(PROJECT_NAME)/bin/Release/*.nupkg \
		--api-key $(NUGET_KEY) \
		--source https://api.nuget.org/v3/index.json
