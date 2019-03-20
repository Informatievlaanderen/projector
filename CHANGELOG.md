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
