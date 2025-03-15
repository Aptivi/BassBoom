# Below is a workaround for .NET SDK 7.0 trying to allocate large amounts of memory for GC work:
# https://github.com/dotnet/runtime/issues/85556#issuecomment-1529177092
DOTNET_HEAP_LIMIT_INT = $(shell sysctl -n hw.memsize 2>/dev/null || grep MemAvailable /proc/meminfo | awk '{print $$2 * 1024}')
DOTNET_HEAP_LIMIT = $(shell printf '%X\n' $(DOTNET_HEAP_LIMIT_INT))

MODAPI = 2
ROOT_DIR := $(dir $(realpath $(lastword $(MAKEFILE_LIST))))
OUTPUTS  := \
	-name "bin" -or \
	-name "obj"

BINARIES = assets/bassboom
MANUALS = assets/bassboom.1
DESKTOPS = assets/bassboom.desktop
BRANDINGS = assets/OfficialAppIcon-AptiviApp-512.png

ARCH := $(shell if [ `uname -m` = "x86_64" ]; then echo "linux-x64"; else echo "linux-arm64"; fi)

ifndef DESTDIR
FDESTDIR := /usr/local
else
FDESTDIR := $(DESTDIR)/usr
endif

ifndef ENVIRONMENT
ENVIRONMENT := Release
endif

.PHONY: all install lite

# General use

all: all-online

all-online: invoke-build

dbg:
	$(MAKE) invoke-build ENVIRONMENT=Debug

dbg-ci:
	$(MAKE) invoke-build-ci ENVIRONMENT=Debug

rel-ci:
	$(MAKE) invoke-build-ci ENVIRONMENT=Release

doc: invoke-doc-build

clean:
	rm -rf $(OUTPUTS)

all-offline:
	$(MAKE) invoke-build-offline

init-offline:
	$(MAKE) invoke-init-offline

install:
	mkdir -m 755 -p $(FDESTDIR)/bin $(FDESTDIR)/lib/bassboom-$(MODAPI) $(FDESTDIR)/share/applications $(FDESTDIR)/share/man/man1/
	install -m 755 -t $(FDESTDIR)/bin/ $(BINARIES)
	install -m 644 -t $(FDESTDIR)/share/man/man1/ $(MANUALS)
	find private/BassBoom.Cli/bin/$(ENVIRONMENT)/net8.0 -mindepth 1 -type d -exec sh -c 'mkdir -p -m 755 "$(FDESTDIR)/lib/bassboom-$(MODAPI)/$$(realpath --relative-to private/BassBoom.Cli/bin/$(ENVIRONMENT)/net8.0 "$$0")"' {} \;
	find private/BassBoom.Cli/bin/$(ENVIRONMENT)/net8.0 -mindepth 1 -type f -exec sh -c 'install -m 644 -t "$(FDESTDIR)/lib/bassboom-$(MODAPI)/$$(dirname $$(realpath --relative-to private/BassBoom.Cli/bin/$(ENVIRONMENT)/net8.0 "$$0"))" "$$0"' {} \;
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
	chmod +x ./tools/build.sh
	./tools/build.sh $(ENVIRONMENT) || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./tools/build.sh $(ENVIRONMENT))

invoke-build-ci:
	chmod +x ./tools/build.sh
	./tools/build.sh $(ENVIRONMENT) -p:ContinuousIntegrationBuild=true || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./tools/build.sh $(ENVIRONMENT) -p:ContinuousIntegrationBuild=true)
    
invoke-doc-build:
	chmod +x ./tools/docgen.sh
	./tools/docgen.sh || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./tools/docgen.sh)

invoke-build-offline:
	chmod +x ./tools/build.sh
	HOME=`pwd`"/debian/homedir" ./tools/build.sh Release -p:ApplicationFlags=PACKAGEMANAGERBUILD -p:ContinuousIntegrationBuild=true || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) HOME=`pwd`"/debian/homedir" ./tools/build.sh Release -p:ApplicationFlags=PACKAGEMANAGERBUILD -p:ContinuousIntegrationBuild=true)

invoke-init-offline:
	chmod +x ./vnd/initializeoffline.sh
	./vnd/initializeoffline.sh || (echo Retrying with heap limit 0x$(DOTNET_HEAP_LIMIT)... && DOTNET_GCHeapHardLimit=$(DOTNET_HEAP_LIMIT) ./vnd/initializeoffline.sh)
