# Thirdweb Makefile
# Cross-platform targets to mirror tw.bat functionality
# Requires: GNU Make, dotnet SDK, optional CSharpier

# Use bash for consistent behavior across platforms (Git Bash/MSYS2/WSL/macOS/Linux)
SHELL := bash
.SHELLFLAGS := -o pipefail -c

# Default target
.DEFAULT_GOAL := help

# Tools and paths
DOTNET := dotnet
API_CLIENT := Thirdweb/Thirdweb.Api/ThirdwebApi.cs
CONSOLE_PROJ := Thirdweb.Console
GENERATOR_PROJ := Thirdweb.Generator
LIB_PROJ := Thirdweb/Thirdweb.csproj

# Defaults for publishing/building
CONFIG ?= Release
TFM ?= netstandard2.1
RID ?=
OUT ?=

# Colors (best effort; will be empty if tput is unavailable)
C_RST  := $(shell tput sgr0 2>/dev/null || echo "")
C_BOLD := $(shell tput bold 2>/dev/null || echo "")
C_DIM  := $(shell tput dim 2>/dev/null || echo "")
C_RED  := $(shell tput setaf 1 2>/dev/null || echo "")
C_GRN  := $(shell tput setaf 2 2>/dev/null || echo "")
C_YEL  := $(shell tput setaf 3 2>/dev/null || echo "")
C_BLU  := $(shell tput setaf 4 2>/dev/null || echo "")
C_MAG  := $(shell tput setaf 5 2>/dev/null || echo "")
C_CYN  := $(shell tput setaf 6 2>/dev/null || echo "")

# Icons 
IC_BUILD := BUILD
IC_CLEAN := CLEAN
IC_RESTORE := RESTORE
IC_TEST := TEST
IC_PACK := PACK
IC_RUN := RUN
IC_GEN := GEN
IC_INFO := INFO
IC_OK := OK
IC_WARN := WARN
IC_ERR := ERR
IC_FMT := FMT
IC_PUB := PUBLISH

hr = printf '$(C_DIM)%s$(C_RST)\n' '--------------------------------------------------------------------'
msg = printf '%s[%s]%s %s\n' '$(1)' '$(2)' '$(C_RST)' '$(3)'

.PHONY: help
help:
	@printf '\n$(C_CYN)$(C_BOLD)%s$(C_RST)\n' 'Thirdweb Tools'
	@$(hr)
	@printf 'Usage: $(C_BOLD)make$(C_RST) $(C_CYN)[target]$(C_RST)\n\n'
	@printf '$(C_BOLD)Targets:$(C_RST)\n'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'build' 'Generate API and build the solution'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'clean' 'Clean build artifacts'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'restore' 'Restore NuGet packages'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'test' 'Run tests'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'pack' 'Generate API (if needed) and create NuGet package'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'publish' 'Publish the Thirdweb project (dotnet publish)'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'run' 'Run the console application'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'generate' 'Generate API client from OpenAPI spec'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'generate-llms' 'Generate llms.txt from XML documentation'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'lint' 'Check code formatting (dry run)'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'fix' 'Fix code formatting issues'
	@printf '  $(C_CYN)%-12s$(C_RST) - %s\n' 'help' 'Show this help message'
	@$(hr)

.PHONY: publish
# Publish the Thirdweb library project
# Usage examples:
#   make publish                          # Release publish
#   make publish CONFIG=Debug              # Debug config
#   make publish RID=win-x64               # Target runtime
#   make publish OUT=artifacts/publish     # Custom output dir
publish:
	@if [ ! -f '$(API_CLIENT)' ]; then \
		$(call msg,$(C_YEL),$(IC_WARN),API client not found, generating it first) ; \
		$(MAKE) --no-print-directory generate ; \
	fi
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_PUB) Publishing Thirdweb project)
	@CMD="$(DOTNET) publish '$(LIB_PROJ)' -c '$(CONFIG)' -f '$(TFM)'"; \
	if [ -n "$(RID)" ]; then CMD="$$CMD -r '$(RID)'"; fi; \
	if [ -n "$(OUT)" ]; then CMD="$$CMD -o '$(OUT)'"; fi; \
	echo $$CMD; eval $$CMD && \
	$(call msg,$(C_GRN),$(IC_OK),Publish succeeded) || \
	$(call msg,$(C_RED),$(IC_ERR),Publish failed)

.PHONY: generate
# Clean previous file and generate API client
generate:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_GEN) Cleaning generated API files)
	@rm -f '$(API_CLIENT)' 2>/dev/null || true
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_GEN) Generating Thirdweb API client with custom generator)
	@$(DOTNET) run --project '$(GENERATOR_PROJ)' --no-build >/dev/null 2>&1 \
	|| ( \
		$(call msg,$(C_MAG),>> ,Building generator) ; \
		$(DOTNET) build '$(GENERATOR_PROJ)' ; \
		$(call msg,$(C_MAG),>> ,Running generator) ; \
		$(DOTNET) run --project '$(GENERATOR_PROJ)' \
	)
	@$(call msg,$(C_GRN),$(IC_OK),API client generation complete)

.PHONY: generate-llms
# Generate llms.txt from XML documentation
generate-llms:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_BUILD) Building Thirdweb in Release mode)
	@$(DOTNET) build '$(LIB_PROJ)' -c Release >/dev/null 2>&1 || { \
		$(call msg,$(C_MAG),>> ,Building Thirdweb project) ; \
		$(DOTNET) build '$(LIB_PROJ)' -c Release ; \
	}
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_GEN) Generating llms.txt from XML documentation)
	@$(DOTNET) run --project '$(GENERATOR_PROJ)' -- --llms && \
	$(call msg,$(C_GRN),$(IC_OK),llms.txt generation complete) || \
	$(call msg,$(C_RED),$(IC_ERR),llms.txt generation failed)

.PHONY: build
build:
	@$(MAKE) --no-print-directory generate
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_BUILD) Building with dotnet build)
	@$(DOTNET) build && \
	$(call msg,$(C_GRN),$(IC_OK),Build succeeded) || \
	$(call msg,$(C_RED),$(IC_ERR),Build failed)
	@$(MAKE) --no-print-directory generate-llms

.PHONY: clean
clean:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_CLEAN) Cleaning with dotnet clean)
	@$(DOTNET) clean && \
	$(call msg,$(C_GRN),$(IC_OK),Clean completed) || \
	$(call msg,$(C_RED),$(IC_ERR),Clean failed)

.PHONY: restore
restore:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_RESTORE) Restoring with dotnet restore)
	@$(DOTNET) restore && \
	$(call msg,$(C_GRN),$(IC_OK),Restore completed) || \
	$(call msg,$(C_RED),$(IC_ERR),Restore failed)

.PHONY: test
test:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_TEST) Running dotnet test)
	@$(DOTNET) test && \
	$(call msg,$(C_GRN),$(IC_OK),All tests passed) || \
	$(call msg,$(C_RED),$(IC_ERR),Some tests failed)

.PHONY: pack
pack:
	@if [ ! -f '$(API_CLIENT)' ]; then \
		$(call msg,$(C_YEL),$(IC_WARN),API client not found, generating it first) ; \
		$(MAKE) --no-print-directory generate ; \
	fi
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_BUILD) Building Release)
	@$(DOTNET) build --configuration Release || { $(call msg,$(C_RED),$(IC_ERR),Build (Release) failed); exit 1; }
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_PACK) Packing NuGet package(s))
	@$(DOTNET) pack --configuration Release && \
	$(call msg,$(C_GRN),$(IC_OK),Pack completed) || \
	$(call msg,$(C_RED),$(IC_ERR),Packing failed)

.PHONY: run
run:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_RUN) dotnet run --project $(CONSOLE_PROJ))
	@$(DOTNET) run --project '$(CONSOLE_PROJ)' && \
	$(call msg,$(C_GRN),$(IC_OK),Application exited) || \
	$(call msg,$(C_RED),$(IC_ERR),Application exited with errors)

.PHONY: lint
lint:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_FMT) Checking code formatting with CSharpier)
	@csharpier --help >/dev/null 2>&1 || { \
		$(call msg,$(C_YEL),$(IC_WARN),CSharpier is not installed) ; \
		printf '    Install it with: dotnet tool install -g csharpier\n' ; \
		exit 0 ; \
	}
	@csharpier check . >/dev/null 2>&1 || { \
		$(call msg,$(C_YEL),$(IC_WARN),Formatting issues found) ; \
		printf '    Run "make fix" to automatically fix them.\n' ; \
		exit 0 ; \
	}
	@$(call msg,$(C_GRN),$(IC_OK),Code formatting is correct)

.PHONY: fix
fix:
	@$(call msg,$(C_BLU),$(IC_INFO),$(IC_FMT) Running CSharpier formatter)
	@csharpier --help >/dev/null 2>&1 || { \
		$(call msg,$(C_YEL),$(IC_WARN),CSharpier is not installed) ; \
		printf '    Install it with: dotnet tool install -g csharpier\n' ; \
		exit 0 ; \
	}
	@csharpier format . >/dev/null 2>&1 || { \
		$(call msg,$(C_RED),$(IC_ERR),CSharpier formatting failed) ; \
		exit 1 ; \
	}
	@$(call msg,$(C_GRN),$(IC_OK),Code formatting completed)
