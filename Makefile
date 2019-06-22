PROJECT_NAME:=VGltf
PROJECT_VERSION:=0.1.1

PACKAGE_NAME:=net.yutopp.vgltf
PACKAGE_JSON_PATH:=Assets/${PACKAGE_NAME}/package.json

PROJECT_DIR:=StandaloneProject
PROJECT_TEST_DIR:=${PROJECT_DIR}/Tests

NUNIT_CONSOLE:=.nuget/NUnit.ConsoleRunner/tools/nunit3-console.exe

.PHONY: all
all: setup-net test

.PHONY: test
test: test-net35 test-net45 test-netcore20

.PHONY: setup
setup:
	git submodule update --init --recursive

.PHONY: setup-net
setup-net: setup
	nuget install NUnit.Console -ExcludeVersion -OutputDirectory .nuget

test-results:
	mkdir test-results

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

# .NET Standard 1.6, Core 1.0, Core 2.0
.PHONY: restore-dotnet
restore-dotnet:
	dotnet restore ${PROJECT_TEST_DIR}

.PHONY: build-debug-netcore20
build-debug-netcore20: restore-dotnet
	dotnet build ${PROJECT_TEST_DIR} -f netcoreapp2.0

.PHONY: test-netcore20
test-netcore20: build-debug-netcore20 test-results
	dotnet test ${PROJECT_TEST_DIR} -f netcoreapp2.0 -r test-results

.PHONY: coverage-netcore20
coverage-netcore20: build-debug-netcore20
	dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput='./lcov.info' ${PROJECT_TEST_DIR} -f netcoreapp2.0
	cp ${PROJECT_TEST_DIR}/lcov.info coverage/.

.PHONY: benchmark-netcore20
benchmark-netcore20:
	dotnet run -p ${PROJECT_DIR}/Benchmarks -c Release -f netcoreapp2.0 -- --job short --runtimes core

#
.PHONY: publish
publish:
	dotnet pack ${PROJECT_DIR}/${PROJECT_NAME} -c Release -p:PackageVersion=${PROJECT_VERSION}
	# export NuGetKey="~~"
	cd ${PROJECT_DIR}/${PROJECT_NAME}/bin/Release/ && \
		dotnet nuget push \
			-k $(NuGetKey) ${PROJECT_NAME}.${PROJECT_VERSION}.nupkg \
			-s https://api.nuget.org/v3/index.json
