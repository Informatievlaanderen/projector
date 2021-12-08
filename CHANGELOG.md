## [10.2.2](https://github.com/informatievlaanderen/projector/compare/v10.2.1...v10.2.2) (2021-12-08)


### Bug Fixes

* add varchar conversions ([bd30d0a](https://github.com/informatievlaanderen/projector/commit/bd30d0a724bee146644b536412f44f1f4c52057e))

## [10.2.1](https://github.com/informatievlaanderen/projector/compare/v10.2.0...v10.2.1) (2021-12-03)


### Bug Fixes

* restore RegisterConnectionString ([189c462](https://github.com/informatievlaanderen/projector/commit/189c462f2ba4fa531d2dfd83c4158695f7542a0b))

# [10.2.0](https://github.com/informatievlaanderen/projector/compare/v10.1.2...v10.2.0) (2021-12-03)


### Features

* provide endpoint for event history ([175c9d0](https://github.com/informatievlaanderen/projector/commit/175c9d03dd0fd8e004e02e9072231ef739cad33d))

## [10.1.2](https://github.com/informatievlaanderen/projector/compare/v10.1.1...v10.1.2) (2021-10-20)


### Bug Fixes

* bump projection handling ([b045f1e](https://github.com/informatievlaanderen/projector/commit/b045f1ecc2fe8f0dc408134e839b5df1e2d6f0a2))

## [10.1.1](https://github.com/informatievlaanderen/projector/compare/v10.1.0...v10.1.1) (2021-05-28)


### Bug Fixes

* move to 5.0.6 ([195c553](https://github.com/informatievlaanderen/projector/commit/195c553ceb1b4accee75b18aa15ce9a55690c11b))

# [10.1.0](https://github.com/informatievlaanderen/projector/compare/v10.0.0...v10.1.0) (2021-03-31)


### Features

* bump projection handling ([78decfd](https://github.com/informatievlaanderen/projector/commit/78decfdcbe76aec460a084c41d0cf70485fc61ec))

# [10.0.0](https://github.com/informatievlaanderen/projector/compare/v9.0.0...v10.0.0) (2021-03-10)


### Bug Fixes

* clean up deprecated out code ([b022e9a](https://github.com/informatievlaanderen/projector/commit/b022e9a8ce5e962a6ddae4426c4754e1813e5d98))
* get name and description from acutal projections GRAR-1876 ([cd41783](https://github.com/informatievlaanderen/projector/commit/cd417834471f91d9e20da65caffe722ae6345528))


### BREAKING CHANGES

* CHANGE
Removed deprecated code that was alreayd flagged with 'error on build'
* CHANGE
updated the RegisteredConnectedProjection to use ConnectedProjectionInfo
instead of strings

# [9.0.0](https://github.com/informatievlaanderen/projector/compare/v8.0.0...v9.0.0) (2021-03-10)


### Bug Fixes

* update projectionhandling dependency GRAR-1876 ([2f2b60c](https://github.com/informatievlaanderen/projector/commit/2f2b60cb5258948cdd4cd06f73112b230f7bd07e))


### Features

* add name and description to registered projections GRAR-1876 ([8bd4dbc](https://github.com/informatievlaanderen/projector/commit/8bd4dbc9435cf301c3a09caac37d77d7160e3c14))


### BREAKING CHANGES

* CHANGE
Extended the ProjectionRepsonse type and cleaned up property names

# [8.0.0](https://github.com/informatievlaanderen/projector/compare/v7.0.4...v8.0.0) (2021-03-10)


### Bug Fixes

* rename projection identifier from Name to Id GRAR-1876 ([618f8e2](https://github.com/informatievlaanderen/projector/commit/618f8e25b2941d53141d255ded55423662c801ce))


### BREAKING CHANGES

* CHANGE
Exception and Response have been modified

## [7.0.4](https://github.com/informatievlaanderen/projector/compare/v7.0.3...v7.0.4) (2021-02-02)


### Bug Fixes

* move to 5.0.2 ([d014d15](https://github.com/informatievlaanderen/projector/commit/d014d1504b9cdec5a846897c4c28455479e83993))

## [7.0.3](https://github.com/informatievlaanderen/projector/compare/v7.0.2...v7.0.3) (2020-12-19)


### Bug Fixes

* move to 5.0.1 ([a98fc91](https://github.com/informatievlaanderen/projector/commit/a98fc91a18f294152049c3960bcf6102eeb8c60d))

## [7.0.2](https://github.com/informatievlaanderen/projector/compare/v7.0.1...v7.0.2) (2020-11-19)


### Bug Fixes

* update eventhandling reference ([0219497](https://github.com/informatievlaanderen/projector/commit/02194976363145f484f81a4e4ef1cd43c7a09c48))

## [7.0.1](https://github.com/informatievlaanderen/projector/compare/v7.0.0...v7.0.1) (2020-11-18)


### Bug Fixes

* remove set-env usage in gh-actions ([8c82ae9](https://github.com/informatievlaanderen/projector/commit/8c82ae956a37438d9cc111874872327c80c2b827))

# [7.0.0](https://github.com/informatievlaanderen/projector/compare/v6.1.2...v7.0.0) (2020-11-02)


### Bug Fixes

* change noretry to default settings ([e9199c7](https://github.com/informatievlaanderen/projector/commit/e9199c79de515945e672ae8d43d0ba8b473bdec1))
* return unknow status for default mapping ([dcbc09f](https://github.com/informatievlaanderen/projector/commit/dcbc09f1f080b0dbe0bb8c50fbe4587957c46a1a))
* use catch up page size from settings ([4d96d5e](https://github.com/informatievlaanderen/projector/commit/4d96d5e200757ec55fef1444b58324e95a4a1d8b))


### Features

* introduce projection specific settings ([ead9df0](https://github.com/informatievlaanderen/projector/commit/ead9df08c3e31adf5e938c58cab31b8e37d7cc4b))


### BREAKING CHANGES

* CHANGES
introduce the ConnectedProjectionSettings
no longer except registrations without projection settings

## [6.1.2](https://github.com/informatievlaanderen/projector/compare/v6.1.1...v6.1.2) (2020-10-23)


### Bug Fixes

* remove ending slash of base url ([514d2e8](https://github.com/informatievlaanderen/projector/commit/514d2e8ed58fe0254905d2b6ed1c1121060be781))

## [6.1.1](https://github.com/informatievlaanderen/projector/compare/v6.1.0...v6.1.1) (2020-10-23)


### Bug Fixes

* projector module uses now IConfiguration instead of Root ([ca4ff22](https://github.com/informatievlaanderen/projector/commit/ca4ff22e81cb363889dd6b6416dc67123a33b980))

# [6.1.0](https://github.com/informatievlaanderen/projector/compare/v6.0.0...v6.1.0) (2020-10-22)


### Bug Fixes

* correct issues after rebase ([1a9cc2e](https://github.com/informatievlaanderen/projector/commit/1a9cc2e8ad5d49e62dc6830becd4be09f6c3dfb2))
* resolve some review remarks ([61cca96](https://github.com/informatievlaanderen/projector/commit/61cca9651bb97b39a0f19af3bcde4bbb2bbbe3db))
* set default position to -1 for projection state in api ([1a27f9d](https://github.com/informatievlaanderen/projector/commit/1a27f9d2f6b0c56981c4fcab6214b619a9b6ac72))


### Features

* add error message to api + refactor getting the state GRAR-1302 ([2a67c3b](https://github.com/informatievlaanderen/projector/commit/2a67c3b4f0d68713a1c40bf154a32bebd0444378))
* add HATEOAS links to projections GRAR-1304 ([40d86d8](https://github.com/informatievlaanderen/projector/commit/40d86d8d6782a5b7789acc57b6e16cd24e0a9966))
* add last projection position to projection api status GRAR-1300 ([62587f6](https://github.com/informatievlaanderen/projector/commit/62587f6c684e4f6bf668b81e313d9259982ca752))
* add projection state 'Crashed' to status api GRAR-1303 ([2f18e82](https://github.com/informatievlaanderen/projector/commit/2f18e8265371efa4ddde2bad8741923fbe881718))
* add stream position to projection status api GRAR-1301 ([c8ba6f7](https://github.com/informatievlaanderen/projector/commit/c8ba6f72317b1cf91352a3544e4b07bf79658e92))
* set and clear errormessage for the projection GRAR-1302 ([b5f086e](https://github.com/informatievlaanderen/projector/commit/b5f086efc87765e20da3131e68149eea57047c64))
* update packages projection handling ([38c5b12](https://github.com/informatievlaanderen/projector/commit/38c5b126a8e2da4f575540ce9c40d50681cbb04e))

# [6.0.0](https://github.com/informatievlaanderen/projector/compare/v5.4.5...v6.0.0) (2020-10-20)


### Bug Fixes

* add test for messagehandler executing gapstrategy GRAR-1355 ([050174e](https://github.com/informatievlaanderen/projector/commit/050174ea0f5541985230ef31d421d89e3591fee7))
* implement the correct gap strategies GRAR-1355 ([2d31403](https://github.com/informatievlaanderen/projector/commit/2d314032bee20e49b741b91f640ed22f263f6381))
* remove convenience Queue overload from interface GRAR-1355 ([5a44e08](https://github.com/informatievlaanderen/projector/commit/5a44e088dfe1c4371a505997dfdbf73661d5d6f9))


### Features

* add restart command GRAR-1355 ([9ab0435](https://github.com/informatievlaanderen/projector/commit/9ab04352f16a2dd5ac5e54ec5c190732b944c7e9))
* handle stream gap exceptions in catch up GRAR-1355 ([7f8cebc](https://github.com/informatievlaanderen/projector/commit/7f8cebc0efc61307105e2dc92edcffe0dc5a5ec8))
* handle stream gap exceptions in subscription GRAR-1355 ([5332468](https://github.com/informatievlaanderen/projector/commit/53324685fbb728e7bc5ca2762cbf26728ba6991c))


### BREAKING CHANGES

* CHANGES

## [5.4.5](https://github.com/informatievlaanderen/projector/compare/v5.4.4...v5.4.5) (2020-09-21)


### Bug Fixes

* move to 3.1.8 ([9d9983d](https://github.com/informatievlaanderen/projector/commit/9d9983d08d79b0ce980ecfa36666fed0a3be4610))

## [5.4.4](https://github.com/informatievlaanderen/projector/compare/v5.4.3...v5.4.4) (2020-07-18)


### Bug Fixes

* move to 3.1.6 ([96e14d3](https://github.com/informatievlaanderen/projector/commit/96e14d3f0237b31881ac9775950fb771eedafa95))

## [5.4.3](https://github.com/informatievlaanderen/projector/compare/v5.4.2...v5.4.3) (2020-07-03)


### Bug Fixes

* set correct policy name in obsolete message ([e33d98e](https://github.com/informatievlaanderen/projector/commit/e33d98eee252e6624d161a25dd9149a8c81f9c5d))

## [5.4.2](https://github.com/informatievlaanderen/projector/compare/v5.4.1...v5.4.2) (2020-07-02)


### Bug Fixes

* update streamstore ([2ba50a0](https://github.com/informatievlaanderen/projector/commit/2ba50a0099ee8d78f494ec2f2ae72704fc12a31f))

## [5.4.1](https://github.com/informatievlaanderen/projector/compare/v5.4.0...v5.4.1) (2020-06-19)


### Bug Fixes

* move to 3.1.5 ([e9e2f56](https://github.com/informatievlaanderen/projector/commit/e9e2f568a686d3f90bc4369789bc3d2faa6a7104))

# [5.4.0](https://github.com/informatievlaanderen/projector/compare/v5.3.0...v5.4.0) (2020-06-16)


### Bug Fixes

* check if delay is not negative GRAR-1284 ([043d757](https://github.com/informatievlaanderen/projector/commit/043d7576f6822e0d3acb5d1c8abe32fc5d8d9846))
* make configuration LinearBackoff specific GRAR-1284 ([dfd45a5](https://github.com/informatievlaanderen/projector/commit/dfd45a55e1b86951c95a1c04cf949b6393c1f0f6))


### Features

* add configuration for retry policy GRAR-1284 ([8770855](https://github.com/informatievlaanderen/projector/commit/8770855be484ce1c9fc074210d2639f3d16f994a))

# [5.3.0](https://github.com/informatievlaanderen/projector/compare/v5.2.4...v5.3.0) (2020-06-16)


### Bug Fixes

* add linear backoff policy GRAR-1257 ([2260c77](https://github.com/informatievlaanderen/projector/commit/2260c7723d75585ba6b7bdcb3b45a68fab52d17e))
* add retry infrastructure GRAR-1257 ([9d5caad](https://github.com/informatievlaanderen/projector/commit/9d5caad3df0445b87f4a8fefc47a59d54e302d67))
* expose projection name and logger in message handler GRAR-1257 ([4775dd6](https://github.com/informatievlaanderen/projector/commit/4775dd670515211bcdea3c0ddff1d73f7579de28))
* make handler interface internal again GRAR-1257 ([cee934e](https://github.com/informatievlaanderen/projector/commit/cee934e382974de3e0ead6f21515109a578f4bf1))
* simplify backoff wait time calculation GRAR-1257 ([1e8680e](https://github.com/informatievlaanderen/projector/commit/1e8680e26d2d06d610b23a7fbdd5f638fc4b3e6b))


### Features

* enable retry policies GRAR-1257 ([2c4a810](https://github.com/informatievlaanderen/projector/commit/2c4a8102a4a4b66b54deea8678a9a3fa8ae495fa))

## [5.2.4](https://github.com/informatievlaanderen/projector/compare/v5.2.3...v5.2.4) (2020-05-18)


### Bug Fixes

* move to 3.1.4 ([1abc3d2](https://github.com/informatievlaanderen/projector/commit/1abc3d28eb2dd28b470a2c49ff7b0bba9d583f56))

## [5.2.3](https://github.com/informatievlaanderen/projector/compare/v5.2.2...v5.2.3) (2020-05-14)


### Bug Fixes

* remove dotnet 3.1.4 package references ([29b86a2](https://github.com/informatievlaanderen/projector/commit/29b86a2))

## [5.2.2](https://github.com/informatievlaanderen/projector/compare/v5.2.1...v5.2.2) (2020-05-13)


### Bug Fixes

* move to GH-actions ([5d0a1dc](https://github.com/informatievlaanderen/projector/commit/5d0a1dc))

## [5.2.1](https://github.com/informatievlaanderen/projector/compare/v5.2.0...v5.2.1) (2020-03-03)


### Bug Fixes

* bump netcore to 3.1.2 ([1f0086c](https://github.com/informatievlaanderen/projector/commit/1f0086c))

# [5.2.0](https://github.com/informatievlaanderen/projector/compare/v5.1.0...v5.2.0) (2020-02-01)


### Features

* upgrade netcoreapp31 and dependencies ([fd45b14](https://github.com/informatievlaanderen/projector/commit/fd45b14))

# [5.1.0](https://github.com/informatievlaanderen/projector/compare/v5.0.0...v5.1.0) (2020-01-15)


### Features

* upgrade projection handling ([5780cfb](https://github.com/informatievlaanderen/projector/commit/5780cfb))

# [5.0.0](https://github.com/informatievlaanderen/projector/compare/v4.2.0...v5.0.0) (2019-12-17)


### Code Refactoring

* upgrade to netcoreapp31 ([f48b1ad](https://github.com/informatievlaanderen/projector/commit/f48b1ad))


### BREAKING CHANGES

* Upgrade to .NET Core 3.1

# [4.2.0](https://github.com/informatievlaanderen/projector/compare/v4.1.1...v4.2.0) (2019-09-25)


### Features

* add projection manager helpers ([4fac7ae](https://github.com/informatievlaanderen/projector/commit/4fac7ae))

## [4.1.1](https://github.com/informatievlaanderen/projector/compare/v4.1.0...v4.1.1) (2019-09-25)


### Bug Fixes

* allow reserved column names ([6ace0bc](https://github.com/informatievlaanderen/projector/commit/6ace0bc))

# [4.1.0](https://github.com/informatievlaanderen/projector/compare/v4.0.0...v4.1.0) (2019-09-25)


### Features

* add db query endpoint ([54fd213](https://github.com/informatievlaanderen/projector/commit/54fd213))

# [4.0.0](https://github.com/informatievlaanderen/projector/compare/v3.3.2...v4.0.0) (2019-09-24)


### Bug Fixes

* make setup for should resume explicit ([7b3fc27](https://github.com/informatievlaanderen/projector/commit/7b3fc27))
* register context so that it will not be a single instance ([fa647ac](https://github.com/informatievlaanderen/projector/commit/fa647ac))


### Features

* add ==/!= operator for ConnectedProjectionName ([43d8b1d](https://github.com/informatievlaanderen/projector/commit/43d8b1d))
* add UserDesiredState class ([ef35af2](https://github.com/informatievlaanderen/projector/commit/ef35af2))
* allow projections to be resumed ([abab01a](https://github.com/informatievlaanderen/projector/commit/abab01a))
* update user desired state when starting/stopping projections ([5604d33](https://github.com/informatievlaanderen/projector/commit/5604d33))


### BREAKING CHANGES

* ConnectedProjectionsManager.Start/Stop made async, needs cancellation token.
DefaultProjectorController made async.

## [3.3.2](https://github.com/informatievlaanderen/projector/compare/v3.3.1...v3.3.2) (2019-08-27)


### Bug Fixes

* make datadog tracing check more for nulls ([8e07d08](https://github.com/informatievlaanderen/projector/commit/8e07d08))

## [3.3.1](https://github.com/informatievlaanderen/projector/compare/v3.3.0...v3.3.1) (2019-08-26)


### Bug Fixes

* use fixed datadog tracing ([abd74a4](https://github.com/informatievlaanderen/projector/commit/abd74a4))

# [3.3.0](https://github.com/informatievlaanderen/projector/compare/v3.2.0...v3.3.0) (2019-08-22)


### Features

* bump to .net 2.2.6 ([5d61fb4](https://github.com/informatievlaanderen/projector/commit/5d61fb4))

# [3.2.0](https://github.com/informatievlaanderen/projector/compare/v3.1.3...v3.2.0) (2019-08-22)


### Features

* return 400 when projection name is incorrect fixes [#20](https://github.com/informatievlaanderen/projector/issues/20) ([fd907d3](https://github.com/informatievlaanderen/projector/commit/fd907d3))

## [3.1.3](https://github.com/informatievlaanderen/projector/compare/v3.1.2...v3.1.3) (2019-04-30)

## [3.1.2](https://github.com/informatievlaanderen/projector/compare/v3.1.1...v3.1.2) (2019-04-26)

## [3.1.1](https://github.com/informatievlaanderen/projector/compare/v3.1.0...v3.1.1) (2019-04-14)


### Bug Fixes

* don't use same instance of projectioncontext ([36a85f3](https://github.com/informatievlaanderen/projector/commit/36a85f3))
* lastProcessedPosition is set when picking message of the queue ([d5de81d](https://github.com/informatievlaanderen/projector/commit/d5de81d))
* log warning instead of throwning exception on gaps in messages ([aed5b9e](https://github.com/informatievlaanderen/projector/commit/aed5b9e))
* log warnings when projections are stopped due to exception ([5d18978](https://github.com/informatievlaanderen/projector/commit/5d18978))
* set project type to dotnet core guid ([b61b947](https://github.com/informatievlaanderen/projector/commit/b61b947))

# [3.1.0](https://github.com/informatievlaanderen/projector/compare/v3.0.1...v3.1.0) (2019-04-10)


### Features

* allow access to container when registering projections, fixes [#10](https://github.com/informatievlaanderen/projector/issues/10) ([78c0cd8](https://github.com/informatievlaanderen/projector/commit/78c0cd8))

## [3.0.1](https://github.com/informatievlaanderen/projector/compare/v3.0.0...v3.0.1) (2019-03-26)


### Bug Fixes

* use dotnetcore guid for test project ([a7dc108](https://github.com/informatievlaanderen/projector/commit/a7dc108))

# [3.0.0](https://github.com/informatievlaanderen/projector/compare/v2.0.0...v3.0.0) (2019-03-25)


### Bug Fixes

* catch exception on handle:ProcessEvent ([c623121](https://github.com/informatievlaanderen/projector/commit/c623121))
* extract migrationhelper from mananger ([d29ac6f](https://github.com/informatievlaanderen/projector/commit/d29ac6f))
* push all received messages on commande bus ([3ba2942](https://github.com/informatievlaanderen/projector/commit/3ba2942))
* remove commands from public interface ([1ac18d2](https://github.com/informatievlaanderen/projector/commit/1ac18d2))
* remove logger dependency in projectormodule ([86e1f89](https://github.com/informatievlaanderen/projector/commit/86e1f89))
* split streamstore subscription and subscription runner ([575ee3d](https://github.com/informatievlaanderen/projector/commit/575ee3d))
* splite mananger into manager, commandbus and commandhandler ([e4e2cab](https://github.com/informatievlaanderen/projector/commit/e4e2cab))
* use referenced test project properties ([88edf56](https://github.com/informatievlaanderen/projector/commit/88edf56))


### BREAKING CHANGES

* manager no longer receives commands
* removed the constructor with ILoggerFactory dependency

# [2.0.0](https://github.com/informatievlaanderen/projector/compare/v1.1.1...v2.0.0) (2019-03-20)


### Bug Fixes

* allow subscription when last processed is equal for stream and projection ([91571c5](https://github.com/informatievlaanderen/projector/commit/91571c5))
* avoid exceptions when stopping a catchup already being stopped ([dbd942e](https://github.com/informatievlaanderen/projector/commit/dbd942e))
* do not check if projection is running after creating a catchup ([bcd8955](https://github.com/informatievlaanderen/projector/commit/bcd8955))
* dont await _mailbox.SendAsync ([2e63587](https://github.com/informatievlaanderen/projector/commit/2e63587))
* loggin no longer throws an exception ([e56d12b](https://github.com/informatievlaanderen/projector/commit/e56d12b))
* remove Start.CatchUp and Start.Subscription ([aaa35dc](https://github.com/informatievlaanderen/projector/commit/aaa35dc))
* send new instance of subscribe to the queue ([8f74b03](https://github.com/informatievlaanderen/projector/commit/8f74b03))
* show command name in logging by default ([1ed7d71](https://github.com/informatievlaanderen/projector/commit/1ed7d71))
* update logging levels ([fc947fa](https://github.com/informatievlaanderen/projector/commit/fc947fa))
* use the supplied ILoggerFactory by configuration from host ([e10f616](https://github.com/informatievlaanderen/projector/commit/e10f616))


### Features

* remove own eventbus implementation ([f96326a](https://github.com/informatievlaanderen/projector/commit/f96326a))
* use a single command flow ([a0b24ec](https://github.com/informatievlaanderen/projector/commit/a0b24ec))


### BREAKING CHANGES

* changed the manager to use events to have 1 flow to track
changes

## [1.1.1](https://github.com/informatievlaanderen/projector/compare/v1.1.0...v1.1.1) (2019-03-19)

# [1.1.0](https://github.com/informatievlaanderen/projector/compare/v1.0.3...v1.1.0) (2019-03-08)


### Bug Fixes

* update projectionhandling runner dependency ([80895c3](https://github.com/informatievlaanderen/projector/commit/80895c3))


### Features

* add registration for migrator ([5c1fa68](https://github.com/informatievlaanderen/projector/commit/5c1fa68))

## [1.0.3](https://github.com/informatievlaanderen/projector/compare/v1.0.2...v1.0.3) (2019-03-07)


### Bug Fixes

* set correct dependency version in package ([b3941ee](https://github.com/informatievlaanderen/projector/commit/b3941ee))

## [1.0.2](https://github.com/informatievlaanderen/projector/compare/v1.0.1...v1.0.2) (2019-03-07)


### Bug Fixes

* update Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner dependency ([eebc3ec](https://github.com/informatievlaanderen/projector/commit/eebc3ec))

## [1.0.1](https://github.com/informatievlaanderen/projector/compare/v1.0.0...v1.0.1) (2019-02-26)


### Bug Fixes

* properly push to nuget ([179b67b](https://github.com/informatievlaanderen/projector/commit/179b67b))

# 1.0.0 (2019-02-26)


### Bug Fixes

* add nuget dependencies according to paket.references ([8c5e9cc](https://github.com/informatievlaanderen/projector/commit/8c5e9cc))
* backtrack stream before (re)starting subscriptions ([544342b](https://github.com/informatievlaanderen/projector/commit/544342b))
* clean up subscription/catchup flow ([feb2817](https://github.com/informatievlaanderen/projector/commit/feb2817))
* correct package.json with correct initial version ([98b03a8](https://github.com/informatievlaanderen/projector/commit/98b03a8))
* expand logging ([4b7bc5a](https://github.com/informatievlaanderen/projector/commit/4b7bc5a))
* get projection status directly from runners ([85466ae](https://github.com/informatievlaanderen/projector/commit/85466ae))
* register ConnectedProjectionManager using internal constructor ([e0d0ffb](https://github.com/informatievlaanderen/projector/commit/e0d0ffb))
* removed the callbacks in favour of messages ([d2d03cc](https://github.com/informatievlaanderen/projector/commit/d2d03cc))
* set entityframework dependency version ([c642c75](https://github.com/informatievlaanderen/projector/commit/c642c75))


### Features

* add default projector controller template ([8d74597](https://github.com/informatievlaanderen/projector/commit/8d74597))
* first version of projector ([1b60cf9](https://github.com/informatievlaanderen/projector/commit/1b60cf9))
* initial commit ([73be20c](https://github.com/informatievlaanderen/projector/commit/73be20c))
