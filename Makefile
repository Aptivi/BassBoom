# Below is a workaround for .NET SDK 7.0 trying to allocate large amounts of memory for GC work:
# https://github.com/dotnet/runtime/issues/85556#issuecomment-1529177092
DOTNET_HEAP_LIMIT_INT = $(shell sysctl -n hw.memsize 2>/dev/null || grep MemAvailable /proc/meminfo | awk '{print $$2 * 1024}')
DOTNET_HEAP_LIMIT = $(shell printf '%X\n' $(DOTNET_HEAP_LIMIT_INT))

MODAPI = 3
ROOT_DIR := $(shell dirname "$(realpath $(lastword $(MAKEFILE_LIST)))")
OUTPUTS  := \
	-name "bin" -or \
	-name "obj" -or \
	-name "bassboom-$(MODAPI)" -or \
	-name "tmp"

OUTPUT = "$(ROOT_DIR)/private/BassBoom.Cli/bin/$(ENVIRONMENT)/net8.0"
BINARIES = "$(ROOT_DIR)/assets/bassboom"
MANUALS = "$(ROOT_DIR)/assets/bassboom.1"
DESKTOPS = "$(ROOT_DIR)/assets/bassboom.desktop"
BRANDINGS = "$(ROOT_DIR)/assets/OfficialAppIcon-BassBoom-512.png"

ARCH := $(shell if [ `uname -m` = "x86_64" ]; then echo "linux-x64"; else echo "linux-arm64"; fi)

ifndef DESTDIR
FDESTDIR := /usr/local
else
FDESTDIR := $(DESTDIR)/usr
endif

.PHONY: all install lite

# General use

all:
	$(MAKE) all-online BUILDARGS="$(BUILDARGS)"

all-online:
	$(MAKE) invoke-build ENVIRONMENT=Release BUILDARGS="$(BUILDARGS)"

dbg:
	$(MAKE) invoke-build ENVIRONMENT=Debug BUILDARGS="$(BUILDARGS)"

dbg-ci:
	$(MAKE) invoke-build ENVIRONMENT=Debug BUILDARGS="-p:ContinuousIntegrationBuild=true $(BUILDARGS)"

rel-ci:
	$(MAKE) invoke-build ENVIRONMENT=Release BUILDARGS="-p:ContinuousIntegrationBuild=true $(BUILDARGS)"

doc: invoke-doc-build

clean:
	find "$(ROOT_DIR)" -type d \( $(OUTPUTS) \) -print -exec rm -rf "{}" +

all-offline:
	$(MAKE) invoke-build-offline BUILDARGS="-p:ContinuousIntegrationBuild=true $(BUILDARGS)"

init-offline:
	$(MAKE) invoke-init-offline

install:
	mkdir -m 755 -p $(FDESTDIR)/bin $(FDESTDIR)/lib/bassboom-$(MODAPI) $(FDESTDIR)/share/applications $(FDESTDIR)/share/man/man1/
	install -m 755 -t $(FDESTDIR)/bin/ $(BINARIES)
	install -m 644 -t $(FDESTDIR)/share/man/man1/ $(MANUALS)
	find "$(OUTPUT)" -mindepth 1 -type d -exec sh -c 'mkdir -p -m 755 "$(FDESTDIR)/lib/bassboom-$(MODAPI)/$$(realpath --relative-to "$(OUTPUT)" "$$0")"' {} \;
	find "$(OUTPUT)" -mindepth 1 -type f -exec sh -c 'install -m 644 -t "$(FDESTDIR)/lib/bassboom-$(MODAPI)/$$(dirname $$(realpath --relative-to "$(OUTPUT)" "$$0"))" "$$0"' {} \;
	install -m 755 -t $(FDESTDIR)/share/applications/ $(DESKTOPS)
	install -m 755 -t $(FDESTDIR)/lib/bassboom-$(MODAPI)/ $(BRANDINGS)
	mv $(FDESTDIR)/bin/bassboom $(FDESTDIR)/bin/bassboom-$(MODAPI)
	mv $(FDESTDIR)/share/man/man1/bassboom.1 $(FDESTDIR)/share/man/man1/bassboom-$(MODAPI).1
	mv $(FDESTDIR)/share/applications/bassboom.desktop $(FDESTDIR)/share/applications/bassboom-$(MODAPI).desktop
	sed -i 's|/usr/lib/bassboom|/usr/lib/bassboom-$(MODAPI)|g' $(FDESTDIR)/bin/bassboom-*
	sed -i 's|/usr/lib/bassboom|/usr/lib/bassboom-$(MODAPI)|g' $(FDESTDIR)/share/applications/bassboom-$(MODAPI).desktop
	sed -i 's|/usr/bin/bassboom|/usr/bin/bassboom-$(MODAPI)|g' $(FDESTDIR)/share/applications/bassboom-$(MODAPI).desktop
	find '$(FDESTDIR)/lib/' -type d -name "runtimes" -exec sh -c 'find $$0 -mindepth 1 -maxdepth 1 -not -name $(ARCH) -type d -exec rm -rf \{\} \;' {} \;

# Below targets specify functions for full build

invoke-build:
	./tools/build.sh "$(ENVIRONMENT)" $(BUILDARGS) || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./tools/build.sh "$(ENVIRONMENT)" $(BUILDARGS))
    
invoke-doc-build:
	./tools/docgen.sh || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./tools/docgen.sh)

invoke-build-offline:
	HOME=`pwd`"/debian/homedir" ./tools/build.sh Release $(BUILDARGS) || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) HOME=`pwd`"/debian/homedir" ./tools/build.sh Release $(BUILDARGS))

invoke-init-offline:
	./tools/localize.sh || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./tools/localize.sh)
